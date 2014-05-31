using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using AsyncRPGWebService.Environment;
using AsyncRPGSharedLib.Web.Modules;

namespace AsyncRPGWebService
{
    /// <summary>
    /// Summary description for Character
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Character : System.Web.Services.WebService
    {
        static Character()
        {
            WebUtilities.InitializeConstants();
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string CreateCharacter(
            string name,
            int archetype,
            int gender,
            int picture_id)
        {
            return
                new CharacterModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .CreateCharacter(name, archetype, gender, picture_id);
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetCharacterFullState(
            int character_id)
        {
            return
                new CharacterModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .GetCharacterFullState(character_id);
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetCharacterList(
            string username)
        {
            return
                new CharacterModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .GetCharacterList(username);
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string DeleteCharacter(int character_id)
        {
            return
                new CharacterModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .DeleteCharacter(character_id);
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string PingCharacter(int character_id)
        {
            return
                new CharacterModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .PingCharacter(character_id);
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string MoveCharacter(
            int character_id,
            float x,
            float y,
            float z,
            float angle)
        {
            return
                new CharacterModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .MoveCharacter(character_id, x, y, z, angle);
        }
        
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string PortalCharacter(
            int character_id,
            float x,
            float y,
            float z,
            float angle,
            int portal_id)
        {
            return
                new CharacterModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .PortalCharacter(character_id, x, y, z, angle, portal_id);
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetCharacterPosition(
            int character_id)
        {
            return
                new CharacterModule(
                    new HttpCacheAdapter(Application),
                    new HttpSessionAdapter(Session),
                    new HttpResponseAdapter(Context.Response))
                .GetCharacterPosition(character_id);
        }
    }
}
