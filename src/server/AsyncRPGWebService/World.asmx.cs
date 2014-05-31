using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using AsyncRPGWebService.Environment;
using AsyncRPGSharedLib.Web.Modules;

namespace AsyncRPGWebService
{
    /// <summary>
    /// Summary description for World
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WorldService : System.Web.Services.WebService
    {
        static WorldService()
        {
            WebUtilities.InitializeConstants();
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string WorldGetFullGameStateRequest(
            int character_id)
        {
            return
                new WorldModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .WorldGetFullGameStateRequest(character_id);
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetRoomData(
            int game_id,
            int room_x,
            int room_y,
            int room_z)
        {
            return
                new WorldModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .GetRoomData(game_id, room_x, room_y, room_z);
        }
    }
}
