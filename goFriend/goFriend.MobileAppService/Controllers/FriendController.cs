using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Facebook;
using goFriend.DataModel;
using goFriend.MobileAppService.Data;
using goFriend.MobileAppService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NLog;

namespace goFriend.MobileAppService.Controllers
{
    [Produces("application/json")]
    [Route("api/Friend")]
    public class FriendController : Controller
    {
        private readonly IOptions<AppSettingsModel> _appSettings;
        private readonly IMemoryCache _memoryCache;
        private readonly IDataRepository _dataRepo;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected readonly string CacheNameSpace;

        private static readonly Message MsgMissingToken = new Message { Code = MessageCode.MissingToken, Msg = "Missing Token" };
        private static readonly Message MsgInvalidState = new Message { Code = MessageCode.InvalidState, Msg = "Invalid State" };
        private static readonly Message MsgFacebookIdNull = new Message { Code = MessageCode.FacebookIdNull, Msg = "FacebookId is null" };
        private static readonly Message MsgIdOrEmailNull = new Message { Code = MessageCode.IdOrEmailNull, Msg = "Id or Email is null" };
        private static readonly Message MsgUnknown = new Message { Code = MessageCode.Unknown, Msg = "Unknown error" };
        private static readonly Message MsgUserNotFound = new Message { Code = MessageCode.UserNotFound, Msg = "User not found." };
        private static readonly Message MsgWrongToken = new Message { Code = MessageCode.UserTokenError, Msg = "Wrong Token" };

        public FriendController(IOptions<AppSettingsModel> appSettings, IMemoryCache memoryCache, IDataRepository dataRepo)
        {
            _appSettings = appSettings;
            _memoryCache = memoryCache;
            _dataRepo = dataRepo;
            CacheNameSpace = GetType().FullName;
        }

        protected string CurrentMethodName
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);

