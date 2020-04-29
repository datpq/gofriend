using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using goFriend.DataModel;
using goFriend.MobileAppService.Data;
using goFriend.MobileAppService.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using NLog;

namespace goFriend.MobileAppService.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IOptions<AppSettingsModel> _appSettings;
        private readonly IDataRepository _dataRepo;
        private readonly ICacheService _cacheService;
        protected readonly string CacheNameSpace;

        public ChatHub(IOptions<AppSettingsModel> appSettings, IDataRepository dataRepo, ICacheService cacheService)
        {
            _appSettings = appSettings;
            _dataRepo = dataRepo;
            _cacheService = cacheService;
            CacheNameSpace = GetType().FullName;
        }

        public async Task<ChatJoinChatModel> JoinChat(int friendId, string token, ChatJoinChatModel joinChatModel)
        {
            var stopWatch = Stopwatch.StartNew();
            ChatJoinChatModel result = null;
            try
            {
                Logger.Debug($"BEGIN(friendId={friendId}, token={token}, ChatId={joinChatModel.ChatId}, LastMsgIndex={joinChatModel.LastMsgIndex})");

                #region Data Validation

                var arrFriends = _dataRepo.GetMany<Friend>(x => x.Active && x.Id == friendId).ToList();
                if (arrFriends.Count != 1)
                {
                    Logger.Warn(Message.MsgUserNotFound.Msg);
                    //return Message.MsgUserNotFound;
                    return result;
                }
                var friend = arrFriends.Single();
                if (friend.Token != Guid.Parse(token))
                {
                    Logger.Warn(Message.MsgWrongToken.Msg);
                    //return Message.MsgWrongToken;
                    return result;
                }

                Logger.Debug($"friend={friend}");

                #endregion

                //var cachePrefix = $"{CacheNameSpace}.{MethodBase.GetCurrentMethod().Name}";
                //var cacheTimeout = _cacheService.GetCacheTimeout(_dataRepo, cachePrefix);
                //var cacheKey = $"{cachePrefix}.{friendId}.";
                //Logger.Debug($"cacheKey={cacheKey}, cacheTimeout={cacheTimeout}");

                //IEnumerable<Chat> chats = null;
                //if (useCache)
                //{
                //    chats = _cacheService.Get(cacheKey) as IEnumerable<Chat>;
                //}

                //if (chats != null)
                //{
                //    Logger.Debug("Cache found. Return value in cache.");
                //}
                //else
                //{
                //    Logger.Debug("Finding chats from database...");
                //    var myGroups = _dataRepo.GetMany<GroupFriend>(x => x.FriendId == friendId && x.Active).Select(x => x.GroupId).ToList();
                //    chats = _dataRepo.GetMany<Chat>(x => true).Where(x =>
                //            $"{Extension.Sep}{x.Members}{Extension.Sep}".IndexOf($"{Extension.Sep}u{friendId}{Extension.Sep}", StringComparison.Ordinal) >= 0 ||
                //            myGroups.Any(y => $"{Extension.Sep}{x.Members}{Extension.Sep}".IndexOf($"{Extension.Sep}g{y}{Extension.Sep}", StringComparison.Ordinal) >= 0))
                //        .ToList();
                //}

                //_cacheService.Set(cacheKey, chats, DateTimeOffset.Now.AddMinutes(cacheTimeout));

                var chat = _dataRepo.Get<Chat>(x => x.Id == joinChatModel.ChatId);
                if (chat == null)
                {
                    Logger.Error($"Chat not found({joinChatModel.ChatId})");
                }

                joinChatModel.MissingMsgCount = _dataRepo.GetMany<ChatMessage>(x =>
                    x.ChatId == joinChatModel.ChatId && x.MessageIndex > joinChatModel.LastMsgIndex).Count();
                joinChatModel.ChatMessages = _dataRepo.GetMany<ChatMessage>(x =>
                        x.ChatId == joinChatModel.ChatId && x.MessageIndex > joinChatModel.LastMsgIndex)
                    .OrderByDescending(x => x.MessageIndex).Take(joinChatModel.PageSize).ToList();

                Logger.Debug($"{friend.Name} joining group {chat.Id}({chat.Name})");
                await Groups.AddToGroupAsync(Context.ConnectionId, chat.Id.ToString());

                result = joinChatModel;
                PrintChatInfo();
                return result;
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                //result = Message.MsgUnknown;
                return result;
            }
            finally
            {
                Logger.Debug($"END(result={result}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        public async Task<Message> LeaveChat(int friendId, string token, int chatId = 0)
        {
            var stopWatch = Stopwatch.StartNew();
            Message result = null;
            try
            {
                Logger.Debug($"BEGIN(friendId={friendId}, token={token}, chatId={chatId})");

                #region Data Validation

                var arrFriends = _dataRepo.GetMany<Friend>(x => x.Active && x.Id == friendId).ToList();
                if (arrFriends.Count != 1)
                {
                    Logger.Warn(Message.MsgUserNotFound.Msg);
                    return Message.MsgUserNotFound;
                }
                var friend = arrFriends.Single();
                if (friend.Token != Guid.Parse(token))
                {
                    Logger.Warn(Message.MsgWrongToken.Msg);
                    return Message.MsgWrongToken;
                }

                Logger.Debug($"friend={friend}");

                #endregion

                if (chatId > 0)
                {
                    Logger.Debug($"{friend.Name} leaving group {chatId}");
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
                }
                else
                {
                    Logger.Debug($"{friend.Name} leaving all the chat");
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                result = Message.MsgUnknown;
                return result;
            }
            finally
            {
                Logger.Debug($"END(result={result}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
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

        public async Task SendMessage(ChatMessage chatMessage)
        {
            try
            {
                Logger.Debug($"SendMessage.BEGIN(ChatId={chatMessage.ChatId}, MessageType={chatMessage.MessageType}, Message={chatMessage.Message}, OwnerName={chatMessage.OwnerName})");
                chatMessage.Time = DateTime.Now;
                var allChatMessages = _dataRepo.GetMany<ChatMessage>(x => x.ChatId == chatMessage.ChatId);
                if (allChatMessages.Any())
                {
                    chatMessage.MessageIndex = allChatMessages.Max(x => x.MessageIndex) + 1;
                }
                else
                {
                    chatMessage.MessageIndex = 1;
                }

                Logger.Debug("Saving to database");
                _dataRepo.Add(chatMessage);
                _dataRepo.Commit();

                Logger.Debug("Sending message to the group");
                await Clients.Group(chatMessage.ChatId.ToString()).SendAsync(chatMessage.MessageType.ToString(), chatMessage);
                PrintChatInfo(chatMessage.ChatId);
                //await Clients.All.SendAsync(chatMessage.MessageType.ToString(), chatMessage);
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
