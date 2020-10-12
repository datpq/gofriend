using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using goFriend.DataModel;
using goFriend.Services.Data;
using goFriend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NLog;
using Extension = goFriend.DataModel.Extension;

namespace goFriend.MobileAppService.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ChatController : Controller
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IDataRepository _dataRepo;
        private readonly ICacheService _cacheService;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected readonly string CacheNameSpace;

        public ChatController(IOptions<AppSettings> appSettings, IDataRepository dataRepo, ICacheService cacheService)
        {
            _appSettings = appSettings;
            _dataRepo = dataRepo;
            _cacheService = cacheService;
            CacheNameSpace = GetType().FullName;
        }

        [HttpGet]
        [Route("GetMessages/{friendId}/{chatId}/{startMsgIdx}/{stopMsgIdx}/{pageSize}")]
        public ActionResult<IEnumerable<ChatMessage>> GetMessages([FromHeader] string token, [FromRoute] int friendId,
            int chatId, int startMsgIdx, int stopMsgIdx, int pageSize)
        {
            var stopWatch = Stopwatch.StartNew();
            ActionResult<IEnumerable<ChatMessage>> result = null;
            try
            {
                Logger.Debug($"BEGIN(token={token}, friendId={friendId}, chatId={chatId}, startMsgIdx={startMsgIdx}, stopMsgIdx={stopMsgIdx}, pageSize={pageSize})");

                #region Data Validation

                if (string.IsNullOrEmpty(token))
                {
                    Logger.Warn(Message.MsgMissingToken.Msg);
                    return BadRequest(Message.MsgMissingToken);
                }

                var arrFriends = _dataRepo.GetMany<Friend>(x => x.Active && x.Id == friendId).ToList();
                if (arrFriends.Count != 1)
                {
                    Logger.Warn(Message.MsgUserNotFound.Msg);
                    return BadRequest(Message.MsgUserNotFound);
                }
                var friend = arrFriends.Single();
                if (friend.Token != Guid.Parse(token))
                {
                    Logger.Warn(Message.MsgWrongToken.Msg);
                    return BadRequest(Message.MsgWrongToken);
                }
                Logger.Debug($"friend={friend}");

                #endregion

                var orderByAscending = startMsgIdx < stopMsgIdx;
                if (!orderByAscending)
                {
                    var tmp = startMsgIdx;
                    startMsgIdx = stopMsgIdx;
                    stopMsgIdx = tmp;
                }

                IEnumerable<ChatMessage> queryableResult = _dataRepo.GetMany<ChatMessage>(x =>
                        x.ChatId == chatId && x.MessageIndex > startMsgIdx && x.MessageIndex < stopMsgIdx)
                    .AsQueryable().Include(x => x.Owner);
                queryableResult = orderByAscending ? queryableResult.OrderBy(x => x.MessageIndex)
                    : queryableResult.OrderByDescending(x => x.MessageIndex);

                var messages = queryableResult.Take(pageSize).ToList();
                    
                foreach (var chatMessage in messages)
                {
                    chatMessage.LogoUrl = _dataRepo.Get<Friend>(x => x.Id == chatMessage.OwnerId, true)
                        .GetImageUrl(FacebookImageType.small);
                    chatMessage.OwnerName = chatMessage.Owner.Name;
                    chatMessage.OwnerFirstName = chatMessage.Owner.FirstName;
                }

                result = messages;
                return result;
            }
            catch (FormatException e)
            {
                Logger.Error(e, Message.MsgWrongToken.Msg);
                return BadRequest(Message.MsgWrongToken);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                Logger.Error(e, Message.MsgUnknown.Msg);
                return BadRequest(Message.MsgUnknown);
            }
            finally
            {
                Logger.Debug($"END(Count={result?.Value.Count()}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }


        [HttpGet]
        [Route("GetChats/{friendId}/{useCache}")]
        public ActionResult<IEnumerable<Chat>> GetChats([FromHeader] string token, [FromRoute] int friendId, bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            ActionResult<IEnumerable<Chat>> result = null;
            try
            {
                Logger.Debug($"BEGIN(token={token}, friendId={friendId}, useCache={useCache})");

                #region Data Validation

                if (string.IsNullOrEmpty(token))
                {
                    Logger.Warn(Message.MsgMissingToken.Msg);
                    return BadRequest(Message.MsgMissingToken);
                }

                var arrFriends = _dataRepo.GetMany<Friend>(x => x.Active && x.Id == friendId).ToList();
                if (arrFriends.Count != 1)
                {
                    Logger.Warn(Message.MsgUserNotFound.Msg);
                    return BadRequest(Message.MsgUserNotFound);
                }
                var friend = arrFriends.Single();
                if (friend.Token != Guid.Parse(token))
                {
                    Logger.Warn(Message.MsgWrongToken.Msg);
                    return BadRequest(Message.MsgWrongToken);
                }
                Logger.Debug($"friend={friend}");

                #endregion

                var cachePrefix = $"{CacheNameSpace}.{MethodBase.GetCurrentMethod().Name}";
                var cacheTimeout = _cacheService.GetCacheTimeout(_dataRepo, cachePrefix);
                var cacheKey = $"{cachePrefix}.{friendId}.";
                Logger.Debug($"cacheKey={cacheKey}, cacheTimeout={cacheTimeout}");

                if (useCache)
                {
                    result = _cacheService.Get(cacheKey) as ActionResult<IEnumerable<Chat>>;
                    if (result != null)
                    {
                        Logger.Debug("Cache found. Return value in cache.");
                        return result;
                    }
                }

                var myGroups = _dataRepo.GetMany<GroupFriend>(x => x.FriendId == friendId && x.Active).Select(x => x.GroupId).ToList();

                result = _dataRepo.GetMany<Chat>(x => true).Where(x =>
                    $"{Extension.Sep}{x.Members}{Extension.Sep}".IndexOf($"{Extension.Sep}u{friendId}{Extension.Sep}", StringComparison.Ordinal) >= 0 ||
                    myGroups.Any(y => $"{Extension.Sep}{x.Members}{Extension.Sep}".IndexOf($"{Extension.Sep}g{y}{Extension.Sep}", StringComparison.Ordinal) >= 0))
                    .ToList();

                Logger.Debug($"result={JsonConvert.SerializeObject(result)}");

                _cacheService.Set(cacheKey, result, DateTimeOffset.Now.AddMinutes(cacheTimeout));
                return result;
            }
            catch (FormatException e)
            {
                Logger.Error(e, Message.MsgWrongToken.Msg);
                return BadRequest(Message.MsgWrongToken);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                Logger.Error(e, Message.MsgUnknown.Msg);
                return BadRequest(Message.MsgUnknown);
            }
            finally
            {
                Logger.Debug($"END(Count={result?.Value.Count()}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }
    }
}
