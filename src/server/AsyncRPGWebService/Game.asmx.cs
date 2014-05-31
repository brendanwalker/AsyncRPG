using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using AsyncRPGWebService.Environment;
using AsyncRPGSharedLib.Web.Modules;

namespace AsyncRPGWebService
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class GameService : System.Web.Services.WebService
    {
        static GameService()
        {
            WebUtilities.InitializeConstants();
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string CreateGame(
            string game_name,
            int dungeon_size,
            int dungeon_difficulty,
            bool irc_enabled,
            string irc_server,
            int irc_port,
            bool irc_encryption_enabled)
        {
            return
                new GameModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .CreateGame(
                    game_name,
                    dungeon_size,
                    dungeon_difficulty,
                    irc_enabled,
                    irc_server,
                    irc_port,
                    irc_encryption_enabled);
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string DeleteGame(int game_id)
        {
            return
                new GameModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .DeleteGame(game_id);
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetGameList()
        {
            return
                new GameModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .GetGameList();
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string BindCharacterToGame(
            int character_id,
            int game_id)
        {
            return
                new GameModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .BindCharacterToGame(character_id, game_id);
        }
    }
}