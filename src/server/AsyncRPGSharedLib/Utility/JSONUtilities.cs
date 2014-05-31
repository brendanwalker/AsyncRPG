using AsyncRPGSharedLib.Protocol;
using LitJson;

namespace AsyncRPGSharedLib.Utility
{
    public class JSONUtilities
    {
        public static string SerializeJSONResponse<T>(T jsonObject) where T : JSONResponse
        {
            return JsonMapper.ToJson(jsonObject);
        }
    }
}
