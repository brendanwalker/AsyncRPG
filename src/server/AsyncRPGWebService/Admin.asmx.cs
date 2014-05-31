using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using AsyncRPGWebService.Environment;
using AsyncRPGSharedLib.Web.Modules;

namespace AsyncRPGWebService
{
    /// <summary>
    /// Summary description for Admin
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Admin : System.Web.Services.WebService
    {
        static Admin()
        {
            WebUtilities.InitializeConstants();
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string SetAccountOpsLevel(int account_id, int ops_level)
        {
            return
                new AdminModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .SetAccountOpsLevel(account_id, ops_level);
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string DebugClearCachedWorld(int game_id)
        {
            return
                new AdminModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .DebugClearCachedWorld(game_id);
        }

        //[WebMethod(EnableSession = true)]
        //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        //public string UpgradeDatabase()
        //{
            //return
            //    new AdminModule(
            //        new HttpCacheAdapter(Application),
            //        new HttpSessionAdapter(Session),
            //        new HttpResponseAdapter(Context.Response))
            //    .UpgradeDatabase();
        //}

        [WebMethod(EnableSession = true)]
        public string DumpRoomTemplateReport()
        {
            return
                new AdminModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .DumpRoomTemplateReport();
        }
    }
}
