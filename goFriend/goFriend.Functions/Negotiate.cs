using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;

namespace goFriend.Functions
{
    public class Negotiate
    {
        [FunctionName("Negotiate")]
        public SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous,"get",Route = "negotiate")]
            HttpRequest req,
            [SignalRConnectionInfo(HubName = Constants.SignalRHubName, UserId = "{headers.x-ms-client-principal-id}")]
            SignalRConnectionInfo connectionInfo,
            ILogger log)
        {
            log.LogDebug("Negotiate.BEGIN");

            req.ParseSignalRHeaders(out string UserId, out string token);
            log.LogDebug($"UserId={UserId}, Authorization={token}");

            //var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
            //    new Claim(ClaimTypes.Role, "Administrator"),
            //    new Claim(ClaimTypes.NameIdentifier, UserId)
            //}));
            //SignalRConnectionInfoAttribute attribute = new SignalRConnectionInfoAttribute
            //{
            //    HubName = "goFriendChat",
            //    UserId = UserId
            //};
            //SignalRConnectionInfo connection = await binder.BindAsync<SignalRConnectionInfo>(attribute);

            log.LogDebug("Negotiate.END");
            return connectionInfo;
        }
    }
}
