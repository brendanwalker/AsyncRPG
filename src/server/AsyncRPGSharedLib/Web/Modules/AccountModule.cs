using System;
using System.Security.Cryptography;
using AsyncRPGSharedLib.RequestProcessors;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Web.Modules;
using AsyncRPGSharedLib.Web.Cache;
using AsyncRPGSharedLib.Web.Interfaces;
using AsyncRPGSharedLib.Protocol;
using AsyncRPGSharedLib.Queries;
using AsyncRPGSharedLib.Utility;

namespace AsyncRPGSharedLib.Web.Modules
{
    [RestModuleName("Account")]
    public class AccountModule : RestModule
    {
        public AccountModule() : base()
        {
        }

        public AccountModule(
            ICacheAdapter appCache,
            ISessionAdapter session,
            IResponseAdapter response) : 
            base(appCache, session, response)
        {
        }

        public string AccountCreateRequest(string username, string password, string emailAddress)
        {

            BasicResponse response = new BasicResponse();

            string webServiceURL =
                (ApplicationConstants.IsDebuggingEnabled)
                    ? ApplicationConstants.ACCOUNT_DEBUG_WEB_SERVICE_URL
                    : ApplicationConstants.ACCOUNT_WEB_SERVICE_URL;

            CreateAccountRequestProcessor requestProcessor =
                new CreateAccountRequestProcessor(
                    username,
                    password,
                    emailAddress,
                    webServiceURL);

            requestProcessor.ProcessRequest(
                    ApplicationConstants.CONNECTION_STRING,
                    Application,
                    out response.result);

            return JSONUtilities.SerializeJSONResponse<BasicResponse>(response);
        }

        public string AccountEmailVerifyRequest(string username, string key)
        {
            string result = ErrorMessages.GENERAL_ERROR;

            if (AccountQueries.VerifiedEmailAddress(
                    ApplicationConstants.CONNECTION_STRING,
                    username,
                    key,
                    out result))
            {
                if (!ApplicationConstants.IsDebuggingEnabled)
                {
                    Response.AddHeader("Location", ApplicationConstants.ACCOUNT_CLIENT_URL);
                    Response.StatusCode = 307;
                }

                result = SuccessMessages.GENERAL_SUCCESS;
            }

            return result;
        }

        public string AccountDeleteRequest(string username)
        {
            BasicResponse response = new BasicResponse();

            //TODO: Implement AccountDeleteRequest

            return JSONUtilities.SerializeJSONResponse<BasicResponse>(response);
        }

        public string AccountEmailChangeRequest(string username, string newEmailAddress)
        {
            BasicResponse response = new BasicResponse();

            //TODO Implement Email change request

            return JSONUtilities.SerializeJSONResponse<BasicResponse>(response);
        }

        public string AccountChangePasswordRequest(string username, string oldPassword, string newPassword)
        {
            BasicResponse response = new BasicResponse();

            //TODO Implement account change password request

            return JSONUtilities.SerializeJSONResponse<BasicResponse>(response);
        }

        public string AccountResetPasswordRequest(string username)
        {
            BasicResponse response = new BasicResponse();

            //TODO Implement reset password request

            return JSONUtilities.SerializeJSONResponse<BasicResponse>(response);
        }

        public string AccountLoginRequest(string username, string password)
        {
            BasicResponse loginResponse = new BasicResponse();

            if (RestUtilities.GetSessionUsername(Session) == null)
            {
                // Initially, we are not authenticated yet
                Session["Authenticated"] = false;

                AccountLoginRequestProcessor loginProcessor =
                    new AccountLoginRequestProcessor(username, password);

                if (loginProcessor.ProcessRequest(
                        ApplicationConstants.CONNECTION_STRING,
                        this.Application,
                        out loginResponse.result))
                {
                    Session["Authenticated"] = true;
                    Session["AccountId"] = loginProcessor.AccountID;
                    Session["OpsLevel"] = loginProcessor.OpsLevel;
                    Session["EmailAddress"] = loginProcessor.EmailAddress;
                    Session["CharacterIDs"] = loginProcessor.AccountCharacterIDs;
                    Session["Username"] = username;

                    loginResponse.result = SuccessMessages.GENERAL_SUCCESS;
                }
            }
            else
            {
                loginResponse.result = ErrorMessages.ALREADY_LOGGED_IN;
            }

            return JSONUtilities.SerializeJSONResponse<BasicResponse>(loginResponse);
        }

        public string AccountPlainTextLoginRequest(string username, string password)
        {
            BasicResponse loginResponse = new BasicResponse();

            if (RestUtilities.GetSessionUsername(Session) == null)
            {
                // Initially, we are not authenticated yet
                Session["Authenticated"] = false;

                if (username.Length > 0 && password.Length > 0)
                {
                    string hashedPassword = AccountQueries.ClientPasswordHash(password);

                    AccountLoginRequestProcessor loginProcessor =
                        new AccountLoginRequestProcessor(username, hashedPassword);

                    if (loginProcessor.ProcessRequest(
                            ApplicationConstants.CONNECTION_STRING,
                            Application,
                            out loginResponse.result))
                    {
                        Session["Authenticated"] = true;
                        Session["AccountId"] = loginProcessor.AccountID;
                        Session["OpsLevel"] = loginProcessor.OpsLevel;
                        Session["EmailAddress"] = loginProcessor.EmailAddress;
                        Session["CharacterIDs"] = loginProcessor.AccountCharacterIDs;
                        Session["Username"] = username;

                        loginResponse.result = SuccessMessages.GENERAL_SUCCESS;
                    }
                }
                else
                {
                    loginResponse.result = ErrorMessages.MALFORMED_REQUEST;
                }
            }
            else
            {
                loginResponse.result = ErrorMessages.ALREADY_LOGGED_IN;
            }

            return JSONUtilities.SerializeJSONResponse<BasicResponse>(loginResponse);
        }

        public string AccountLogoutRequest(string username)
        {
            BasicResponse logoutResponse = new BasicResponse();

            if (RestUtilities.ValidateJSONRequestHasAuthenticatedSession(Session, out logoutResponse.result) &&
                RestUtilities.ValidateJSONRequestSessionLoggedInAsUser(Session, username, out logoutResponse.result))
            {
                logoutResponse.result = SuccessMessages.GENERAL_SUCCESS;
                Session.Abandon();
            }

            return JSONUtilities.SerializeJSONResponse<BasicResponse>(logoutResponse);
        }
    }
}
