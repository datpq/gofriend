using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using goFriend.DataModel;
using goFriend.MobileAppService.Data;
using goFriend.MobileAppService.Helpers;
using goFriend.Services;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using NLog;
using Extension = goFriend.DataModel.Extension;

namespace goFriend.MobileAppService.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly object LockChatMessage = new object();
        private static readonly Dictionary<int, List<ChatFriendOnline>> OnlineMembers = new Dictionary<int, List<ChatFriendOnline>>();
        private readonly IDataRepository _dataRepo;
        private readonly ICacheService _cacheService;
        private readonly IStorageService _storageService;
        protected readonly string CacheNameSpace;

        public ChatHub(IDataRepository dataRepo, ICacheService cacheService)
        {
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

        public IEnumerable<ChatFriendOnline> Ping(ChatMessage msg)
        {
            var stopWatch = Stopwatch.StartNew();
            List<ChatFriendOnline> result = null;
            try
            {
                Logger.Debug($"Ping.BEGIN(chatId={msg.ChatId}, friendId={msg.OwnerId}, Name={msg.OwnerName})");
                //Logger.Debug($"chatMessage={JsonConvert.SerializeObject(msg)}");
                if (!OnlineMembers.TryGetValue(msg.ChatId, out result))
                {
                    result = new List<ChatFriendOnline>();
                    OnlineMembers.Add(msg.ChatId, result);
                }
                var friendOnline = result.SingleOrDefault(x => x.Friend.Id == msg.OwnerId);
                if (friendOnline != null)
                {
                    //friendOnline.Friend = new Friend {
                    //    Id = msg.OwnerId,
                    //    Name = msg.OwnerName
                    //}; //may be not neccesary
                    friendOnline.Time = DateTime.UtcNow;
                } else
                {
                    result.Add(new ChatFriendOnline
                    {
                        Friend = new Friend {
                            Id = msg.OwnerId,
                            Name = msg.OwnerName
                        },
                        LogoUrl = msg.LogoUrl,
                        Time = DateTime.UtcNow
                    });
                }
                return result;
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                return result;
            }
            finally
            {
                Logger.Debug($"Ping.END(result={JsonConvert.SerializeObject(result)}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        public async Task Text(ChatMessage msg)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                Logger.Debug($"Text.BEGIN({msg.MessageType}, OwnerId={msg.OwnerId}, Token={msg.Token})");
                Logger.Debug($"ChatId={msg.ChatId}, Message={msg.Message}, MessageIndex={msg.MessageIndex}");

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

                msg.ModifiedDate = DateTime.UtcNow;
                if (msg.MessageIndex > 0) //Modification, Deletion
                {
                    var chatMessage = _dataRepo.Get<ChatMessage>(
                        x => x.ChatId == msg.ChatId && x.MessageIndex == msg.MessageIndex);
                    if (chatMessage != null)
                    {
                        chatMessage.IsDeleted = msg.IsDeleted; // Deletion
                        chatMessage.ModifiedDate = DateTime.UtcNow;
                        _dataRepo.Commit();
                    }
                    else
                    {
                        Logger.Error("Message not found for update");
                    }
                }
                else // New message
                {
                    msg.CreatedDate = DateTime.UtcNow;
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

        public void CreateChat(Chat chat)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                Logger.Debug($"CreateChat.BEGIN(Id={chat.Id}, Name={chat.Name}, OwnerId={chat.OwnerId}, Members={chat.Members}, Token={chat.Token})");

                #region Data Validation

                var arrFriends = _dataRepo.GetMany<Friend>(x => x.Active && x.Id == chat.OwnerId).ToList();
                if (arrFriends.Count != 1)
                {
                    Logger.Warn(Message.MsgUserNotFound.Msg);
                    //return Message.MsgUserNotFound;
                    return;
                }
                var friend = arrFriends.Single();
                if (friend.Token != Guid.Parse(chat.Token))
                {
                    Logger.Warn(Message.MsgWrongToken.Msg);
                    //return Message.MsgWrongToken;
                    return;
                }

                Logger.Debug($"friend={friend}");

                #endregion

                //rearrange members, order by Id apart from the first Item which is OwnerId
                var arrMemberIds = chat.GetMemberIds();
                var arrOrderedMemberIds = new int[arrMemberIds.Length - 1];
                Array.Copy(arrMemberIds, 1, arrOrderedMemberIds, 0, arrOrderedMemberIds.Length);
                Array.Sort(arrOrderedMemberIds);

                //OwnerId followed by sorted member Ids
                chat.Members = $"u{arrMemberIds[0]}{Extension.Sep}u{string.Join($"{Extension.Sep}u", arrOrderedMemberIds)}";
                Logger.Debug($"Members={chat.Members}");
                chat.Token = null;

                if (chat.Id == 0) // new Chat
                {
                    if (_dataRepo.GetMany<Chat>(x => x.Members == chat.Members).Any())
                    {
                        Logger.Error("Chat already created. Do nothing"); // raise error maybe
                    }
                    else
                    {
                        chat.CreatedDate = DateTime.UtcNow;
                        _dataRepo.Add(chat);

                        _dataRepo.Add(
                            new ChatMessage
                            {
                                OwnerId = 0,
                                Chat = chat,
                                ChatId = chat.Id,
                                CreatedDate = chat.CreatedDate,
                                Message = string.Format(Constants.ResChatMessageCreateChat,
                                chat.OwnerId == 0 ? Constants.ResSystem : chat.Owner?.FirstName, chat.CreatedDate),
                                MessageIndex = 1,
                                MessageType = ChatMessageType.CreateChat
                            });
                        var chatType = chat.GetChatType();
                        if (chatType == ChatType.Individual || chatType == ChatType.MixedGroup)
                        {
                            _dataRepo.Add(
                                new ChatMessage
                                {
                                    OwnerId = 0,
                                    Chat = chat,
                                    ChatId = chat.Id,
                                    CreatedDate = chat.CreatedDate,
                                    Message = GetMemberNames(chat),
                                    MessageIndex = 2,
                                    MessageType = ChatMessageType.CreateChat
                                });
                        }
                        _dataRepo.Commit();

                        foreach (var memberId in chat.GetMemberIds())
                        {
                            SendCreateChat(chat, memberId);
                            _cacheService.Remove($".GetChats.{memberId}."); // refresh GetChats
                        }
                    }
                }
                else
                {
                    var oldChat = _dataRepo.Get<Chat>(x => x.Id == chat.Id);
                    if (oldChat == null)
                    {
                        Logger.Error($"Chat not found. (Id={chat.Id})");
                    }
                    else
                    {
                        var arrOldMemberIds = oldChat.GetMemberIds();
                        oldChat.Members = chat.Members;
                        oldChat.Name = chat.Name;
                        _dataRepo.Commit();
                        foreach (var memberId in arrMemberIds)
                        {
                            SendCreateChat(oldChat, memberId, arrOldMemberIds);
                            _cacheService.Remove($".GetChats.{memberId}."); // refresh GetChats
                        }
                        foreach (var memberId in arrOldMemberIds)
                        {
                            _cacheService.Remove($".GetChats.{memberId}."); // refresh GetChats
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                Logger.Debug("CreateChat.END");
            }
        }

        private void SendCreateChat(Chat chat, int friendId, int[] oldMemberIds = null)
        {
            Logger.Debug($"SendCreateChat.BEGIN(chat={chat.Id}, friendId={friendId})");
            if (oldMemberIds != null && oldMemberIds.Contains(friendId))
            {
                Logger.Debug($"Sending CreateChat to FriendId={friendId}, through the old chat channel Id={chat.Id}");
                Clients.Group(chat.Id.ToString()).SendAsync(ChatMessageType.CreateChat.ToString(), chat);
                return;
            }
            var existingChat = _dataRepo.GetMany<Chat>(x => !x.Members.StartsWith("g") && x.Id != chat.Id).FirstOrDefault(x => x.MembersContain(friendId));
            if (existingChat != null)
            {
                Logger.Debug($"Sending CreateChat to FriendId={friendId}, through the Inidivial chat channel Id={existingChat.Id}");
                Clients.Group(existingChat.Id.ToString()).SendAsync(ChatMessageType.CreateChat.ToString(), chat);
                return;
            }

            var arrChats = _dataRepo.GetMany<Chat>(x => x.Members.StartsWith("g")).ToList();
            foreach (var c in arrChats)
            {
                var groupFriend = _dataRepo.Get<GroupFriend>(x => x.GroupId == c.GetMemberGroupId()
                                                                  && x.FriendId == friendId && x.Active);
                if (groupFriend != null)
                {
                    Logger.Debug($"Sending CreateChat to FriendId={friendId}, through the Group chat chanel Id={c.Id}");
                    Clients.Group(c.Id.ToString()).SendAsync(ChatMessageType.CreateChat.ToString(), chat);
                    return;
                }
            }
            Logger.Warn($"No channel found that contains FriendId={friendId}. Send to all.");
            Clients.All.SendAsync(ChatMessageType.CreateChat.ToString(), chat);

            Logger.Debug("SendCreateChat.END");
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

        private string GetMemberNames(Chat chat)
        {
            var arrIds = chat.GetMemberIds();
            var arrNames = new string[arrIds.Length];
            for (var i = 0; i < arrNames.Length; i++)
            {
                var localI = i;
                var friend = _dataRepo.Get<Friend>(x => x.Id == arrIds[localI]);
                arrNames[i] = friend.FirstName;
            }
            return string.Join(", ", arrNames);
        }

    }
}
