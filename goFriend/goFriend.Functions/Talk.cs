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

namespace goFriend.Functions
{
    public class Talk
    {
        private static readonly Dictionary<int, List<ChatFriendOnline>> OnlineMembers = new Dictionary<int, List<ChatFriendOnline>>();
        private static readonly object LockChatMessage = new object();
        private static readonly HttpClient httpClient = new HttpClient { BaseAddress = new Uri($"{Constants.AzureBackendUrlDev}/") };

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

                arrChatIds.ForEach(async x =>
                {
                    log.LogDebug($"Adding user {friendId} to group {x}");
                    await signalRGroupActions.AddAsync(
                        new SignalRGroupAction
                        {
                            UserId = UserId,
                            GroupName = x.ToString(),
                            Action = GroupAction.Add
                        });
                });

                return new OkObjectResult(null);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult("There was an error: " + ex.Message);
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

                if (!OnlineMembers.TryGetValue(msg.ChatId, out List<ChatFriendOnline> result))
                {
                    result = new List<ChatFriendOnline>();
                    OnlineMembers.Add(msg.ChatId, result);
                }

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
            catch (Exception ex)
            {
                return new BadRequestObjectResult("There was an error: " + ex.Message);
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
                var friend = _dataRepo.Get<Friend>(x => x.Id == int.Parse(UserId));

                string json = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogDebug($"json = {json}");

                var msg = JsonConvert.DeserializeObject<ChatMessage>(json);
                log.LogDebug($"MessageType={msg.MessageType}, OwnerId={msg.OwnerId}, Token={msg.Token}");
                log.LogDebug($"ChatId={msg.ChatId}, Message={msg.Message}, MessageIndex={msg.MessageIndex}");

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

                        log.LogDebug("Saving to database");
                        _dataRepo.Add(msg);
                        _dataRepo.Commit();
                    }
                }

                log.LogDebug($"Sending message to the group {msg.ChatId}");
                msg.Token = null;
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
            catch (Exception ex)
            {
                return new BadRequestObjectResult("There was an error: " + ex.Message);
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
                var friend = _dataRepo.Get<Friend>(x => x.Id == int.Parse(UserId));

                string json = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogDebug($"json = {json}");

                var msg = JsonConvert.DeserializeObject<ChatMessage>(json);
                log.LogDebug($"MessageType={msg.MessageType}, OwnerId={msg.OwnerId}, Token={msg.Token}");
                log.LogDebug($"ChatId={msg.ChatId}, Message={msg.Message}, MessageIndex={msg.MessageIndex}");

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

                    log.LogDebug("Saving to database");
                    _dataRepo.Add(msg);
                    _dataRepo.Commit();
                }
                var newAttachments = $"{msg.ChatId}/{msg.MessageIndex:D8}_{msg.OwnerId}{Path.GetExtension(msg.Attachments)}";
                log.LogDebug($"renaming {msg.Attachments} to {newAttachments}");
                var storageService = new Services.StorageService(new Services.LoggerMicrosoftImpl(log));
                storageService.Rename(msg.Attachments, newAttachments);
                msg.Attachments = newAttachments;
                _dataRepo.Commit();

                log.LogDebug($"Sending message to the group {msg.ChatId}");
                msg.Token = null;
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
            catch (Exception ex)
            {
                return new BadRequestObjectResult("There was an error: " + ex.Message);
            }
            finally
            {
                log.LogDebug($"Attachment.END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        private void RemoveCache(string key)
        {
            httpClient.GetAsync($"api/Friend/ClearCache/{key}");
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
                var friend = _dataRepo.Get<Friend>(x => x.Id == int.Parse(UserId));

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
                        _dataRepo.Commit();
                        foreach (var memberId in arrOldMemberIds.Union(arrMemberIds))
                        {
                            if (!arrMemberIds.Contains(memberId)) // removed from chat
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
                            else if (!arrOldMemberIds.Contains(memberId))
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
                        }
                    }
                }

                return new OkObjectResult(null);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult("There was an error: " + ex.Message);
            }
            finally
            {
                log.LogDebug($"CreateChat.END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
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
            catch (Exception ex)
            {
                return new BadRequestObjectResult("There was an error: " + ex.Message);
            }
            finally
            {
                log.LogInformation("Talk.END");
            }
        }
    }
}
