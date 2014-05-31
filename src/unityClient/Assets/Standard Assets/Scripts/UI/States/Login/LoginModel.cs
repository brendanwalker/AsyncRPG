using LitJson;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class LoginModel
{
    public bool LoginRequestPending { get; set; }

    private LoginController m_loginController;

    public LoginModel(LoginController loginController)
    {
        SessionData sessionData = SessionData.GetInstance();

        m_loginController = loginController;
        LoginRequestPending = false;

        sessionData.Reset();
    }

    public void RequestLogin(string uname, string password)
    {
        // Hash the password before sending over the wire
        string encryptedPasswordString = ClientUtilities.HashPassword(password);

        LoginRequestPending = true;

        {
            // Allocate an async request on the login controller
            AsyncJSONRequest loginRequest = AsyncJSONRequest.Create(m_loginController.gameObject);
            Dictionary<string, object> requestParameters = new Dictionary<string, object>();

            requestParameters["username"] = uname;
            requestParameters["password"] = encryptedPasswordString;

            loginRequest.POST(
                ServerConstants.loginRequestURL,
                requestParameters,
                this.OnLoginRequestComplete);
        }
    }

    private void OnLoginRequestComplete(AsyncJSONRequest asyncRequest)
    {
        if (asyncRequest.GetRequestState() == AsyncJSONRequest.eRequestState.succeded)
        {
            JsonData loginResponse = asyncRequest.GetResult();
            Dictionary<string, string> responseHeaders= asyncRequest.GetLoader().responseHeaders;

            if ((string)loginResponse["result"] == "Success")
            {
                SessionData sessionData = SessionData.GetInstance();

                sessionData.Authenticated = true;					
                sessionData.UserName = (string)asyncRequest.GetRequestData()["username"];

                // Save the authentication cookie, if any was sent
                if (responseHeaders.ContainsKey("SET-COOKIE"))
                {
                    sessionData.Cookie = responseHeaders["SET-COOKIE"];
                }

                m_loginController.OnLoginSucceeded();
            }
            else 
            {
                m_loginController.OnLoginFailed((string)loginResponse["result"]);
            }
        }
        else 
        {
            m_loginController.OnLoginFailed("Connection Failure!");
            Debug.LogError("Login Failed: " + asyncRequest.GetFailureReason());
        }

        // Free the request now that we're done with it
        AsyncJSONRequest.Destroy(asyncRequest);
        asyncRequest = null;

        LoginRequestPending = false;
    }	
}
