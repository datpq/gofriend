using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using goFriend.DataModel;
using goFriend.MobileAppService.Data;
using goFriend.MobileAppService.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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

        public async Task<ChatJoinChatModel> JoinChat(ChatJoinChatModel result)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                Logger.Debug($"JoinChat.BEGIN({result.MessageType}, OwnerId={result.OwnerId}, Token={result.Token})");
                Logger.Debug($"ChatId={result.ChatId}, LastMsgIndex ={result.LastMsgIndex}");

                #region Data Validation

                var arrFriends = _dataRepo.GetMany<Friend>(x => x.Active && x.Id == result.OwnerId).ToList();
                if (arrFriends.Count != 1)
                {
                    Logger.Warn(Message.MsgUserNotFound.Msg);
                    //return Message.MsgUserNotFound;
                    return result;
                }
                var friend = arrFriends.Single();
                if (friend.Token != Guid.Parse(result.Token))
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

                var chat = _dataRepo.Get<Chat>(x => x.Id == result.ChatId);
                if (chat == null)
                {
                    Logger.Error($"Chat not found({result.ChatId})");
                }

                result.Token = null;
                result.MissingMsgCount = _dataRepo.GetMany<ChatMessage>(x =>
                    x.ChatId == result.ChatId && x.MessageIndex > result.LastMsgIndex).Count();
                result.ChatMessages = _dataRepo.GetMany<ChatMessage>(x =>
                        x.ChatId == result.ChatId && x.MessageIndex > result.LastMsgIndex)
                    .AsQueryable().Include(x => x.Owner)
                    .OrderByDescending(x => x.MessageIndex).Take(result.PageSize).ToList();
                foreach (var chatMessage in result.ChatMessages)
                {
                    chatMessage.LogoUrl = _dataRepo.Get<Friend>(x => x.Id == chatMessage.OwnerId, true)
                        .GetImageUrl(FacebookImageType.small);
                    chatMessage.OwnerName = chatMessage.Owner.Name;
                    chatMessage.OwnerFirstName = chatMessage.Owner.FirstName;
                }

                Logger.Debug($"{friend.Name} joining group {chat.Id}({chat.Name}) MissingMsgCount={result.MissingMsgCount}, ChatMessages.Count={result.ChatMessages.Count()}");
                await Groups.AddToGroupAsync(Context.ConnectionId, chat.Id.ToString());

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
                Logger.Debug($"JoinChat.END(result={JsonConvert.SerializeObject(result)}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
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

        public async Task Text(ChatMessage result)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                Logger.Debug($"Text.BEGIN({result.MessageType}, OwnerId={result.OwnerId}, Token={result.Token})");
                Logger.Debug($"ChatId={result.ChatId}, Message={result.Message}");

                #region Data Validation

                var arrFriends = _dataRepo.GetMany<Friend>(x => x.Active && x.Id == result.OwnerId).ToList();
                if (arrFriends.Count != 1)
                {
                    Logger.Warn(Message.MsgUserNotFound.Msg);
                    //return Message.MsgUserNotFound;
                    return;
                }
                var friend = arrFriends.Single();
                if (friend.Token != Guid.Parse(result.Token))
                {
                    Logger.Warn(Message.MsgWrongToken.Msg);
                    //return Message.MsgWrongToken;
                    return;
                }

                Logger.Debug($"friend={friend}");

                #endregion

                result.Time = DateTime.UtcNow;
                var allChatMessages = _dataRepo.GetMany<ChatMessage>(x => x.ChatId == result.ChatId);
                if (allChatMessages.Any())
                {
                    result.MessageIndex = allChatMessages.Max(x => x.MessageIndex) + 1;
                }
                else
                {
                    result.MessageIndex = 1;
                }

                Logger.Debug("Saving to database");
                _dataRepo.Add(result);
                _dataRepo.Commit();

                Logger.Debug($"Sending message to the group {result.ChatId}");
                result.Token = null;
                result.OwnerName = friend.Name;
                result.OwnerFirstName = friend.FirstName;
                await Clients.Group(result.ChatId.ToString()).SendAsync(result.MessageType.ToString(), result);
                PrintChatInfo(result.ChatId);
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
