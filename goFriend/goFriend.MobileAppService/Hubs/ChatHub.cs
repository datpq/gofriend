using System;
using System.Threading.Tasks;
using goFriend.DataModel;
using Microsoft.AspNetCore.SignalR;
using NLog;

namespace goFriend.MobileAppService.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public async Task Ping()
        {
            try
            {
                Logger.Debug("Ping.BEGIN");
                await Clients.Client(Context.ConnectionId).SendAsync(ChatMessageType.Ping.ToString(), "reply from server OK");
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                Logger.Debug("Ping.END");
            }
        }

        public async Task SendMessage(ChatMessage chatMessage)
        {
            try
            {
                Logger.Debug($"SendMessage.BEGIN(ChatId={chatMessage.ChatId}, MessageType={chatMessage.MessageType}, Message={chatMessage.Message}, OwnerName={chatMessage.OwnerName})");
                chatMessage.Time = DateTime.Now;
                await Clients.All.SendAsync(chatMessage.MessageType.ToString(), chatMessage);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                Logger.Debug("SendMessage.END");
            }
        }

        public void Echo(string user, string message)
        {
            Logger.Debug($"BEGIN(user={user}, message={message})");

            Clients.Client(Context.ConnectionId).SendAsync("echo", user, message + " (echo from server)");

            Logger.Debug("END");
        }
    }
}
