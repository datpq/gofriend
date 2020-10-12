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
using goFriend.DataModel;
using System.Collections.Generic;
using System.Linq;

namespace goFriend.Functions
{
    public class Talk
    {
        private static readonly Dictionary<int, List<ChatFriendOnline>> OnlineMembers = new Dictionary<int, List<ChatFriendOnline>>();
        private static readonly object LockChatMessage = new object();

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

                string json = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogDebug($"json = {json}");

                var msg = JsonConvert.DeserializeObject<ChatJoinChatModel>(json);
                var chat = _dataRepo.Get<Chat>(x => x.Id == msg.ChatId);

                await signalRGroupActions.AddAsync(
                    new SignalRGroupAction
                    {
                        UserId = UserId,
                        GroupName = chat.Id.ToString(),
                        Action = GroupAction.Add
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
