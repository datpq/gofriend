using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using goFriend.DataModel;
using goFriend.MobileAppService.Data;
using goFriend.MobileAppService.Helpers;
using goFriend.MobileAppService.Models;
using goFriend.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NLog;

namespace goFriend.MobileAppService.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly object LockChatMessage = new object();
        private readonly IOptions<AppSettingsModel> _appSettings;
        private readonly IDataRepository _dataRepo;
        private readonly ICacheService _cacheService;
        private readonly IStorageService _storageService;
        protected readonly string CacheNameSpace;

        public ChatHub(IOptions<AppSettingsModel> appSettings, IDataRepository dataRepo, ICacheService cacheService)
        {
            _appSettings = appSettings;
            _dataRepo = dataRepo;
            _cacheService = cacheService;
            _storageService = new StorageService(new LoggerImpl(Logger));
            CacheNameSpace = GetType().FullName;
        }

        public async Task JoinChat(ChatJoinChatModel msg)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                Logger.Debug($"JoinChat.BEGIN({msg.MessageType}, ChatId={msg.ChatId}, OwnerId={msg.OwnerId}, Token={msg.Token})");

                #region Data Validation

                var arrFriends = _dataRepo.GetMany<Friend>(x => x.Active && x.Id == msg.OwnerId).ToList();
                if (arrFriends.Count != 1)
                {
                    Logger.Warn(Message.MsgUserNotFound.Msg);
                    //return Message.MsgUserNotFound;
                    return;
                }
                var friend = arrFriends.Single();
                if (friend.Token != Guid.Parse(msg.Token))
                {
                    Logger.Warn(Message.MsgWrongToken.Msg);
                    //return Message.MsgWrongToken;
                    return;
                }

                Logger.Debug($"friend={friend}");

                #endregion

                var chat = _dataRepo.Get<Chat>(x => x.Id == msg.ChatId);
                if (chat == null)
                {
                    Logger.Error($"Chat not found({msg.ChatId})");
                }

                Logger.Debug($"{friend.Name} joining group {chat.Id}({chat.Name})");
                await Groups.AddToGroupAsync(Context.ConnectionId, chat.Id.ToString());

                PrintChatInfo();
                return;
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                return;
            }
            finally
            {
                Logger.Debug($"JoinChat.END(msg={JsonConvert.SerializeObject(msg)}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

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

        public async Task Text(ChatMessage msg)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                Logger.Debug($"Text.BEGIN({msg.MessageType}, OwnerId={msg.OwnerId}, Token={msg.Token})");
                Logger.Debug($"ChatId={msg.ChatId}, Message={msg.Message}");

                #region Data Validation

                var arrFriends = _dataRepo.GetMany<Friend>(x => x.Active && x.Id == msg.OwnerId).ToList();
                if (arrFriends.Count != 1)
                {
                    Logger.Warn(Message.MsgUserNotFound.Msg);
                    //return Message.MsgUserNotFound;
                    return;
                }
                var friend = arrFriends.Single();
                if (friend.Token != Guid.Parse(msg.Token))
                {
                    Logger.Warn(Message.MsgWrongToken.Msg);
                    //return Message.MsgWrongToken;
                    return;
                }

                Logger.Debug($"friend={friend}");

                #endregion

                msg.CreatedDate = msg.ModifiedDate = DateTime.UtcNow;
                lock (LockChatMessage)
                {
                    var allChatMessages = _dataRepo.GetMany<ChatMessage>(x => x.ChatId == msg.ChatId);
                    if (allChatMessages.Any())
                    {
                        msg.MessageIndex = allChatMessages.Max(x => x.MessageIndex) + 1;
                    }
                    else
                    {
                        msg.MessageIndex = 1;
                    }

                    Logger.Debug("Saving to database");
                    _dataRepo.Add(msg);
                    _dataRepo.Commit();
                }

                Logger.Debug($"Sending message to the group {msg.ChatId}");
                msg.Token = null;
                msg.OwnerName = friend.Name;
                msg.OwnerFirstName = friend.FirstName;
                await Clients.Group(msg.ChatId.ToString()).SendAsync(msg.MessageType.ToString(), msg);
                PrintChatInfo(msg.ChatId);
                //await Clients.All.SendAsync(chatMessage.MessageType.ToString(), chatMessage);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                Logger.Debug("Text.END");
            }
        }

        public async Task Attachment(ChatMessage msg)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                Logger.Debug($"Attachment.BEGIN({msg.MessageType}, OwnerId={msg.OwnerId}, Token={msg.Token})");
                Logger.Debug($"ChatId={msg.ChatId}, Attachments={msg.Attachments}");

                #region Data Validation

                var arrFriends = _dataRepo.GetMany<Friend>(x => x.Active && x.Id == msg.OwnerId).ToList();
                if (arrFriends.Count != 1)
                {
                    Logger.Warn(Message.MsgUserNotFound.Msg);
                    //return Message.MsgUserNotFound;
                    return;
                }
                var friend = arrFriends.Single();
                if (friend.Token != Guid.Parse(msg.Token))
                {
                    Logger.Warn(Message.MsgWrongToken.Msg);
                    //return Message.MsgWrongToken;
                    return;
                }

                Logger.Debug($"friend={friend}");

                #endregion

                msg.CreatedDate = msg.ModifiedDate = DateTime.UtcNow;
                lock (LockChatMessage)
                {
                    var allChatMessages = _dataRepo.GetMany<ChatMessage>(x => x.ChatId == msg.ChatId);
                    if (allChatMessages.Any())
                    {
                        msg.MessageIndex = allChatMessages.Max(x => x.MessageIndex) + 1;
                    }
                    else
                    {
                        msg.MessageIndex = 1;
                    }

                    Logger.Debug("Saving to database");
                    _dataRepo.Add(msg);
                    _dataRepo.Commit();
                }
                var newAttachments = $"{msg.ChatId}/{msg.MessageIndex:D8}_{msg.OwnerId}{Path.GetExtension(msg.Attachments)}";
                Logger.Debug($"renaming {msg.Attachments} to {newAttachments}");
                _storageService.Rename(msg.Attachments, newAttachments);
                msg.Attachments = newAttachments;
                _dataRepo.Commit();

                Logger.Debug($"Sending message to the group {msg.ChatId}");
                msg.Token = null;
                msg.OwnerName = friend.Name;
                msg.OwnerFirstName = friend.FirstName;
                await Clients.Group(msg.ChatId.ToString()).SendAsync(msg.MessageType.ToString(), msg);
                PrintChatInfo(msg.ChatId);
                //await Clients.All.SendAsync(chatMessage.MessageType.ToString(), chatMessage);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                Logger.Debug("Attachment.END");
            }
        }

        public void Echo(string user, string message)
        {
            Logger.Debug($"BEGIN(user={user}, message={message})");

            Clients.Client(Context.ConnectionId).SendAsync("echo", user, message + " (echo from server)");

            Logger.Debug("END");
        }

        public override Task OnConnectedAsync()
        {
            Logger.Debug($"OnConnectedAsync(ConnectionId={Context.ConnectionId})");
            PrintChatInfo();
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Logger.Debug($"OnDisconnectedAsync(ConnectionId={Context.ConnectionId}, exception={exception}");
            PrintChatInfo();
            return base.OnDisconnectedAsync(exception);
        }

        private void PrintChatInfo(int chatId = 0)
        {
        }
    }
}