                return sf.GetMethod().Name;
            }
        }

        // GET: api/Friend
        [HttpGet]
        public IEnumerable<Friend> Get()
        {
            var stopWatch = Stopwatch.StartNew();
            if (Logger.IsDebugEnabled && Request != null)
            {
                Logger.Debug("BEGIN");
            }
            var result = _dataRepo.GetAll<Friend>();
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug($"END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
            return result;
        }

        // GET: api/Friend/5
        [HttpGet("{id}", Name = "Get")]
        public Friend Get(int id)
        {
            var stopWatch = Stopwatch.StartNew();
            if (Logger.IsDebugEnabled && Request != null)
            {
                Logger.Debug($"BEGIN({id})");
            }
            //var result = _dataRepo.Get<Friend>(x => x.Id == id);
            var result = (_dataRepo.GetMany<Friend>(x => x.Id == id) as IQueryable<Friend>).Include(x => x.GroupFriends).FirstOrDefault();
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug($"END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
            return result;
        }

        [HttpGet]
        [Route("LoginWithFacebook")]
        public ActionResult<Friend> LoginWithFacebook([FromHeader] string authToken, [FromHeader] string deviceInfo)
        {
            var stopWatch = Stopwatch.StartNew();
            Logger.Debug($"BEGIN(authToken={authToken}, deviceInfo={deviceInfo})");
            try
            {
                if (string.IsNullOrEmpty(authToken))
                {
                    Logger.Warn(MsgMissingToken.Msg);
                    return BadRequest(MsgMissingToken);
                }
                var fb = new FacebookClient
                {
                    AccessToken = authToken
                };
                dynamic fbUser = fb.Get("me?fields=name,first_name,middle_name,last_name,id,email,gender,birthday");
                Logger.Debug($"facebookUser={fbUser}");
                if (!(fbUser.id is string facebookId))
                {
                    Logger.Warn(MsgFacebookIdNull.Msg);
                    return BadRequest(MsgFacebookIdNull);
                }
                var result = _dataRepo.Get<Friend>(x => x.FacebookId == facebookId);
                if (result == null) //register with facebook
                {
                    lock (_dataRepo)
                    {
                        result = _dataRepo.Get<Friend>(x => x.FacebookId == facebookId);
                        if (result == null)
                        {
                            Logger.Debug("New user registered");
                            result = new Friend
                            {
                                FacebookId = facebookId,
                                CreatedDate = DateTime.Now,
                                ModifiedDate = DateTime.Now,
                                Active = true // new user is Active for now
                            };
                            _dataRepo.Add(result);
                        }
                        else
                        {
                            Logger.Warn("Duplicate request happened.");
                        }
                    }
                }
                else//already registered. Logged-in again
                {
                    Logger.Debug($"Already registered. Logged-in again.  {result.ToFullString()}");
                }

                var isUpdated = false;
                if (!string.IsNullOrEmpty(fbUser.name) && result.Name != fbUser.name)
                {
                    result.Name = fbUser.name;
                    Logger.Debug($"Name updated: {result.Name}");
                    isUpdated = true;
                }
                if (!string.IsNullOrEmpty(fbUser.first_name) && result.FirstName != fbUser.first_name)
                {
                    result.FirstName = fbUser.first_name;
                    Logger.Debug($"FirstName updated: {result.FirstName}");
                    isUpdated = true;
                }
                if (!string.IsNullOrEmpty(fbUser.last_name) && result.LastName != fbUser.last_name)
                {
                    result.LastName = fbUser.last_name;
                    Logger.Debug($"LastName updated: {result.LastName}");
                    isUpdated = true;
                }
                if (!string.IsNullOrEmpty(fbUser.middle_name) && result.MiddleName != fbUser.middle_name)
                {
                    result.MiddleName = fbUser.middle_name;
                    Logger.Debug($"MiddleName updated: {result.MiddleName}");
                    isUpdated = true;
                }
                if (!string.IsNullOrEmpty(fbUser.email) && result.Email != fbUser.email)
                {
                    result.Email = fbUser.email;
                    Logger.Debug($"Email updated: {result.Email}");
                    isUpdated = true;
                }
                if (!string.IsNullOrEmpty(fbUser.gender) && result.Gender != fbUser.gender)
                {
                    result.Gender = fbUser.gender;
                    Logger.Debug($"Gender updated: {result.Gender}");
                    isUpdated = true;
                }
                result.FacebookToken = authToken;
                if (!string.IsNullOrEmpty(deviceInfo) && result.DeviceInfo != deviceInfo)
                {
                    result.DeviceInfo = deviceInfo;
                    Logger.Debug($"DeviceInfo updated: {result.DeviceInfo}");
                    isUpdated = true;
                }
                result.Token = Guid.NewGuid();
                if (!string.IsNullOrEmpty(fbUser.birthday))
                {
                    try
                    {
                        var birthday = DateTime.ParseExact(fbUser.birthday, "MM/dd/yyyy", null);
                        if (result.Birthday != birthday)
                        {
                            Logger.Debug($"Birthday updated: {result.Birthday}");
                            result.Birthday = birthday;
                            isUpdated = true;
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
                if (isUpdated)
                {
                    result.ModifiedDate = DateTime.Now;
                }

                _dataRepo.Commit();
                return result;
            }
            catch (Exception e)
            {
                Logger.Error(e, MsgUnknown.Msg);
                return BadRequest(MsgUnknown);
            }
            finally
            {
                Logger.Debug($"END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        [HttpPut]
        [Route("SaveBasicInfo")]
        public IActionResult SaveBasicInfo([FromBody]Friend friend)
        {
            var stopWatch = Stopwatch.StartNew();
            Logger.Debug($"BEGIN({friend})");
            try
            {
                if (friend == null || !ModelState.IsValid)
                {
                    Logger.Warn(MsgInvalidState.Msg);
                    return BadRequest(MsgInvalidState);
                }
                else if (friend.Id == 0 || string.IsNullOrEmpty(friend.Email))
                {
                    Logger.Warn(MsgIdOrEmailNull.Msg);
                    return BadRequest(MsgIdOrEmailNull);
                }
                var result = _dataRepo.Get<Friend>(x => x.Id == friend.Id);
                if (result == null)
                {
                    Logger.Warn(MsgUserNotFound.Msg);
                    return BadRequest(MsgUserNotFound);
                }

                result.Name = friend.Name;
                result.FirstName = friend.FirstName;
                result.LastName = friend.LastName;
                result.MiddleName = friend.MiddleName;
                result.Email = friend.Email;
                result.Birthday = friend.Birthday;
                result.Gender = friend.Gender;
                result.ModifiedDate = DateTime.Now;
                Logger.Debug($"SaveBasicInfo ok: {result}");
                _dataRepo.Commit();
                return Ok();
            }
            catch (Exception e)
            {
                Logger.Error(e, MsgUnknown.Msg);
                return BadRequest(MsgUnknown);
            }
            finally
            {
                Logger.Debug($"END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        [HttpGet]
        [Route("GetGroupCatValues/{friendId}/{groupId}/{useCache}")]
        public ActionResult<IEnumerable<ApiGetGroupCatValuesModel>> GetGroupCatValues([FromHeader] string token, [FromRoute] int friendId,
            [FromRoute] int groupId, bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            ActionResult<IEnumerable<ApiGetGroupCatValuesModel>> result = null;
            try
            {
                Logger.Debug($"BEGIN(token={token}, friendId={friendId}, groupId={groupId}, useCache={useCache}, QueryString={Request.QueryString})");
                var cachePrefix = $"{CacheNameSpace}.{CurrentMethodName}";
                var cacheTimeout = _appSettings.Value.CacheDefaultTimeout;
                var cacheKey = $"{cachePrefix}.{friendId}.{groupId}.{Request.QueryString}";
                Logger.Debug($"cacheKey={cacheKey}, cacheTimeout={cacheTimeout}");

                if (useCache)
                {
                    result = _memoryCache.Get(cacheKey) as ActionResult<IEnumerable<ApiGetGroupCatValuesModel>>;
                    if (result != null)
                    {
                        Logger.Debug("Cache found. Return value in cache.");
                        return result;
                    }
                }

                if (string.IsNullOrEmpty(token))
                {
                    Logger.Warn(MsgMissingToken.Msg);
                    //return BadRequest(MsgMissingToken);
                }

                var groupFixedCatValues = _dataRepo.Get<GroupFixedCatValues>(x => x.Group.Id == groupId);

                // if groupFixedCatValues contains Cat1, Cat2, Cat3 so we start to find Cat4 in QueryString
                var startCatIdx = groupFixedCatValues.GetCatList().Count() + 1;
                var idx = startCatIdx;
                var predefinedCategories = Enumerable.Empty<string>();
                var groupFriends = _dataRepo.GetMany<GroupFriend>(x => x.GroupId == groupId);
                if (!Request.Query.Keys.Contains($"Cat{idx}"))
                {
                    predefinedCategories = _dataRepo.GetMany<GroupPredefinedCategory>(
                        x => x.GroupId == groupId && x.ParentId == null).Select(x => x.Category);
                    idx++;
                }
                else
                {
                    GroupPredefinedCategory currentPredefinedCategory = null;
                    while (Request.Query.Keys.Contains($"Cat{idx}"))
                    {
                        var localIdx = idx;
                        var catVal = Request.Query[$"Cat{localIdx}"];
                        if (currentPredefinedCategory != null || idx == startCatIdx)
                        {
                            var parentCategory = currentPredefinedCategory;
                            var parentCategoryId = localIdx == startCatIdx ? null : parentCategory?.Id;
                            Logger.Debug($"groupId={groupId}, localIdx={localIdx}, startCatIdx={startCatIdx}, parentCategoryId={parentCategoryId}");

                            currentPredefinedCategory = _dataRepo.Get<GroupPredefinedCategory>(
                                x => x.Group.Id == groupId && x.Category == catVal
                                                           && x.ParentId == parentCategoryId);
                        }
                        groupFriends = groupFriends.Where(x => x.GetCatByIdx(localIdx) == Request.Query[$"Cat{localIdx}"]);
                        idx++;
                    }

                    if (currentPredefinedCategory != null)
                    {
                        predefinedCategories = _dataRepo.GetMany<GroupPredefinedCategory>(
                            x => x.GroupId == groupId && x.ParentId == currentPredefinedCategory.Id).Select(x => x.Category);
                    }
                }

                var groupFiendList = groupFriends.ToList();
                var groupCatValues = predefinedCategories.Union(groupFiendList.Select(x => x.GetCatByIdx(idx))).Distinct();
                result = groupCatValues.Select(x => new ApiGetGroupCatValuesModel
                {
                    CatValue = x,
                    MemberCount = groupFiendList.Count(y => y.GetCatByIdx(idx) == x)
                }).ToList();

                Logger.Debug($"result={JsonConvert.SerializeObject(result)}");
                _memoryCache.Set(cacheKey, result, DateTimeOffset.Now.AddSeconds(cacheTimeout));
                return result;
            }
            catch (FormatException e)
            {
                Logger.Error(e, MsgWrongToken.Msg);
                return BadRequest(MsgWrongToken);
            }
            catch (InvalidOperationException e)
            {
                Logger.Error(e, MsgUserNotFound.Msg);
                return BadRequest(MsgUserNotFound);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                Logger.Error(e, MsgUnknown.Msg);
                return BadRequest(MsgUnknown);
            }
            finally
            {
                Logger.Debug($"END(Count={result?.Value.Count()}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        [HttpGet]
        [Route("GetGroupFixedCatValues/{friendId}/{groupId}/{useCache}")]
        public ActionResult<GroupFixedCatValues> GetGroupFixedCatValues([FromHeader] string token, [FromRoute] int friendId,
            [FromRoute] int groupId, bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            ActionResult<GroupFixedCatValues> result = null;
            try
            {
                Logger.Debug($"BEGIN(token={token}, friendId={friendId}, groupId={groupId}, useCache={useCache}, QueryString={Request.QueryString})");
                var cachePrefix = $"{CacheNameSpace}.{CurrentMethodName}";
                var cacheTimeout = _appSettings.Value.CacheDefaultTimeout;
                var cacheKey = $"{cachePrefix}.{friendId}.{groupId}.{Request.QueryString}";
                Logger.Debug($"cacheKey={cacheKey}, cacheTimeout={cacheTimeout}");

                if (useCache)
                {
                    result = _memoryCache.Get(cacheKey) as ActionResult<GroupFixedCatValues>;
                    if (result != null)
                    {
                        Logger.Debug("Cache found. Return value in cache.");
                        return result;
                    }
                }

                if (string.IsNullOrEmpty(token))
                {
                    Logger.Warn(MsgMissingToken.Msg);
                    return BadRequest(MsgMissingToken);
                }

                result = _dataRepo.Get<GroupFixedCatValues>(x => x.Group.Id == groupId);
                Logger.Debug($"result={JsonConvert.SerializeObject(result)}");

                _memoryCache.Set(cacheKey, result, DateTimeOffset.Now.AddSeconds(cacheTimeout));
                return result;
            }
            catch (FormatException e)
            {
                Logger.Error(e, MsgWrongToken.Msg);
                return BadRequest(MsgWrongToken);
            }
            catch (InvalidOperationException e)
            {
                Logger.Error(e, MsgUserNotFound.Msg);
                return BadRequest(MsgUserNotFound);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                Logger.Error(e, MsgUnknown.Msg);
                return BadRequest(MsgUnknown);
            }
            finally
            {
                Logger.Debug($"END(result={result?.Value}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        [HttpGet]
        [Route("GetGroups/{friendId}/{useCache}")]
        public ActionResult<IEnumerable<ApiGetGroupsModel>> GetGroups([FromHeader] string token, [FromRoute] int friendId,
            bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            ActionResult<IEnumerable<ApiGetGroupsModel>> result = null;
            try
            {
                Logger.Debug($"BEGIN(token={token}, friendId={friendId}, useCache={useCache}, QueryString={Request.QueryString})");
                var cachePrefix = $"{CacheNameSpace}.{CurrentMethodName}";
                var cacheTimeout = _appSettings.Value.CacheDefaultTimeout;
                var cacheKey = $"{cachePrefix}.{friendId}.{Request.QueryString}";
                Logger.Debug($"cacheKey={cacheKey}, cacheTimeout={cacheTimeout}");

                if (useCache)
                {
                    result = _memoryCache.Get(cacheKey) as ActionResult<IEnumerable<ApiGetGroupsModel>>;
                    if (result != null)
                    {
                        Logger.Debug("Cache found. Return value in cache.");
                        return result;
                    }
                }

                if (string.IsNullOrEmpty(token))
                {
                    Logger.Warn(MsgMissingToken.Msg);
                    return BadRequest(MsgMissingToken);
                }
                var friend = _dataRepo.GetMany<Friend>(x => x.Active && x.Id == friendId).Single();
                if (friend.Token != Guid.Parse(token))
                {
                    Logger.Warn(MsgWrongToken.Msg);
                    return BadRequest(MsgWrongToken);
                }
                Logger.Debug($"friend={friend}");

                var registeredGroups = _dataRepo.GetMany<GroupFriend>(x => x.FriendId == friend.Id).AsQueryable().Include(x => x.Group).ToList();
                Logger.Debug($"registeredGroups={JsonConvert.SerializeObject(registeredGroups.Select(x => x.Group.Name))}");

                result = registeredGroups.Select(x => new ApiGetGroupsModel
                {
                    Group = x.Group,
                    IsMember = true,
                    IsActiveMember = x.Active,
                    UserRight = x.UserRight,
                    MemberCount = _dataRepo.GetMany<GroupFriend>(y => y.GroupId == x.GroupId && y.Active).Count()
                })
                    .Union(_dataRepo.GetMany<Group>(x => x.Active && !registeredGroups.Select(y => y.GroupId).Contains(x.Id))
                        .Select(x => new ApiGetGroupsModel
                        {
                            Group = x,
                            IsMember = false,
                            IsActiveMember = false,
                            UserRight = UserType.None,
                            MemberCount = _dataRepo.GetMany<GroupFriend>(y => y.GroupId == x.Id && y.Active).Count()
                        })).ToList();

                Logger.Debug($"result={JsonConvert.SerializeObject(result)}");

                _memoryCache.Set(cacheKey, result, DateTimeOffset.Now.AddSeconds(cacheTimeout));
                return result;
            }
            catch(FormatException e)
            {
                Logger.Error(e, MsgWrongToken.Msg);
                return BadRequest(MsgWrongToken);
            }
            catch (InvalidOperationException e)
            {
                Logger.Error(e, MsgUserNotFound.Msg);
                return BadRequest(MsgUserNotFound);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                Logger.Error(e, MsgUnknown.Msg);
                return BadRequest(MsgUnknown);
            }
            finally
            {
                Logger.Debug($"END(Count={result?.Value.Count()}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        // PUT: api/Friend/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
