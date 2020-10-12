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
        readonly HttpClient client;
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public delegate void MessageReceivedHandler(object sender, ChatMessage message);
        public delegate void ConnectionHandler(object sender, bool successful, string message);

        public event MessageReceivedHandler OnChatReceiveMessageHandler;
        public event ConnectionHandler Connected;
        public event ConnectionHandler ConnectionFailed;
        public bool IsConnected { get; private set; }
        public bool IsBusy { get; private set; }

        public SignalRService()
        {
            client = new HttpClient();
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

        public async Task<TResult> SendMessageAsync<TResult>(string msgType, object msgContent)
        {
            var stopWatch = Stopwatch.StartNew();
            TResult result = default;
            try
            {
                Logger.Debug($"SendMessageAsync.BEGIN(msgType={msgType})");
                IsBusy = true;

                var request = BuildRequest(msgType, msgContent);

                //var result = await client.PostAsync($"{Constants.HostName}/api/talk", content);
                var responseMessage = await client.SendAsync(request);
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

        public async Task ConnectAsync()
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                Logger.Debug($"ConnectAsync.BEGIN");
                IsBusy = true;
                var request = BuildRequest("negotiate");

                var responseMessage = await client.SendAsync(request);
                string negotiateJson = await responseMessage.Content.ReadAsStringAsync();
                var negotiate = JsonConvert.DeserializeObject<NegotiateInfo>(negotiateJson);

                HubConnection connection = new HubConnectionBuilder()
                    .AddNewtonsoftJsonProtocol()
                    .WithUrl(negotiate.Url, options =>
                    {
                        options.AccessTokenProvider = async () => negotiate.AccessToken;
                    })
                    .Build();

                connection.Closed += Connection_Closed;
                connection.On<ChatMessage>(ChatMessageType.Text.ToString(), OnChatReceiveMessage);
                connection.On<ChatMessage>(ChatMessageType.Attachment.ToString(), OnChatReceiveAttachment);
                await connection.StartAsync();

                IsConnected = true;
                IsBusy = false;

                Connected?.Invoke(this, true, "Connection successful.");
            }
            catch (Exception ex)
            {
                ConnectionFailed?.Invoke(this, false, ex.Message);
                IsConnected = false;
                IsBusy = false;
            }
            finally
            {
                Logger.Debug($"ConnectAsync.END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        Task Connection_Closed(Exception arg)
        {
            ConnectionFailed?.Invoke(this, false, arg.Message);
            IsConnected = false;
            IsBusy = false;
            return Task.CompletedTask;
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

                OnChatReceiveMessageHandler?.Invoke(this, chatMessage);

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
    }
}
