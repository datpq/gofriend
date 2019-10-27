using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        private readonly ICacheConfigurationService _cacheConfiguration;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected readonly string CacheNameSpace;

        public FriendController(IOptions<AppSettingsModel> appSettings, IMemoryCache memoryCache,
            IDataRepository dataRepo, ICacheConfigurationService cacheConfiguration)
        {
            _appSettings = appSettings;
            _memoryCache = memoryCache;
            _dataRepo = dataRepo;
            _cacheConfiguration = cacheConfiguration;
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
                    Logger.Warn(Message.MsgMissingToken.Msg);
                    return BadRequest(Message.MsgMissingToken);
                }
                var fb = new FacebookClient
                {
                    AccessToken = authToken
                };
                dynamic fbUser = fb.Get("me?fields=name,first_name,middle_name,last_name,id,email,gender,birthday");
                Logger.Debug($"facebookUser={fbUser}");
                if (!(fbUser.id is string facebookId))
                {
                    Logger.Warn(Message.MsgFacebookIdNull.Msg);
                    return BadRequest(Message.MsgFacebookIdNull);
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
                Logger.Error(e, Message.MsgUnknown.Msg);
                return BadRequest(Message.MsgUnknown);
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
                    Logger.Warn(Message.MsgInvalidState.Msg);
                    return BadRequest(Message.MsgInvalidState);
                }
                else if (friend.Id == 0 || string.IsNullOrEmpty(friend.Email))
                {
                    Logger.Warn(Message.MsgIdOrEmailNull.Msg);
                    return BadRequest(Message.MsgIdOrEmailNull);
                }
                var result = _dataRepo.Get<Friend>(x => x.Id == friend.Id);
                if (result == null)
                {
                    Logger.Warn(Message.MsgUserNotFound.Msg);
                    return BadRequest(Message.MsgUserNotFound);
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
                Logger.Error(e, Message.MsgUnknown.Msg);
                return BadRequest(Message.MsgUnknown);
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
                var cacheTimeout = _cacheConfiguration.GetCacheTimeout(cachePrefix);
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
                
                #region Data Validation

                if (string.IsNullOrEmpty(token))
                {
                    Logger.Warn(Message.MsgMissingToken.Msg);
                    return BadRequest(Message.MsgMissingToken);
                }

                var arrFriends = _dataRepo.GetMany<Friend>(x => x.Active && x.Id == friendId).ToList();
                if (arrFriends.Count() != 1)
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

                var arrGroups = _dataRepo.GetMany<Group>(x => x.Active && x.Id == groupId).ToList();
                if (arrGroups.Count() != 1)
                {
                    Logger.Warn(Message.MsgGroupNotFound.Msg);
                    return BadRequest(Message.MsgGroupNotFound);
                }
                
                #endregion

                var groupFixedCatValues = _dataRepo.Get<GroupFixedCatValues>(x => x.Group.Id == groupId);

                // if groupFixedCatValues contains Cat1, Cat2, Cat3 so we start to find Cat4 in QueryString
                var startCatIdx = groupFixedCatValues.GetCatList().Count() + 1;
                var idx = startCatIdx;
                var predefinedCategories = Enumerable.Empty<string>();
                var groupFriends = _dataRepo.GetMany<GroupFriend>(x => x.GroupId == groupId && x.Active);
                if (!Request.Query.Keys.Contains($"Cat{idx}"))
                {
                    predefinedCategories = _dataRepo.GetMany<GroupPredefinedCategory>(
                        x => x.GroupId == groupId && x.ParentId == null).Select(x => x.Category);
                }
                else
                {
                    GroupPredefinedCategory currentPredefinedCategory = null;
                    while (Request.Query.Keys.Contains($"Cat{idx}"))
                    {
                        var localIdx = idx;
                        var catVal = Request.Query[$"Cat{localIdx}"];
                        var parentCategoryId = localIdx == startCatIdx ? null : currentPredefinedCategory?.Id;
                        Logger.Debug($"groupId={groupId}, localIdx={localIdx}, startCatIdx={startCatIdx}, parentCategoryId={parentCategoryId}");

                        currentPredefinedCategory = _dataRepo.Get<GroupPredefinedCategory>(
                            x => x.Group.Id == groupId && x.Category == catVal && x.ParentId == parentCategoryId);
                        //Logger.Debug($"currentPredefinedCategory.Id={currentPredefinedCategory.Id}");
                        groupFriends = groupFriends.Where(x => x.GetCatByIdx(localIdx) == Request.Query[$"Cat{localIdx}"]);
                        idx++;
                    }

                    if (currentPredefinedCategory != null)
                    {
                        predefinedCategories = _dataRepo.GetMany<GroupPredefinedCategory>(
                            x => x.GroupId == groupId && x.ParentId == currentPredefinedCategory.Id).Select(x => x.Category);
                        //Logger.Debug($"predefinedCategories={JsonConvert.SerializeObject(predefinedCategories)}");
                    }
                }

                var groupFiendList = groupFriends.ToList();
                //Logger.Debug($"idx={idx}, groupFiendList={JsonConvert.SerializeObject(groupFiendList)}, predefinedCategories={JsonConvert.SerializeObject(predefinedCategories)}");
                var groupCatValues = predefinedCategories.Union(groupFiendList.Select(x => x.GetCatByIdx(idx))).Distinct();
                result = groupCatValues.Select(x => new ApiGetGroupCatValuesModel
                {
                    CatValue = x,
                    MemberCount = groupFiendList.Count(y => y.GetCatByIdx(idx) == x)
                }).ToList();

                Logger.Debug($"result={JsonConvert.SerializeObject(result)}");
                _memoryCache.Set(cacheKey, result, DateTimeOffset.Now.AddMinutes(cacheTimeout));
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
                var cacheTimeout = _cacheConfiguration.GetCacheTimeout(cachePrefix);
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

                #region Data Validation

                if (string.IsNullOrEmpty(token))
                {
                    Logger.Warn(Message.MsgMissingToken.Msg);
                    return BadRequest(Message.MsgMissingToken);
                }

                var arrFriends = _dataRepo.GetMany<Friend>(x => x.Active && x.Id == friendId).ToList();
                if (arrFriends.Count() != 1)
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

                var arrGroups = _dataRepo.GetMany<Group>(x => x.Active && x.Id == groupId).ToList();
                if (arrGroups.Count() != 1)
                {
                    Logger.Warn(Message.MsgGroupNotFound.Msg);
                    return BadRequest(Message.MsgGroupNotFound);
                }

                #endregion

                result = _dataRepo.Get<GroupFixedCatValues>(x => x.Group.Id == groupId);
                Logger.Debug($"result={JsonConvert.SerializeObject(result)}");

                _memoryCache.Set(cacheKey, result, DateTimeOffset.Now.AddMinutes(cacheTimeout));
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
                Logger.Debug($"END(result={result?.Value}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        [HttpGet]
        [Route("GetMyGroups/{friendId}/{useCache}")]
        public ActionResult<IEnumerable<ApiGetGroupsModel>> GetMyGroups([FromHeader] string token, [FromRoute] int friendId,
            bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            ActionResult<IEnumerable<ApiGetGroupsModel>> result = null;
            try
            {
                Logger.Debug($"BEGIN(token={token}, friendId={friendId}, useCache={useCache})");
                var cachePrefix = $"{CacheNameSpace}.{CurrentMethodName}";
                var cacheTimeout = _cacheConfiguration.GetCacheTimeout(cachePrefix);
                var cacheKey = $"{cachePrefix}.{friendId}";
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

                #region Data Validation

                if (string.IsNullOrEmpty(token))
                {
                    Logger.Warn(Message.MsgMissingToken.Msg);
                    return BadRequest(Message.MsgMissingToken);
                }

                var arrFriends = _dataRepo.GetMany<Friend>(x => x.Active && x.Id == friendId).ToList();
                if (arrFriends.Count() != 1)
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

                var myGroups = _dataRepo.GetMany<GroupFriend>(x => x.FriendId == friendId).AsQueryable().Include(x => x.Group).ToList();
                Logger.Debug($"myGroups={JsonConvert.SerializeObject(myGroups.Select(x => x.Group.Name))}");

                result = myGroups.Select(x => new ApiGetGroupsModel
                {
                    Group = x.Group,
                    GroupFriend = x,
                    UserRight = x.UserRight,
                    MemberCount = _dataRepo.GetMany<GroupFriend>(y => y.GroupId == x.GroupId && y.Active).Count()
                }).ToList();

                Logger.Debug($"result={JsonConvert.SerializeObject(result)}");

                _memoryCache.Set(cacheKey, result, DateTimeOffset.Now.AddMinutes(cacheTimeout));
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
                var cacheTimeout = _cacheConfiguration.GetCacheTimeout(cachePrefix);
                var searchText = Request.Query["searchText"];
                var cacheKey = $"{cachePrefix}.{searchText}";
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

                #region Data Validation

                if (string.IsNullOrEmpty(token))
                {
                    Logger.Warn(Message.MsgMissingToken.Msg);
                    return BadRequest(Message.MsgMissingToken);
                }

                var arrFriends = _dataRepo.GetMany<Friend>(x => x.Active && x.Id == friendId).ToList();
                if (arrFriends.Count() != 1)
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

                result = _dataRepo.GetMany<Group>(
                        x => x.Active && (string.IsNullOrEmpty(searchText)
                                          || CultureInfo.CurrentCulture.CompareInfo.IndexOf(x.Name, searchText, CompareOptions.IgnoreCase) >= 0))
                    .Select(x => new ApiGetGroupsModel
                    {
                        Group = x,
                        UserRight = UserType.NotMember, 
                        MemberCount = _dataRepo.GetMany<GroupFriend>(y => y.GroupId == x.Id && y.Active).Count()
                    }).ToList();

                Logger.Debug($"result={JsonConvert.SerializeObject(result)}");

                _memoryCache.Set(cacheKey, result, DateTimeOffset.Now.AddMinutes(cacheTimeout));
                return result;
            }
            catch(FormatException e)
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

        [HttpPost]
        [Route("GroupSubscription")]
        public IActionResult GroupSubscription([FromHeader] string token, [FromBody] GroupFriend groupFriend)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                Logger.Debug($"BEGIN(token={token}, groupFriend={groupFriend})");

                #region Data Validation

                if (string.IsNullOrEmpty(token))
                {
                    Logger.Warn(Message.MsgMissingToken.Msg);
                    return BadRequest(Message.MsgMissingToken);
                }

                var arrFriends = _dataRepo.GetMany<Friend>(x => x.Active && x.Id == groupFriend.FriendId).ToList();
                if (arrFriends.Count() != 1)
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

                var arrGroups = _dataRepo.GetMany<Group>(x => x.Active && x.Id == groupFriend.GroupId).ToList();
                if (arrGroups.Count() != 1)
                {
                    Logger.Warn(Message.MsgGroupNotFound.Msg);
                    return BadRequest(Message.MsgGroupNotFound);
                }

                #endregion

                if (_dataRepo.GetMany<GroupFriend>(x => x.GroupId == groupFriend.GroupId && x.FriendId == groupFriend.FriendId).Any())
                {
                    Logger.Warn("GroupFriend already exists.");
                    return BadRequest(Message.MsgInvalidData);
                }

                var groupFixedCatValues = _dataRepo.Get<GroupFixedCatValues>(x => x.Group.Id == groupFriend.GroupId).GetCatList().ToList();
                var startCatIdx = groupFixedCatValues.Count() + 1;

                var group = arrGroups.Single();
                var endCatIdx = group.GetCatDescList().Count() + 1;

                var newGroupFriend = new GroupFriend
                {
                    FriendId = groupFriend.FriendId,
                    GroupId = groupFriend.GroupId,
                    Active = true,
                    UserRight = UserType.Pending
                };
                for (var i = 1; i < endCatIdx; i++)
                {
                    if (i < startCatIdx)
                    {
                        newGroupFriend.SetCatByIdx(i, groupFixedCatValues[i-1]);
                    }
                    else
                    {
                        var catVal = groupFriend.GetCatByIdx(i);
                        if (string.IsNullOrEmpty(catVal))
                        {
                            Logger.Warn($"Cat{i} is empty");
                            return BadRequest(Message.MsgInvalidData);
                        }
                        newGroupFriend.SetCatByIdx(i, groupFriend.GetCatByIdx(i));
                    }
                }

                _dataRepo.Add(newGroupFriend);
                _dataRepo.Commit();

                return Ok();
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
                Logger.Debug($"END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }
    }
}
