using Microsoft.AspNetCore.Http;

namespace goFriend.Functions
{
    public static class Extension
    {
        public static void ParseSignalRHeaders(this HttpRequest req, out string UserId, out string token)
        {
            UserId = req.Headers["x-ms-client-principal-id"];
            token = req.Headers["Authorization"];
        }
    }
}
