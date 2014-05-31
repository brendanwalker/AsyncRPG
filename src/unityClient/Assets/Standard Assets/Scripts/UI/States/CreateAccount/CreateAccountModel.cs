using LitJson;
using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using System.Collections.Generic;

public class CreateAccountModel
{
    public string Status { get; set; }
    public bool IsCreateAccountRequestPending { get; private set; }

    private CreateAccountController m_createAccountController;

    public CreateAccountModel(CreateAccountController createAccountController)
    {
        m_createAccountController = createAccountController;
    }

	public void RequestCreateAccount(
        string uname, 
        string password, 
        string email)
	{
		string encryptedPassword= ClientUtilities.HashPassword(password);
			
		IsCreateAccountRequestPending = true;

        Dictionary<string, object> requestParameters = new Dictionary<string, object>();
        requestParameters["username"] = uname;
        requestParameters["password"] = encryptedPassword;
        requestParameters["emailAddress"] = email;

		AsyncJSONRequest createAccountRequest = AsyncJSONRequest.Create(m_createAccountController.gameObject);

        createAccountRequest.POST(
            ServerConstants.createAccountRequestURL,
            requestParameters,
            (AsyncJSONRequest request) =>
		{
			if (request.GetRequestState() == AsyncJSONRequest.eRequestState.succeded)
			{
				JsonData createAccountResponse = request.GetResult();
                string result= (string)createAccountResponse["result"];
					
				if (result.StartsWith("Success"))
				{
					m_createAccountController.OnCreateAccountSucceeded(result);
				}
				else 
				{
                    m_createAccountController.OnCreateAccountFailed(result);
				}
			}
			else 
			{
				m_createAccountController.OnCreateAccountFailed("Connection Failure!");
				Debug.LogError("Create Account Failed: " + request.GetFailureReason());
			}
				
			IsCreateAccountRequestPending = false;
		});							
	}
}
