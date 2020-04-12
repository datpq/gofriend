using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using goFriend.DataModel;
using goFriend.MobileAppService.Data;
using goFriend.MobileAppService.Models;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IOptions<AppSettingsModel> _appSettings;
        private readonly IDataRepository _dataRepo;
        private readonly ICacheService _cacheService;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected readonly string CacheNameSpace;

        public ChatController(IOptions<AppSettingsModel> appSettings, IDataRepository dataRepo, ICacheService cacheService)
        {
            _appSettings = appSettings;
            _dataRepo = dataRepo;
            _cacheService = cacheService;
            CacheNameSpace = GetType().FullName;
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
