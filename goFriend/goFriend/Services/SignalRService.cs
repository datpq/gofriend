using goFriend.DataModel;
using goFriend.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace goFriend.Services
{
    public class SignalRService
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());
        private readonly HttpClient httpClient;
        private HubConnection hubConnection;

        public bool IsConnected { get; private set; }
        public bool IsBusy { get; private set; }

        public SignalRService()
        {
            httpClient = new HttpClient();
        }

        private HttpRequestMessage BuildRequest(string msgType, object msgContent = null)
        {
            HttpRequestMessage request;
            if (msgContent == null)
            {
                request = new HttpRequestMessage
                {
                    RequestUri = new Uri($"{Constants.ChatFuncUrl}/api/{msgType}"),
                    Method = HttpMethod.Get,
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
                    Method = HttpMethod.Post,
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
            var stopWatch = Stopwatch.StartNew();
            TResult result = default;
            try
            {
                Logger.Debug($"SendMessageAsync.BEGIN(msgType={msgType})");
                IsBusy = true;

                var request = BuildRequest(msgType, msgContent);

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
                Logger.Error(e.ToString());
                Logger.TrackError(e);
            }
            finally
            {
                Logger.Debug($"StopAsync.END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        public async Task ConnectAsync()
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                Logger.Debug($"ConnectAsync.BEGIN");

                if (!App.IsUserLoggedIn || App.User == null)
                {
                    Logger.Debug("User is not logged in.");
                    return;
                }
                if (hubConnection != null)
                {
                    Logger.Debug($"hubConnection.State = {hubConnection.State}");
                    if (hubConnection.State == HubConnectionState.Connected)
                    {
                        Logger.Debug("Hub connected. Do nothing.");
                        return;
                    }
                    else if (hubConnection.State == HubConnectionState.Connecting || hubConnection.State == HubConnectionState.Reconnecting)
                    {
                        Logger.Debug("Connecting to the Hub. Wait for automatic connection to be completed");
                        return;
                    }
                }
                IsBusy = true;
                var request = BuildRequest("negotiate");

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
                hubConnection.On<Chat>(ChatMessageType.CreateChat.ToString(), OnChatReceiveCreateChat);
                await hubConnection.StartAsync();

                IsConnected = true;
                IsBusy = false;
            }
            catch (Exception e)
            {
                IsConnected = false;
                IsBusy = false;
                Logger.Error(e.ToString());
                Logger.TrackError(e);
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

        private Task HubConnectionOnClosed(Exception arg)
        {
            IsConnected = false;
            IsBusy = false;
            Logger.Debug($"OnClosed(exception={arg})");
            Logger.Debug("Waiting for 5 seconds before rejoining the chat");
            Task.Delay(TimeSpan.FromSeconds(5));
            ConnectAsync();
            return Task.CompletedTask;
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
                Logger.Error(e.ToString());
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
                Logger.Error(e.ToString());
                Logger.TrackError(e);
            }
            finally
            {
                Logger.Debug($"OnChatReceiveMessage.END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }
    }
}
