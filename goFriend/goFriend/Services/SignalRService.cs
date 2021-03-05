using goFriend.DataModel;
using goFriend.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace goFriend.Services
{
    public class SignalRService
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());
        private readonly HttpClient httpClient;
        private HubConnection hubConnection;
        private ConcurrentDictionary<int, FriendLocation> _lastFriendLocations = new ConcurrentDictionary<int, FriendLocation>();

        public bool IsConnected { get; private set; }
        public bool IsBusy { get; private set; }

        public SignalRService()
        {
            httpClient = new HttpClient();
        }

        private HttpRequestMessage BuildRequest(string msgType, HttpMethod method, object msgContent = null)
        {
            HttpRequestMessage request;
            if (msgContent == null)
            {
                request = new HttpRequestMessage
                {
                    RequestUri = new Uri($"{Constants.ChatFuncUrl}/api/{msgType}"),
                    Method = method,
                    Headers = {
                        { "x-ms-client-principal-id", App.User.Id.ToString() },
                        { "Authorization", App.User.Token.ToString()}
                    }
                };
            }
            else
            {
                var json = JsonConvert.SerializeObject(msgContent);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                request = new HttpRequestMessage
                {
                    RequestUri = new Uri($"{Constants.ChatFuncUrl}/api/{msgType}"),
                    Method = method,
                    Headers = {
                        { "x-ms-client-principal-id", App.User.Id.ToString() },
                        { "Authorization", App.User.Token.ToString()}
                    },
                    Content = content
                };
            }

            return request;
        }

        public async Task<TResult> SendMessageAsync<TResult>(string msgType, object msgContent = null)
        {
            TResult result = default;
            await ConnectAsync();
            var stopWatch = Stopwatch.StartNew();
            try
            {
                Logger.Debug($"SendMessageAsync.BEGIN(msgType={msgType})");
                IsBusy = true;

                var request = BuildRequest(msgType, HttpMethod.Post, msgContent);

                //var result = await client.PostAsync($"{Constants.HostName}/api/talk", content);
                var responseMessage = await httpClient.SendAsync(request);
                var resultJson = await responseMessage.Content.ReadAsStringAsync();
                Logger.Debug($"resultJson={resultJson}");

                result = JsonConvert.DeserializeObject<TResult>(resultJson);

                IsBusy = false;
                return result;
            }
            catch (Exception e)
            {
                Logger.TrackError(e);
                return result;
            }
            finally
            {
                Logger.Debug($"SendMessageAsync.END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        public async Task StopAsync()
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                Logger.Debug($"StopAsync.BEGIN");

                if (hubConnection != null)
                {
                    Logger.Debug($"Disconnecting...");
                    hubConnection.Reconnected -= HubConnectionOnReconnected;
                    hubConnection.Closed -= HubConnectionOnClosed;
                    await hubConnection.StopAsync();
                    hubConnection = null;
                }
                else
                {
                    Logger.Debug($"hubConnection null");
                }
            }
            catch (Exception e)
            {
                Logger.TrackError(e);
            }
            finally
            {
                Logger.Debug($"StopAsync.END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        public async Task ConnectAsync()
        {
            if (!App.IsUserLoggedIn || App.User == null)
            {
                throw new Exception("User is not logged in.");
            }
            if (hubConnection != null)
            {
                if (hubConnection.State == HubConnectionState.Connected)
                {
                    Logger.Debug("Hub connected. Do nothing.");
                    return;
                }
                else if (hubConnection.State == HubConnectionState.Connecting || hubConnection.State == HubConnectionState.Reconnecting)
                {
                    Logger.Debug($"hubConnection.State = {hubConnection.State}");
                    throw new Exception("Connecting to the Hub. Wait for automatic connection to be completed");
                }
            }
            var stopWatch = Stopwatch.StartNew();
            try
            {
                Logger.Debug($"ConnectAsync.BEGIN");

                IsBusy = true;
                var request = BuildRequest("negotiate", HttpMethod.Get);

                var responseMessage = await httpClient.SendAsync(request);
                string negotiateJson = await responseMessage.Content.ReadAsStringAsync();
                Logger.Debug($"negotiateJson={negotiateJson}");
                var negotiate = JsonConvert.DeserializeObject<NegotiateInfo>(negotiateJson);

                hubConnection = new HubConnectionBuilder()
                    .AddNewtonsoftJsonProtocol()
                    .WithUrl(negotiate.Url, options =>
                    {
                        options.AccessTokenProvider = async () => negotiate.AccessToken;
                    })
                    .Build();

                hubConnection.Reconnected += HubConnectionOnReconnected;
                hubConnection.Closed += HubConnectionOnClosed;
                hubConnection.On<ChatMessage>(ChatMessageType.Text.ToString(), OnChatReceiveMessage);
                hubConnection.On<ChatMessage>(ChatMessageType.Attachment.ToString(), OnChatReceiveAttachment);
                hubConnection.On<FriendLocation>(ChatMessageType.Location.ToString(), OnReceiveLocation);
                hubConnection.On<Chat>(ChatMessageType.CreateChat.ToString(), OnChatReceiveCreateChat);
                await hubConnection.StartAsync();

                IsConnected = true;
                IsBusy = false;

                Logger.Debug("Connected. Sending join chat...");
                _ = SendMessageAsync<string>(ChatMessageType.JoinChat.ToString());
            }
            catch (Exception e)
            {
                IsConnected = false;
                IsBusy = false;
                Logger.TrackError(e);
                throw;
            }
            finally
            {
                Logger.Debug($"ConnectAsync.END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        private Task HubConnectionOnReconnected(string arg)
        {
            Logger.Debug($"HubConnectionOnReconnected(arg={arg})");
            //return App.JoinAllChats();
            return Task.CompletedTask;
        }

        private async Task HubConnectionOnClosed(Exception arg)
        {
            IsConnected = false;
            IsBusy = false;
            Logger.Debug($"OnClosed(exception={arg})");
            Logger.Debug("Waiting for 5 seconds before rejoining the chat");
            await Task.Delay(TimeSpan.FromSeconds(5));
            try
            {
                await ConnectAsync();
            }
            catch { }
        }

        private async Task OnChatReceiveCreateChat(Chat chat)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                Logger.Debug($"OnChatReceiveCreateChat.BEGIN");

                Logger.Debug($"chat={JsonConvert.SerializeObject(chat)})");
                await App.ChatListVm.ReceiveCreateChat(chat);
            }
            catch (Exception e) //Unknown error
            {
                Logger.TrackError(e);
            }
            finally
            {
                Logger.Debug($"OnChatReceiveCreateChat.END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        private void OnChatReceiveAttachment(ChatMessage chatMessage)
        {
            Logger.Debug($"OnChatReceiveAttachment.BEGIN(MessageType={chatMessage.MessageType})");
            OnChatReceiveMessage(chatMessage);
            Logger.Debug($"OnChatReceiveAttachment.END");
        }

        private void OnChatReceiveMessage(ChatMessage chatMessage)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                Logger.Debug($"OnChatReceiveMessage.BEGIN(MessageType={chatMessage.MessageType})");

                Logger.Debug($"chatMessage={JsonConvert.SerializeObject(chatMessage)})");
                switch (chatMessage.MessageType)
                {
                    case ChatMessageType.Text:
                    case ChatMessageType.Attachment:
                        if (App.ChatListVm.ChatListItems.Any(x => x.Chat.Id == chatMessage.ChatId))
                        {
                            Logger.Debug("Chat found. Message added.");
                            App.ChatListVm.ChatListItems.Single(x => x.Chat.Id == chatMessage.ChatId).ChatViewModel.ReceiveMessage(chatMessage);
                        }
                        break;
                    default:
                        Logger.Error("Type of message not supported.");
                        break;
                }
            }
            catch (Exception e) //Unknown error
            {
                Logger.TrackError(e);
            }
            finally
            {
                Logger.Debug($"OnChatReceiveMessage.END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        private void OnReceiveLocation(FriendLocation friendLocation)
        {
            if (Views.MapOnlinePage.Instance == null) return;
            var stopWatch = Stopwatch.StartNew();
            try
            {
                Logger.Debug($"OnReceiveLocation.BEGIN(FriendId={friendLocation.FriendId}, SharingInfo={friendLocation.SharingInfo})");
                if (friendLocation.SharingInfo == null) return;
                if (!App.IsUserLoggedIn || App.User == null)
                {
                    Logger.Warn("Reiceiving location while logged out");
                    return;
                }
                friendLocation.ModifiedDate = DateTime.Now;
                if (friendLocation.FriendId == App.User.Id) //receive my own location. Stored to use in distance calculation
                {
                    friendLocation.Friend = App.User;
                    Views.MapOnlinePage.MyLocation = friendLocation;

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Views.MapOnlinePage.Instance.RecenterMap();//First time receiving Location, recenter the map
                    });
                }
                if (_lastFriendLocations.Any(
                    x => x.Key == friendLocation.FriendId && x.Value.Location == friendLocation.Location && !x.Value.IsRefreshNeeded()))
                {
                    // the case when some one has more than one common group as you have
                    // his/her location is sent to you more than one times, so ignore the duplicate ones.
                    // receive the duplicated one only if the old one need to be refreshed
                }
                else
                {
                    _lastFriendLocations[friendLocation.FriendId] = friendLocation;
                    Views.MapOnlinePage.Instance.ReceiveLocation(friendLocation);
                }
            }
            catch (Exception e) //Unknown error
            {
                Logger.TrackError(e);
            }
            finally
            {
                Logger.Debug($"OnReceiveLocation.END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }
    }
}
