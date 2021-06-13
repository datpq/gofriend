using goFriend.Services.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using ILogger = Microsoft.Extensions.Logging.ILogger;

using goFriend.DataModel;
using goFriend.Services;
using System.Net.Http;
using System.Collections.Concurrent;

namespace goFriend.Functions
{
    public class Talk
    {
        private static readonly ConcurrentDictionary<int, List<ChatFriendOnline>> OnlineMembers = new ConcurrentDictionary<int, List<ChatFriendOnline>>();
        private static readonly ConcurrentDictionary<int, object> LockChatMessageByChatId = new ConcurrentDictionary<int, object>();
        private static readonly HttpClient httpClient = new HttpClient { BaseAddress = new Uri($"{Constants.AzureBackendUrl}/") };

        private readonly IDataRepository _dataRepo;

        public Talk(IDataRepository dataRepo)
        {
            _dataRepo = dataRepo;
        }

        [FunctionName("JoinChat")]
        public async Task<IActionResult> JoinChat(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "JoinChat")] HttpRequest req,
            [SignalR(HubName = Constants.SignalRHubName)] IAsyncCollector<SignalRGroupAction> signalRGroupActions,
            ILogger log)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                log.LogDebug("JoinChat.BEGIN");

                req.ParseSignalRHeaders(out string UserId, out string token);
                log.LogDebug($"UserId={UserId}, Authorization={token}");

                var friendId = int.Parse(UserId);
                var myGroups = _dataRepo.GetMany<GroupFriend>(x => x.FriendId == friendId && x.Active).Select(x => x.GroupId).ToList();
                var arrChatIds = _dataRepo.GetMany<Chat>(x => true).Where(x =>
                    $"{Extension.Sep}{x.Members}{Extension.Sep}".IndexOf($"{Extension.Sep}u{friendId}{Extension.Sep}", StringComparison.Ordinal) >= 0 ||
                    myGroups.Any(y => $"{Extension.Sep}{x.Members}{Extension.Sep}".IndexOf($"{Extension.Sep}g{y}{Extension.Sep}", StringComparison.Ordinal) >= 0))
                    .Select(x => x.Id).ToList();

                await Task.WhenAll(arrChatIds.Select(async x =>
                {
                    log.LogDebug($"Adding user {friendId} to group {x}");
                    await signalRGroupActions.AddAsync(
                        new SignalRGroupAction
                        {
                            UserId = UserId,
                            GroupName = x.ToString(),
                            Action = GroupAction.Add
                        });
                }));

                return new OkObjectResult(null);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                return new BadRequestObjectResult($"Error: {e.Message}");
            }
            finally
            {
                log.LogDebug($"JoinChat.END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        [FunctionName("Ping")]
        public async Task<IActionResult> Ping(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Ping")] HttpRequest req,
            ILogger log)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                log.LogDebug("Ping.BEGIN");

                req.ParseSignalRHeaders(out string UserId, out string token);
                log.LogDebug($"UserId={UserId}, Authorization={token}");

                string json = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogDebug($"json = {json}");

                var msg = JsonConvert.DeserializeObject<ChatMessage>(json);

                var result = OnlineMembers.GetOrAdd(msg.ChatId, new List<ChatFriendOnline>());

                var friendOnline = result.SingleOrDefault(x => x.Friend.Id == msg.OwnerId);
                if (friendOnline != null)
                {
                    friendOnline.Time = DateTime.UtcNow;
                }
                else
                {
                    result.Add(new ChatFriendOnline
                    {
                        Friend = new Friend
                        {
                            Id = msg.OwnerId,
                            Name = msg.OwnerName
                        },
                        LogoUrl = msg.LogoUrl,
                        Time = DateTime.UtcNow
                    });
                }

                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                return new BadRequestObjectResult($"Error: {e.Message}");
            }
            finally
            {
                log.LogDebug($"Ping.END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        [FunctionName("Text")]
        public async Task<IActionResult> Text(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Text")] HttpRequest req,
            [SignalR(HubName = Constants.SignalRHubName)] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                log.LogDebug("Text.BEGIN");

                req.ParseSignalRHeaders(out string UserId, out string token);
                log.LogDebug($"UserId={UserId}, Authorization={token}");

                string json = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogDebug($"json = {json}");

                var msg = JsonConvert.DeserializeObject<ChatMessage>(json);
                log.LogDebug($"MessageType={msg.MessageType}, OwnerId={msg.OwnerId}, Token={msg.Token}");
                log.LogDebug($"ChatId={msg.ChatId}, Message={msg.Message}, MessageIndex={msg.MessageIndex}");

                //Check if user has been kicked out and try to send more message
                var chat = _dataRepo.Get<Chat>(x => x.Id == msg.ChatId);
                if (chat == null || (chat.GetChatType() == ChatType.Individual && !chat.MembersContain(int.Parse(UserId)))) {
                    log.LogWarning($"Chat {msg.ChatId} not found or user {UserId} is not a member");
                    return new OkObjectResult(null);
                }

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
                        log.LogError("Message not found for update");
                    }
                }
                else // New message
                {
                    AddNewMessage(log, msg);
                    _dataRepo.Commit();
                }

                log.LogDebug($"Sending message to the group {msg.ChatId}");
                msg.Token = null;
                var friend = _dataRepo.Get<Friend>(x => x.Id == int.Parse(UserId));
                msg.OwnerName = friend.Name;
                msg.OwnerFirstName = friend.FirstName;

                await signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        Target = msg.MessageType.ToString(),
                        GroupName = msg.ChatId.ToString(),
                        Arguments = new[] { msg }
                    });

                return new OkObjectResult(null);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                return new BadRequestObjectResult($"Error: {e.Message}");
            }
            finally
            {
                log.LogDebug($"Text.END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        [FunctionName("Attachment")]
        public async Task<IActionResult> Attachment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Attachment")] HttpRequest req,
            [SignalR(HubName = Constants.SignalRHubName)] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                log.LogDebug("Attachment.BEGIN");

                req.ParseSignalRHeaders(out string UserId, out string token);
                log.LogDebug($"UserId={UserId}, Authorization={token}");

                string json = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogDebug($"json = {json}");

                var msg = JsonConvert.DeserializeObject<ChatMessage>(json);
                log.LogDebug($"MessageType={msg.MessageType}, OwnerId={msg.OwnerId}, Token={msg.Token}");
                log.LogDebug($"ChatId={msg.ChatId}, Message={msg.Message}, MessageIndex={msg.MessageIndex}");

                //Check if user has been kicked out and try to send more message
                var chat = _dataRepo.Get<Chat>(x => x.Id == msg.ChatId);
                if (chat == null || (chat.GetChatType() == ChatType.Individual && !chat.MembersContain(int.Parse(UserId))))
                {
                    log.LogWarning($"Chat {msg.ChatId} not found or user {UserId} is not a member");
                    return new OkObjectResult(null);
                }

                AddNewMessage(log, msg);
                _dataRepo.Commit();

                var newAttachments = $"{msg.ChatId}/{msg.MessageIndex:D8}_{msg.OwnerId}{Path.GetExtension(msg.Attachments)}";
                log.LogDebug($"renaming {msg.Attachments} to {newAttachments}");
                var storageService = new StorageService(new LoggerMicrosoftImpl(log));
                storageService.Rename(msg.Attachments, newAttachments);
                msg.Attachments = newAttachments;
                _dataRepo.Commit();

                log.LogDebug($"Sending message to the group {msg.ChatId}");
                msg.Token = null;
                var friend = _dataRepo.Get<Friend>(x => x.Id == int.Parse(UserId));
                msg.OwnerName = friend.Name;
                msg.OwnerFirstName = friend.FirstName;

                await signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        Target = msg.MessageType.ToString(),
                        GroupName = msg.ChatId.ToString(),
                        Arguments = new[] { msg }
                    });

                return new OkObjectResult(null);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                return new BadRequestObjectResult($"Error: {e.Message}");
            }
            finally
            {
                log.LogDebug($"Attachment.END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        private void RemoveCache(string key)
        {
            var requestUri = $"api/Friend/ClearCache/{key}";
            httpClient.GetAsync(requestUri);
            //new HttpClient { BaseAddress = new Uri($"{Constants.AzureBackendUrlDev}/") }.GetAsync(requestUri);
            //new HttpClient { BaseAddress = new Uri($"{Constants.AzureBackendUrlChat}/") }.GetAsync(requestUri);
        }

        [FunctionName("CreateChat")]
        public async Task<IActionResult> CreateChat(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CreateChat")] HttpRequest req,
            [SignalR(HubName = Constants.SignalRHubName)] IAsyncCollector<SignalRMessage> signalRMessages,
            [SignalR(HubName = Constants.SignalRHubName)] IAsyncCollector<SignalRGroupAction> signalRGroupActions,
            ILogger log)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                log.LogDebug("CreateChat.BEGIN");

                req.ParseSignalRHeaders(out string UserId, out string token);
                log.LogDebug($"UserId={UserId}, Authorization={token}");
                //var friend = _dataRepo.Get<Friend>(x => x.Id == int.Parse(UserId));

                string json = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogDebug($"json = {json}");

                var chat = JsonConvert.DeserializeObject<Chat>(json);
                log.LogDebug($"Id={chat.Id}, Name={chat.Name}, OwnerId={chat.OwnerId}, Members={chat.Members}, Token={chat.Token}");

                //rearrange members, order by Id apart from the first Item which is OwnerId
                var arrMemberIds = chat.GetMemberIds();
                var arrOrderedMemberIds = new int[arrMemberIds.Length - 1];
                Array.Copy(arrMemberIds, 1, arrOrderedMemberIds, 0, arrOrderedMemberIds.Length);
                Array.Sort(arrOrderedMemberIds);

                //OwnerId followed by sorted member Ids
                chat.Members = $"u{arrMemberIds[0]}{Extension.Sep}u{string.Join($"{Extension.Sep}u", arrOrderedMemberIds)}";
                log.LogDebug($"Members={chat.Members}");
                chat.Token = null;

                if (chat.Id == 0) // new Chat
                {
                    if (_dataRepo.GetMany<Chat>(x => x.Members == chat.Members).Any())
                    {
                        log.LogError("Chat already created. Do nothing"); // raise error maybe
                    }
                    else
                    {
                        chat.CreatedDate = DateTime.UtcNow;
                        _dataRepo.Add(chat);

                        //first message is: who created chat
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
                            //second message is: members for the chat
                            _dataRepo.Add(
                                new ChatMessage
                                {
                                    OwnerId = 0,
                                    Chat = chat,
                                    ChatId = chat.Id,
                                    CreatedDate = chat.CreatedDate,
                                    Message = _dataRepo.GetChatMemberNames(chat),
                                    MessageIndex = 2,
                                    MessageType = ChatMessageType.CreateChat
                                });
                        }
                        _dataRepo.Commit();

                        foreach (var memberId in chat.GetMemberIds())
                        {
                            log.LogDebug($"puting {memberId} to the SignalR group");
                            await signalRGroupActions.AddAsync(
                                new SignalRGroupAction
                                {
                                    UserId = memberId.ToString(),
                                    GroupName = chat.Id.ToString(),
                                    Action = GroupAction.Add
                                });
                            log.LogDebug($"sending CreateChat to {memberId}");
                            await signalRMessages.AddAsync(
                                new SignalRMessage
                                {
                                    Target = ChatMessageType.CreateChat.ToString(),
                                    UserId = memberId.ToString(),
                                    //GroupName = chat.Id.ToString(),
                                    Arguments = new[] { chat }
                                });
                            var cacheKey = $".GetChats.{memberId}.";
                            log.LogDebug($"sending ClearCache to backend: cacheKey={cacheKey}");
                            RemoveCache(cacheKey); // refresh GetChats
                        }
                    }
                }
                else
                {
                    var oldChat = _dataRepo.Get<Chat>(x => x.Id == chat.Id);
                    if (oldChat == null)
                    {
                        log.LogError($"Chat not found. (Id={chat.Id})");
                    }
                    else
                    {
                        var arrOldMemberIds = oldChat.GetMemberIds();
                        oldChat.Members = chat.Members;
                        oldChat.Name = chat.Name;

                        var arrRemovingUsers = arrOldMemberIds.Except(arrMemberIds).ToArray();
                        foreach(var memberId in arrRemovingUsers)
                        {
                            log.LogDebug($"removing user {memberId} from SignalR group");
                            await signalRGroupActions.AddAsync(
                                new SignalRGroupAction
                                {
                                    UserId = memberId.ToString(),
                                    GroupName = chat.Id.ToString(),
                                    Action = GroupAction.Remove
                                });
                        }
                        if (arrRemovingUsers.Length > 0)
                        {
                            // Message removing users from chat group
                            AddNewMessage(log, 
                                new ChatMessage
                                {
                                    OwnerId = 0,
                                    ChatId = chat.Id,
                                    Message = $"Loại: {_dataRepo.GetMemberNames(arrRemovingUsers)}",
                                    MessageType = ChatMessageType.CreateChat
                                });
                        }

                        var arrAddingUsers = arrMemberIds.Except(arrOldMemberIds).ToArray();
                        foreach(var memberId in arrAddingUsers)
                        {
                            log.LogDebug($"puting user {memberId} to the SignalR group");
                            await signalRGroupActions.AddAsync(
                                new SignalRGroupAction
                                {
                                    UserId = memberId.ToString(),
                                    GroupName = chat.Id.ToString(),
                                    Action = GroupAction.Add
                                });
                        }
                        if (arrAddingUsers.Length > 0)
                        {
                            // Message adding users to the chat group
                            AddNewMessage(log,
                                new ChatMessage
                                {
                                    OwnerId = 0,
                                    ChatId = chat.Id,
                                    Message = $"Thêm: {_dataRepo.GetMemberNames(arrAddingUsers)}",
                                    MessageType = ChatMessageType.CreateChat
                                });
                        }
                        _dataRepo.Commit();

                        foreach (var memberId in arrOldMemberIds.Union(arrMemberIds))
                        {
                            log.LogDebug($"sending CreateChat to user {memberId}");
                            await signalRMessages.AddAsync(
                                new SignalRMessage
                                {
                                    Target = ChatMessageType.CreateChat.ToString(),
                                    UserId = memberId.ToString(),
                                    //GroupName = chat.Id.ToString(),
                                    Arguments = new[] { chat }
                                });
                            var cacheKey = $".GetChats.{memberId}.";
                            log.LogDebug($"sending ClearCache to backend: cacheKey={cacheKey}");
                            RemoveCache(cacheKey); // refresh GetChats
                            //No Cache for GetMessages implemented for now, just do in case
                            cacheKey = $".GetMessages.{memberId}.";
                            log.LogDebug($"sending ClearCache to backend: cacheKey={cacheKey}");
                            RemoveCache(cacheKey); // refresh GetMessages
                        }
                    }
                }

                return new OkObjectResult(null);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                return new BadRequestObjectResult($"Error: {e.Message}");
            }
            finally
            {
                log.LogDebug($"CreateChat.END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        [FunctionName("EditGroup")]
        public async Task<IActionResult> EditGroup(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "EditGroup/{groupId}")] HttpRequest req,
            [SignalR(HubName = Constants.SignalRHubName)] IAsyncCollector<SignalRMessage> signalRMessages,
            [SignalR(HubName = Constants.SignalRHubName)] IAsyncCollector<SignalRGroupAction> signalRGroupActions,
            int groupId, ILogger log)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                log.LogDebug($"EditGroup.BEGIN(groupId={groupId})");

                req.ParseSignalRHeaders(out string UserId, out string token);
                log.LogDebug($"UserId={UserId}, Authorization={token}");
                //var friend = _dataRepo.Get<Friend>(x => x.Id == int.Parse(UserId));

                string json = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogDebug($"json = {json}");

                var chat = _dataRepo.Get<Chat>(x => x.Members == $"g{groupId}");
                if (chat == null) throw new ArgumentException($"Could not find the chat for group {groupId}");
                var friendIds = JsonConvert.DeserializeObject<List<int>>(json);
                var arrOldGroupFriendIds = _dataRepo.GetMany<GroupFriend>(x => x.GroupId == groupId && x.Active).Select(x => x.FriendId).ToList();
                log.LogDebug($"chatId={chat?.Id}, friendIds={string.Join(',', friendIds)}, arrOldGroupFriendIds={string.Join(',', arrOldGroupFriendIds)}");

                log.LogDebug($"disconnecting kicked-out members...");
                foreach(var x in arrOldGroupFriendIds.Where(x => !friendIds.Any(y => y == x)).ToList())
                {
                    log.LogDebug($"disconnecting user {x}...");
                    await signalRGroupActions.AddAsync(
                        new SignalRGroupAction
                        {
                            UserId = x.ToString(),
                            GroupName = chat.Id.ToString(),
                            Action = GroupAction.Remove
                        });
                }
                log.LogDebug($"connecting new members...");
                foreach (var x in friendIds.Where(x => !arrOldGroupFriendIds.Any(y => y == x)).ToList())
                {
                    log.LogDebug($"connecting user {x}...");
                    await signalRGroupActions.AddAsync(
                        new SignalRGroupAction
                        {
                            UserId = x.ToString(),
                            GroupName = chat.Id.ToString(),
                            Action = GroupAction.Add
                        });
                }

                return new OkObjectResult(null);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                return new BadRequestObjectResult($"Error: {e.Message}");
            }
            finally
            {
                log.LogDebug($"EditGroup.END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        [FunctionName("Location")]
        public async Task<IActionResult> Location(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Location")] HttpRequest req,
            [SignalR(HubName = Constants.SignalRHubName)] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                log.LogDebug("Location.BEGIN");

                req.ParseSignalRHeaders(out string UserId, out string token);
                log.LogDebug($"UserId={UserId}, Authorization={token}");

                string json = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogDebug($"json = {json}");

                var friendLocation = JsonConvert.DeserializeObject<FriendLocation>(json);
                if (friendLocation.DeviceId == null) friendLocation.DeviceId = "N/A";
                log.LogDebug($"FriendId={friendLocation.FriendId}, SharingInfo={friendLocation.SharingInfo}, DeviceId={friendLocation.DeviceId}");

                var result = _dataRepo.Get<FriendLocation>(x => x.FriendId == friendLocation.FriendId && x.DeviceId == friendLocation.DeviceId);
                if (result == null)
                {
                    log.LogDebug("New FriendLocation recorded.");
                    result = friendLocation;
                    _dataRepo.Add(result);
                }
                else
                {
                    result.Location = friendLocation.Location;
                    result.SharingInfo = friendLocation.SharingInfo;
                }
                result.ModifiedDate = DateTime.UtcNow;

                _dataRepo.Commit();

                if (friendLocation.SharingInfo != null)
                {
                    //var arrGroupIds = friendLocation.SharingInfo.Split(Extension.SepMain).Select(x => x.Split(Extension.SepSub)[0]).ToList();
                    //var arrChatIds = _dataRepo.GetMany<Chat>(
                    //    x => arrGroupIds.Any(y => x.Members == $"g{y}")).Select(x => x.Id).ToList();
                    friendLocation.SharingInfo.Split(Extension.SepMain).ToList().ForEach(async x =>
                    {
                        var groupId = int.Parse(x.Split(Extension.SepSub)[0]);
                        //var radius = double.Parse(x.Split(Extension.SepSub)[1]);
                        var chat = _dataRepo.Get<Chat>(x => x.Members == $"g{groupId}", true);
                        log.LogDebug($"Sending to the group {groupId} (SharingInfo={friendLocation.SharingInfo}, DeviceId={friendLocation.DeviceId}, chatId={chat.Id}, chatName={chat.Name})");

                        await signalRMessages.AddAsync(
                            new SignalRMessage
                            {
                                Target = ChatMessageType.Location.ToString(),
                                GroupName = chat.Id.ToString(),
                                Arguments = new[] {
                                    new FriendLocation {
                                        FriendId = friendLocation.FriendId,
                                        DeviceId = friendLocation.DeviceId,
                                        Location = friendLocation.Location,
                                        SharingInfo = friendLocation.SharingInfo
                                    }
                                }
                            });
                    });
                }

                return new OkObjectResult(null);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                return new BadRequestObjectResult($"Error: {e.Message}");
            }
            finally
            {
                log.LogDebug($"Location.END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        private void AddNewMessage(ILogger log, ChatMessage msg)
        {
            try
            {
                log.LogDebug($"AddNewMessage.BEGIN(ChatId={msg.ChatId}, MessageType={msg.MessageType}, MessageIndex={msg.MessageIndex}, Message={msg.Message})");
                msg.CreatedDate = msg.ModifiedDate = DateTime.UtcNow;
                msg.Chat = null;
                var lockObj = LockChatMessageByChatId.GetOrAdd(msg.ChatId, new object());
                lock (lockObj)
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
                    log.LogDebug($"MessageIndex={msg.MessageIndex}, Id={msg.Id})");

                    _dataRepo.Add(msg);
                }
            }
            catch(Exception e)
            {
                log.LogError(e.Message);
                throw;
            }
            finally
            {
                log.LogDebug($"AddNewMessage.END");
            }
        }

        [FunctionName("Talk")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "talk")] HttpRequest req,
            [SignalR(HubName = Constants.SignalRHubName)] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log, ClaimsPrincipal claimsPrincipal)
        {
            try
            {
                log.LogInformation("Talk.BEGIN");
                var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
                log.LogInformation($"userIdClaim={userIdClaim}");
                var cp = req.HttpContext.User as ClaimsPrincipal;
                if (null != cp)
                {
                    log.LogInformation("cp != null");
                    var identity = cp.Identity as ClaimsIdentity;
                    if (identity != null)
                    {
                        log.LogInformation("identity != null");
                        var claims = identity.Claims;
                        // or
                        log.LogInformation("ClaimName: " + identity.FindFirst("ClaimName")?.Value);
                        foreach (Claim claim in claims)
                        {
                            log.LogInformation("CLAIM TYPE: " + claim.Type + "; CLAIM VALUE: " + claim.Value);
                        }
                    }
                    else
                    {
                        log.LogInformation("identity = null");
                    }

                    foreach (Claim claim in cp.Claims)
                    {
                        log.LogInformation("CLAIM TYPE: " + claim.Type + "; CLAIM VALUE: " + claim.Value);
                    }
                } else
                {
                    log.LogInformation("cp = null");
                }
                string json = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogInformation($"json = {json}");
                dynamic obj = JsonConvert.DeserializeObject(json);
                var jObject = new JObject(obj);

                await signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        Target = "newMessage",
                        GroupName = "1",
                        Arguments = new[] { jObject }
                    });

                var name = obj.name.ToString();
                var text = obj.text.ToString();

                // NOTE: returning values is helpful for testing requests in a browser
                // or with a program such as Postman
                return new OkObjectResult($"Hello {name}, your message was '{text}'");
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                return new BadRequestObjectResult($"Error: {e.Message}");
            }
            finally
            {
                log.LogInformation("Talk.END");
            }
        }
    }
}
