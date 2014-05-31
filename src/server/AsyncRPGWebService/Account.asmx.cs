using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using AsyncRPGWebService.Environment;
using AsyncRPGSharedLib.Web.Modules;

namespace AsyncRPGWebService
{
    /// <summary>
    /// Summary description for Account
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Account : System.Web.Services.WebService
    {
        static Account()
        {
            WebUtilities.InitializeConstants();
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string AccountCreateRequest(string username, string password, string emailAddress)
        {
            return
                new AccountModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .AccountCreateRequest(username, password, emailAddress);
        }

        [WebMethod(EnableSession = true)]
        public string AccountEmailVerifyRequest(string username, string key)
        {
            return
                new AccountModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .AccountEmailVerifyRequest(username, key);
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string AccountDeleteRequest(string username)
        {
            return
                new AccountModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .AccountDeleteRequest(username);
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string AccountEmailChangeRequest(string username, string newEmailAddress)
        {
            return
                new AccountModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .AccountEmailChangeRequest(username, newEmailAddress);
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string AccountChangePasswordRequest(string username, string oldPassword, string newPassword)
        {
            return
                new AccountModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .AccountChangePasswordRequest(username, oldPassword, newPassword);
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string AccountResetPasswordRequest(string username)
        {
            return
                new AccountModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .AccountResetPasswordRequest(username);
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string AccountLoginRequest(string username, string password)
        {
            return
                new AccountModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .AccountLoginRequest(username, password);
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string AccountPlainTextLoginRequest(string username, string password)
        {
            return
                new AccountModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .AccountPlainTextLoginRequest(username, password);
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string AccountLogoutRequest(string username)
        {
            return
                new AccountModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .AccountLogoutRequest(username);
        }
    }
}
