using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Facebook;
using goFriend.DataModel;
using goFriend.Services.Data;
using goFriend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NLog;
using Extension = goFriend.DataModel.Extension;
using Group = goFriend.DataModel.Group;

namespace goFriend.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class FriendController : Controller
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IDataRepository _dataRepo;
        private readonly ICacheService _cacheService;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected readonly string CacheNameSpace;

        public FriendController(IOptions<AppSettings> appSettings, IDataRepository dataRepo, ICacheService cacheService)
        {
            _appSettings = appSettings;
            _dataRepo = dataRepo;
            _cacheService = cacheService;
            CacheNameSpace = GetType().FullName;
        }

        [HttpGet]
        [Route("LoginWithFacebook")]
        public ActionResult<Friend> LoginWithFacebook([FromHeader] string authToken, [FromHeader] string deviceInfo, [FromHeader] string info)
        {
            var stopWatch = Stopwatch.StartNew();
            Logger.Debug($"BEGIN(authToken={authToken}, deviceInfo={deviceInfo}, info={info})");
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
                //dynamic fbUser = fb.Get("me?fields=name,first_name,middle_name,last_name,id,email,gender,birthday");
                dynamic fbUser = fb.Get("me?fields=name,first_name,middle_name,last_name,id,email");
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
                            var setting = _dataRepo.Get<Setting, int>(
                                x => Regex.Match(deviceInfo, x.Rule, RegexOptions.IgnoreCase).Success, x => x.Order, true);
                            Logger.Debug($"setting={JsonConvert.SerializeObject(setting)}");
                            result = new Friend
                            {
                                FacebookId = facebookId,
                                ThirdPartyLogin = ThirdPartyLogin.Facebook,
                                ThirdPartyUserId = facebookId,
                                CreatedDate = DateTime.Now,
                                ModifiedDate = DateTime.Now,
                                Token = Guid.NewGuid(),
                                ShowLocation = setting.DefaultShowLocation,
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
                result.ThirdPartyToken = authToken;
                if (!string.IsNullOrEmpty(deviceInfo) && result.DeviceInfo != deviceInfo)
                {
                    result.DeviceInfo = deviceInfo;
                    Logger.Debug($"DeviceInfo updated: {result.DeviceInfo}");
                    isUpdated = true;
                }
                if (!string.IsNullOrEmpty(info) && result.Info != info)
                {
                    result.Info = info;
                    Logger.Debug($"Info updated: {result.Info}");
                    isUpdated = true;
                }

                if (result.ModifiedDate != null && result.ModifiedDate.Value.AddDays(1) < DateTime.Now) //
                {
                    result.Token = Guid.NewGuid();
                    Logger.Debug($"Token expired. new Token = {result.Token}");
                    isUpdated = true;
                }
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

                _cacheService.Remove($".GetProfile.{result.Id}."); // refresh profile
                _cacheService.Remove($".GetSetting.{result.Id}."); // refresh setting
                _cacheService.Remove($".GetMyGroups.{result.Id}."); // refresh my groups

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
        [Route("LoginWithThirdParty")]
        public ActionResult<Friend> LoginWithThirdParty([FromBody] Friend friend, [FromHeader] string deviceInfo)
        {
            var stopWatch = Stopwatch.StartNew();
            Logger.Debug($"BEGIN({friend}, Name={friend.Name}, Email={friend.Email} ThirdPartyLogin={friend.ThirdPartyLogin}, ThirdPartyUserId={friend.ThirdPartyUserId}, Info={friend.Info})");
            //Logger.Debug($"friend={JsonConvert.SerializeObject(friend)}");
            try
            {
                if (!ModelState.IsValid)
                {
                    Logger.Warn(Message.MsgInvalidState.Msg);
                    return BadRequest(Message.MsgInvalidState);
                }
                if (friend.ThirdPartyUserId == null)
                {
                    Logger.Warn(Message.MsgThirdPartyIdNull.Msg);
                    return BadRequest(Message.MsgThirdPartyIdNull);
                }
                var result = _dataRepo.Get<Friend>(x => x.ThirdPartyLogin == friend.ThirdPartyLogin && x.ThirdPartyUserId == friend.ThirdPartyUserId);
                if (result == null) //register with ThirdParty
                {
                    lock (_dataRepo)
                    {
                        result = _dataRepo.Get<Friend>(x => x.ThirdPartyLogin == friend.ThirdPartyLogin && x.ThirdPartyUserId == friend.ThirdPartyUserId);
                        if (result == null)
                        {
                            if (string.IsNullOrEmpty(friend.Name)) {
                                Logger.Warn("Cannot retrieve Name from thirdparty login");
                                friend.Name = "Unknown";
                            }
                            //if (string.IsNullOrEmpty(friend.Name) || string.IsNullOrEmpty(friend.Email))
                            //{
                            //    Logger.Warn(Message.MsgThirdPartyIdNull.Msg);
                            //    return BadRequest(Message.MsgThirdPartyIdNull);
                            //}
                                Logger.Debug("New thirdparty user registered.");
                            var setting = _dataRepo.Get<Setting, int>(
                                x => Regex.Match(deviceInfo, x.Rule, RegexOptions.IgnoreCase).Success, x => x.Order, true);
                            Logger.Debug($"setting={JsonConvert.SerializeObject(setting)}");
                            result = new Friend
                            {
                                ThirdPartyLogin = friend.ThirdPartyLogin,
                                ThirdPartyUserId = friend.ThirdPartyUserId,
                                CreatedDate = DateTime.Now,
                                ModifiedDate = DateTime.Now,
                                Token = Guid.NewGuid(),
                                ShowLocation = setting.DefaultShowLocation,
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
                if (!string.IsNullOrEmpty(friend.Name) && result.Name != friend.Name)
                {
                    result.Name = friend.Name;
                    Logger.Debug($"Name updated: {result.Name}");
                    isUpdated = true;
                }
                if (!string.IsNullOrEmpty(friend.FirstName) && result.FirstName != friend.FirstName)
                {
                    result.FirstName = friend.FirstName;
                    Logger.Debug($"FirstName updated: {result.FirstName}");
                    isUpdated = true;
                }
                if (!string.IsNullOrEmpty(friend.LastName) && result.LastName != friend.LastName)
                {
                    result.LastName = friend.LastName;
                    Logger.Debug($"LastName updated: {result.LastName}");
                    isUpdated = true;
                }
                if (!string.IsNullOrEmpty(friend.MiddleName) && result.MiddleName != friend.MiddleName)
                {
                    result.MiddleName = friend.MiddleName;
                    Logger.Debug($"MiddleName updated: {result.MiddleName}");
                    isUpdated = true;
                }
                if (!string.IsNullOrEmpty(friend.Email) && result.Email != friend.Email)
                {
                    result.Email = friend.Email;
                    Logger.Debug($"Email updated: {result.Email}");
                    isUpdated = true;
                }
                if (!string.IsNullOrEmpty(friend.Gender) && result.Gender != friend.Gender)
                {
                    result.Gender = friend.Gender;
                    Logger.Debug($"Gender updated: {result.Gender}");
                    isUpdated = true;
                }
                //result.ThirdPartyToken = thirdPartyToken;
                if (!string.IsNullOrEmpty(deviceInfo) && result.DeviceInfo != deviceInfo)
                {
                    result.DeviceInfo = deviceInfo;
                    Logger.Debug($"DeviceInfo updated: {result.DeviceInfo}");
                    isUpdated = true;
                }
                if (!string.IsNullOrEmpty(friend.Info) && result.Info != friend.Info)
                {
                    result.Info = friend.Info;
                    Logger.Debug($"Info updated: {result.Info}");
                    isUpdated = true;
                }

                if (result.ModifiedDate != null && result.ModifiedDate.Value.AddDays(1) < DateTime.Now) //
                {
                    result.Token = Guid.NewGuid();
                    Logger.Debug($"Token expired. new Token = {result.Token}");
                    isUpdated = true;
                }

                if (isUpdated)
                {
                    result.ModifiedDate = DateTime.Now;
                }

                _dataRepo.Commit();

                _cacheService.Remove($".GetProfile.{result.Id}."); // refresh profile
                _cacheService.Remove($".GetSetting.{result.Id}."); // refresh setting
                _cacheService.Remove($".GetMyGroups.{result.Id}."); // refresh my groups

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
            Logger.Debug($"BEGIN({friend}, Latitude={friend.Location?.Y}, Longitude={friend.Location?.X}, Info={friend.Info}, ShowLocation={friend.ShowLocation})");
            try
            {
                if (!ModelState.IsValid)
                {
                    Logger.Warn(Message.MsgInvalidState.Msg);
                    return BadRequest(Message.MsgInvalidState);
                }
                else if (friend.Id == 0 && string.IsNullOrEmpty(friend.Email))
                {
                    Logger.Warn(Message.MsgIdAndEmailNull.Msg);
                    return BadRequest(Message.MsgIdAndEmailNull);
                }
                var result = _dataRepo.Get<Friend>(x => x.Id == friend.Id);
                if (result == null)
                {
                    Logger.Warn(Message.MsgUserNotFound.Msg);
                    return BadRequest(Message.MsgUserNotFound);
                }

                //result.Name = friend.Name;
                //result.FirstName = friend.FirstName;
                //result.LastName = friend.LastName;
                //result.MiddleName = friend.MiddleName;
                //result.Email = friend.Email;
                //result.Birthday = friend.Birthday;
                //result.Gender = friend.Gender;
                var isUpdated = false;
                if (friend.Location != null)
                {
                    Logger.Debug("Location not null. Location, Address, CountryName updated.");
                    result.Location = friend.Location;
                    result.Address = friend.Address;
                    result.CountryName = friend.CountryName;
                    isUpdated = true;
                }

                if (friend.Info != null && friend.Info != result.Info)
                {
                    Logger.Debug($"Info updated: {friend.Info}");
                    result.Info = friend.Info;
                    isUpdated = true;
                }

                if (friend.ShowLocation.HasValue && friend.ShowLocation != result.ShowLocation)
                {
                    Logger.Debug($"ShowLocation updated: {friend.ShowLocation}");
                    result.ShowLocation = friend.ShowLocation;
                    isUpdated = true;
                }

                if (friend.Email != null && friend.Email != result.Email)
                {
                    Logger.Debug($"Email updated: {friend.Email}");
                    result.Email = friend.Email;
                    isUpdated = true;
                }

                if (friend.Relationship != null && friend.Relationship != result.Relationship)
                {
                    Logger.Debug($"Relationship updated: {friend.Relationship}");
                    result.Relationship = friend.Relationship;
                    isUpdated = true;
                }

                if (friend.Phone != null && friend.Phone != result.Phone)
                {
                    Logger.Debug($"Phone updated: {friend.Phone}");
                    result.Phone = friend.Phone;
                    isUpdated = true;
                }

                if (friend.Gender != null && friend.Gender != result.Gender)
                {
                    Logger.Debug($"Gender updated: {friend.Gender}");
                    result.Gender = friend.Gender;
                    isUpdated = true;
                }

                if (friend.Birthday != null && friend.Birthday != result.Birthday)
                {
                    Logger.Debug($"Birthday updated: {friend.Birthday}");
                    result.Birthday = friend.Birthday;
                    isUpdated = true;
                }

                if (isUpdated)
                {
                    result.ModifiedDate = DateTime.Now;
                    Logger.Debug($"SaveBasicInfo ok: {result}");
                    _dataRepo.Commit();

                    //remove Cache
                    _cacheService.Remove($".GetProfile.{friend.Id}.");
                    _cacheService.Remove($".GetFriend.{friend.Id}.");
                    //_cacheService.Remove($".GetGroupFriends.");
                }
                else
                {
                    Logger.Debug("Nothing changed.");
                }

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

                var arrGroups = _dataRepo.GetMany<Group>(x => x.Active && x.Id == groupId).ToList();
                if (arrGroups.Count != 1)
                {
                    Logger.Warn(Message.MsgGroupNotFound.Msg);
                    return BadRequest(Message.MsgGroupNotFound);
                }

                #endregion

                var cachePrefix = $"{CacheNameSpace}.{MethodBase.GetCurrentMethod().Name}";
                var cacheTimeout = _cacheService.GetCacheTimeout(_dataRepo, cachePrefix);
                var cacheKey = $"{cachePrefix}.{groupId}.{Request.QueryString}.";
                Logger.Debug($"cacheKey={cacheKey}, cacheTimeout={cacheTimeout}");

                if (useCache)
                {
                    result = _cacheService.Get(cacheKey) as ActionResult<IEnumerable<ApiGetGroupCatValuesModel>>;
                    if (result != null)
                    {
                        Logger.Debug("Cache found. Return value in cache.");
                        return result;
                    }
                }
                
                var groupFixedCatValues = _dataRepo.Get<GroupFixedCatValues>(x => x.GroupId == groupId, true);

                // if groupFixedCatValues contains Cat1, Cat2, Cat3 so we start to find Cat0 (idx) in QueryString which is Cat4 (startCatIdx)
                var startCatIdx = groupFixedCatValues?.GetCatList().Count() + 1 ?? 1;
                var idx = 0;
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
                        var parentCategoryId = localIdx == 0 ? null : currentPredefinedCategory?.Id;
                        Logger.Debug($"groupId={groupId}, localIdx={localIdx}, startCatIdx={startCatIdx}, parentCategoryId={parentCategoryId}, catVal={catVal}");

                        currentPredefinedCategory = _dataRepo.Get<GroupPredefinedCategory>(
                            x => x.GroupId == groupId && x.Category == catVal && x.ParentId == parentCategoryId, true);
                        Logger.Debug($"currentPredefinedCategory.Id={currentPredefinedCategory?.Id}");
                        groupFriends = groupFriends.Where(x => x.GetCatByIdx(localIdx + startCatIdx) == Request.Query[$"Cat{localIdx}"]);
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
                var groupCatValues = predefinedCategories.Union(groupFiendList.Select(x => x.GetCatByIdx(idx + startCatIdx))).Distinct();
                result = groupCatValues.Select(x => new ApiGetGroupCatValuesModel
                {
                    CatValue = x,
                    MemberCount = groupFiendList.Count(y => y.GetCatByIdx(idx + startCatIdx) == x)
                }).OrderBy(x => x.CatValue).ToList();

                //Logger.Debug($"result={JsonConvert.SerializeObject(result)}");
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

                var arrGroups = _dataRepo.GetMany<Group>(x => x.Active && x.Id == groupId).ToList();
                if (arrGroups.Count != 1)
                {
                    Logger.Warn(Message.MsgGroupNotFound.Msg);
                    return BadRequest(Message.MsgGroupNotFound);
                }

                #endregion

                var cachePrefix = $"{CacheNameSpace}.{MethodBase.GetCurrentMethod().Name}";
                var cacheTimeout = _cacheService.GetCacheTimeout(_dataRepo, cachePrefix);
                var cacheKey = $"{cachePrefix}.{groupId}.";
                Logger.Debug($"cacheKey={cacheKey}, cacheTimeout={cacheTimeout}");

                if (useCache)
                {
                    result = _cacheService.Get(cacheKey) as ActionResult<GroupFixedCatValues>;
                    if (result != null)
                    {
                        Logger.Debug("Cache found. Return value in cache.");
                        return result;
                    }
                }

                result = _dataRepo.Get<GroupFixedCatValues>(x => x.GroupId == groupId, true);
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
                Logger.Debug($"END(result={result?.Value}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        [HttpGet]
        [Route("GetGroupFriends/{friendId}/{groupId}/{top}/{skip}/{useCache}")]
        public ActionResult<IEnumerable<GroupFriend>> GetGroupFriends([FromHeader] string token, [FromRoute] int friendId,
            [FromRoute] int groupId, int top = 0, int skip = 0, bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            ActionResult<IEnumerable<GroupFriend>> result = null;
            try
            {
                Logger.Debug($"BEGIN(token={token}, friendId={friendId}, groupId={groupId}, top={top}, skip={skip}, useCache={useCache}, QueryString={Request.QueryString})");
                bool? isActive = null;
                if (Request.Query.Keys.Contains(Extension.ParamIsActive))
                {
                    isActive = bool.Parse(Request.Query[Extension.ParamIsActive]);
                }

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

                var arrGroups = _dataRepo.GetMany<Group>(x => x.Active && x.Id == groupId).ToList();
                if (arrGroups.Count != 1)
                {
                    Logger.Warn(Message.MsgGroupNotFound.Msg);
                    return BadRequest(Message.MsgGroupNotFound);
                }

                #endregion

                var groupFriendMe = _dataRepo.Get<GroupFriend>(x => x.GroupId == groupId && x.FriendId == friendId && x.Active);
                if (groupFriendMe == null) // if user is not an Active member --> can not get the group's friend list
                {
                    Logger.Warn($"User is not an Active member of group: {Message.MsgUserNoPermission.Msg}");
                    return BadRequest(Message.MsgUserNoPermission);
                }
                if (isActive == false && groupFriendMe.UserRight < UserType.Admin) // in order to get pending friend list ==> need to be Admin
                {
                    Logger.Warn($"User is not Admin: {Message.MsgUserNoPermission.Msg}");
                    return BadRequest(Message.MsgUserNoPermission);
                }

                var cachePrefix = $"{CacheNameSpace}.{MethodBase.GetCurrentMethod().Name}";
                var cacheTimeout = _cacheService.GetCacheTimeout(_dataRepo, cachePrefix);
                var cacheKey = $"{cachePrefix}.{groupId}.{top}.{skip}.{Request.QueryString}.";
                Logger.Debug($"cacheKey={cacheKey}, cacheTimeout={cacheTimeout}");

                if (useCache)
                {
                    result = _cacheService.Get(cacheKey) as ActionResult<IEnumerable<GroupFriend>>;
                    if (result != null)
                    {
                        Logger.Debug("Cache found. Return value in cache.");
                        return result;
                    }
                }

                IEnumerable<GroupFriend> queryableResult = _dataRepo.GetMany<GroupFriend>(x => x.GroupId == groupId)
                    .AsQueryable().Include(x => x.Friend);
                if (isActive.HasValue)
                {
                    queryableResult = queryableResult.Where(x => x.Active == isActive.Value);
                }
                var groupFixedCatValues = _dataRepo.Get<GroupFixedCatValues>(x => x.GroupId == groupId, true);

                if (Request.Query.Keys.Contains(Extension.ParamSearchText))
                {
                    var searchText = Request.Query[Extension.ParamSearchText];
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        queryableResult = queryableResult.Where(x => x.Friend.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
                    }
                }

                if (Request.Query.Keys.Contains(Extension.ParamOtherFriendId))
                {
                    var otherFriendId = int.Parse(Request.Query[Extension.ParamOtherFriendId]);
                    queryableResult = queryableResult.Where(x => x.FriendId == otherFriendId);
                }

                // if groupFixedCatValues contains Cat1, Cat2, Cat3 so we start to find Cat0 (idx) in QueryString which is Cat4 (startCatIdx)
                var startCatIdx = groupFixedCatValues?.GetCatList().Count() + 1 ?? 1;
                var idx = 0;
                while (Request.Query.Keys.Contains($"{Extension.ParamCategory}{idx}"))
                {
                    var localIdx = idx;
                    var catVal = Request.Query[$"{Extension.ParamCategory}{localIdx}"];
                    if (string.IsNullOrEmpty(catVal)) continue;
                    //Logger.Debug($"Cat{localIdx + startCatIdx}={Request.Query[$"Cat{localIdx}"]}");
                    queryableResult = queryableResult.Where(x => x.GetCatByIdx(localIdx + startCatIdx) == catVal).ToList();
                    idx++;
                }

                //result = groupFriends.AsQueryable().Include(x => x.Friend).ToList();
                queryableResult = queryableResult.OrderBy(x => x.Friend.Name);
                if (skip > 0)
                {
                    queryableResult = queryableResult.Skip(skip);
                }
                if (top > 0)
                {
                    queryableResult = queryableResult.Take(top);
                }
                //clear Token before returning friend object
                result = queryableResult.Select(x => { x.Friend.Token = Guid.Empty; return x; }).ToList();

                //Logger.Debug($"result={JsonConvert.SerializeObject(result)}");
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

        [HttpGet]
        [Route("GetProfile/{friendId}/{useCache}")]
        public ActionResult<Friend> GetProfile([FromHeader] string token, [FromRoute] int friendId, bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            ActionResult<Friend> result = null;
            try
            {
                Logger.Debug($"BEGIN(token={token}, friendId={friendId}, useCache={useCache})");

                #region Data Validation

                if (string.IsNullOrEmpty(token))
                {
                    Logger.Warn(Message.MsgMissingToken.Msg);
                    return BadRequest(Message.MsgMissingToken);
                }

                var cachePrefix = $"{CacheNameSpace}.{MethodBase.GetCurrentMethod().Name}";
                var cacheTimeout = _cacheService.GetCacheTimeout(_dataRepo, cachePrefix);
                var cacheKey = $"{cachePrefix}.{friendId}.";
                Logger.Debug($"cacheKey={cacheKey}, cacheTimeout={cacheTimeout}");

                if (useCache)
                {
                    result = _cacheService.Get(cacheKey) as ActionResult<Friend>;
                    if (result != null)
                    {
                        Logger.Debug("Cache found. Return value in cache.");
                        return result;
                    }
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

                var resultFriend = friend.CloneJson();
                //TODO uncomment when deploying for a new version
                //clear Token before returning friend object
                //resultFriend.Token = Guid.Empty;
                resultFriend.Info = null;
                result = resultFriend;

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
                Logger.Debug($"END(result={result?.Value}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        [HttpGet]
        [Route("GetSetting/{friendId}/{useCache}")]
        public ActionResult<Setting> GetSetting([FromHeader] string token, [FromRoute] int friendId, bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            ActionResult<Setting> result = null;
            try
            {
                Logger.Debug($"BEGIN(token={token}, friendId={friendId}, useCache={useCache})");

                #region Data Validation

                if (string.IsNullOrEmpty(token))
                {
                    Logger.Warn(Message.MsgMissingToken.Msg);
                    return BadRequest(Message.MsgMissingToken);
                }

                var cachePrefix = $"{CacheNameSpace}.{MethodBase.GetCurrentMethod().Name}";
                var cacheTimeout = _cacheService.GetCacheTimeout(_dataRepo, cachePrefix);
                var cacheKey = $"{cachePrefix}.{friendId}.";
                Logger.Debug($"cacheKey={cacheKey}, cacheTimeout={cacheTimeout}");

                if (useCache)
                {
                    result = _cacheService.Get(cacheKey) as ActionResult<Setting>;
                    if (result != null)
                    {
                        Logger.Debug("Cache found. Return value in cache.");
                        return result;
                    }
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

                result = _dataRepo.Get<Setting, int>(
                    x => Regex.Match($"|{friend.DeviceInfo}|{friend.Info}|Email={friend.Email}|FullName={friend.Name}|FriendId={friend.Id}|",
                        x.Rule, RegexOptions.IgnoreCase).Success, x => x.Order, true);

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
                Logger.Debug($"result={JsonConvert.SerializeObject(result?.Value)}");
                Logger.Debug($"END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        [HttpGet]
        [Route("GetFriendInfo/{friendId}/{otherFriendId}/{useCache}")]
        public ActionResult<Friend> GetFriendInfo([FromHeader] string token, [FromRoute] int friendId,
            [FromRoute] int otherFriendId, bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            ActionResult<Friend> result = null;
            try
            {
                Logger.Debug($"BEGIN(token={token}, friendId={friendId}, otherFriendId={otherFriendId}, useCache={useCache})");

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
                var cacheKey = $"{cachePrefix}.{otherFriendId}.";
                Logger.Debug($"cacheKey={cacheKey}, cacheTimeout={cacheTimeout}");

                if (useCache)
                {
                    result = _cacheService.Get(cacheKey) as ActionResult<Friend>;
                    if (result != null)
                    {
                        Logger.Debug("Cache found. Return value in cache.");
                        return result;
                    }
                }

                var resultFriend = _dataRepo.Get<Friend>(x => x.Id == otherFriendId);
                //clear Token before returning friend object
                resultFriend = resultFriend.CloneJson();
                resultFriend.Token = Guid.Empty;
                result = resultFriend;

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
                Logger.Debug($"END(result={result?.Value}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        [HttpGet]
        [Route("GetFriend/{friendId}/{groupId}/{otherFriendId}/{useCache}")]
        public ActionResult<Friend> GetFriend([FromHeader] string token, [FromRoute] int friendId,
            [FromRoute] int groupId, [FromRoute] int otherFriendId, bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            ActionResult<Friend> result = null;
            try
            {
                Logger.Debug($"BEGIN(token={token}, friendId={friendId}, groupId={groupId}, otherFriendId={otherFriendId}, useCache={useCache})");

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

                var arrGroups = _dataRepo.GetMany<Group>(x => x.Active && x.Id == groupId).ToList();
                if (arrGroups.Count != 1)
                {
                    Logger.Warn(Message.MsgGroupNotFound.Msg);
                    return BadRequest(Message.MsgGroupNotFound);
                }

                #endregion

                var groupFriendMe = _dataRepo.Get<GroupFriend>(x => x.GroupId == groupId && x.FriendId == friendId && x.Active);
                if (groupFriendMe == null) // if user is not an Active member --> can not get other friend's information
                {
                    Logger.Warn($"User is not an Active member of group: {Message.MsgUserNoPermission.Msg}");
                    return BadRequest(Message.MsgUserNoPermission);
                }
                var groupFriendOther = _dataRepo.Get<GroupFriend>(x => x.GroupId == groupId && x.FriendId == otherFriendId);
                if (groupFriendOther == null)
                {
                    Logger.Warn($"Other user is not a member of group: {Message.MsgUserNoPermission.Msg}");
                    return BadRequest(Message.MsgUserNoPermission);
                }
                if (!groupFriendOther.Active && groupFriendMe.UserRight < UserType.Admin) // in order to get pending user's information ==> need to be Admin
                {
                    Logger.Warn($"User is not Admin: {Message.MsgUserNoPermission.Msg}");
                    return BadRequest(Message.MsgUserNoPermission);
                }

                var cachePrefix = $"{CacheNameSpace}.{MethodBase.GetCurrentMethod().Name}";
                var cacheTimeout = _cacheService.GetCacheTimeout(_dataRepo, cachePrefix);
                var cacheKey = $"{cachePrefix}.{otherFriendId}.";
                Logger.Debug($"cacheKey={cacheKey}, cacheTimeout={cacheTimeout}");

                if (useCache)
                {
                    result = _cacheService.Get(cacheKey) as ActionResult<Friend>;
                    if (result != null)
                    {
                        Logger.Debug("Cache found. Return value in cache.");
                        return result;
                    }
                }

                var resultFriend = _dataRepo.Get<Friend>(x => x.Id == otherFriendId);
                //clear Token before returning friend object
                resultFriend = resultFriend.CloneJson();
                resultFriend.Token = Guid.Empty;
                result = resultFriend;

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
                    result = _cacheService.Get(cacheKey) as ActionResult<IEnumerable<ApiGetGroupsModel>>;
                    if (result != null)
                    {
                        Logger.Debug("Cache found. Return value in cache.");
                        return result;
                    }
                }

                var myGroups = _dataRepo.GetMany<GroupFriend>(x => x.FriendId == friendId).AsQueryable().Include(x => x.Group)
                    .ToList().Select(x => { x.Group.Info = null; return x; }).ToList(); // .Info = null is no more necessary as we can set JsonIgnore
                Logger.Debug($"myGroups={JsonConvert.SerializeObject(myGroups.Select(x => x.Group.Name))}");

                result = myGroups.Select(x => new ApiGetGroupsModel
                {
                    Group = x.Group,
                    GroupFriend = x,
                    UserRight = x.UserRight,
                    MemberCount = _dataRepo.GetMany<GroupFriend>(y => y.GroupId == x.GroupId && y.Active).Count()
                }).Select(x => {
                    // there's no JsonIgnore in class GroupFriend, so we need to empty Group and Friend here to make object lighter
                    x.GroupFriend.Group = null;
                    x.GroupFriend.Friend = null;
                    return x;
                }).ToList();

                //Logger.Debug($"result={JsonConvert.SerializeObject(result)}");

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
                string searchText = Request.Query["searchText"];
                var cacheKey = $"{cachePrefix}.{searchText}.";
                Logger.Debug($"cacheKey={cacheKey}, cacheTimeout={cacheTimeout}");

                if (useCache)
                {
                    result = _cacheService.Get(cacheKey) as ActionResult<IEnumerable<ApiGetGroupsModel>>;
                    if (result != null)
                    {
                        Logger.Debug("Cache found. Return value in cache.");
                        return result;
                    }
                }

                //Where clause is done in 2 times to avoid the error LINQ could not be translated. Either rewrite the query in a form that can be translated
                //There is already an open DataReader associated with this Command which must be closed first. ==> ToList()
                result = _dataRepo.GetMany<Group>(x => x.Active && x.Public).ToList().Where(
                    x => string.IsNullOrEmpty(searchText) || x.Name.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    .Select(x => new ApiGetGroupsModel
                    {
                        Group = x,
                        UserRight = UserType.NotMember, 
                        MemberCount = _dataRepo.GetMany<GroupFriend>(y => y.GroupId == x.Id && y.Active).Count()
                    }).ToList();

                //Logger.Debug($"result={JsonConvert.SerializeObject(result)}");

                _cacheService.Set(cacheKey, result, DateTimeOffset.Now.AddMinutes(cacheTimeout));
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

        [HttpGet]
        [Route("GetNotifications/{friendId}/{top}/{skip}/{useCache}")]
        public ActionResult<IEnumerable<Notification>> GetNotifications([FromHeader] string token, [FromRoute] int friendId,
            int top = 0, int skip = 0, bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            ActionResult<IEnumerable<Notification>> result = null;
            try
            {
                Logger.Debug($"BEGIN(token={token}, friendId={friendId}, top={top}, skip={skip}, useCache={useCache})");

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
                var cacheKey = $"{cachePrefix}.{friendId}.{top}.{skip}.";
                Logger.Debug($"cacheKey={cacheKey}, cacheTimeout={cacheTimeout}");

                if (useCache)
                {
                    result = _cacheService.Get(cacheKey) as ActionResult<IEnumerable<Notification>>;
                    if (result != null)
                    {
                        Logger.Debug("Cache found. Return value in cache.");
                        return result;
                    }
                }

                var myGroups = _dataRepo.GetMany<GroupFriend>(x => x.FriendId == friendId && x.Active).Select(x => x.GroupId).ToList();

                //Where clause is done in 2 times to avoid the error LINQ could not be translated. Either rewrite the query in a form that can be translated
                //var queryableResult = _dataRepo.GetMany<Notification>(x => x.CreatedDate.HasValue && x.CreatedDate.Value.AddDays(_appSettings.Value.NotificationFetchingDays) >= DateTime.Now).Where(
                var queryableResult = _dataRepo.GetMany<Notification>(x => true).Where(
                    x => ($"{Extension.Sep}{x.Destination}{Extension.Sep}".IndexOf($"{Extension.Sep}u{friendId}{Extension.Sep}", StringComparison.Ordinal) >= 0 ||
                          myGroups.Any(y => $"{Extension.Sep}{x.Destination}{Extension.Sep}".IndexOf($"{Extension.Sep}g{y}{Extension.Sep}", StringComparison.Ordinal) >= 0))
                    && $"{Extension.Sep}{x.Deletions}{Extension.Sep}".IndexOf($"{Extension.Sep}u{friendId}{Extension.Sep}", StringComparison.Ordinal) < 0)
                    .Select(x =>
                    {
                        x.Deletions = null; //privacy reason
                        x.Destination = null; //privacy reason
                        x.OwnerId = 0; //privacy reason
                        x.Reads = $"{Extension.Sep}{x.Reads}{Extension.Sep}".IndexOf($"{Extension.Sep}u{friendId}{Extension.Sep}",
                            StringComparison.Ordinal).ToString(); //Reads = -1 ==> not read
                        return x;
                    });

                queryableResult = queryableResult.OrderByDescending(x => x.CreatedDate);
                if (skip > 0)
                {
                    queryableResult = queryableResult.Skip(skip);
                }
                if (top > 0)
                {
                    queryableResult = queryableResult.Take(top);
                }
                result = queryableResult.ToList();

                //Logger.Debug($"result={JsonConvert.SerializeObject(result)}");
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

        [HttpGet]
        [Route("ClearCache/{keyPart}")]
        public IActionResult ClearCache([FromRoute] string keyPart)
        {
            Logger.Debug($"BEGIN(keyPart={keyPart})");
            _cacheService.Remove(keyPart);
            Logger.Debug("END");
            return Ok();
        }

        [HttpPut]
        [Route("ReadNotification/{friendId}")]
        public IActionResult ReadNotification([FromHeader] string token, [FromRoute] int friendId, [FromBody] string notifIds)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                Logger.Debug($"BEGIN(token={token}, friendId={friendId}, notifIds={notifIds})");

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

                var isUpdated = false;
                var listIds = notifIds.Split(Extension.Sep).ToList();
                foreach (var notif in _dataRepo.GetMany<Notification>(x => listIds.Contains(x.Id.ToString())))
                {
                    if (notif.DoRead(friendId))
                    {
                        isUpdated = true;
                    }
                }

                if (isUpdated)
                {
                    _dataRepo.Commit();
                }

                //remove Cache
                _cacheService.Remove($".GetNotifications.{friendId}.");

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

        [HttpPost]
        [Route("GroupSubscriptionReact/{friendId}/{groupFriendId}/{userRight}")]
        public IActionResult GroupSubscriptionReact([FromHeader] string token, [FromRoute] int friendId,
            [FromRoute] int groupFriendId, [FromRoute] UserType userRight)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                Logger.Debug($"BEGIN(token={token}, friendId={friendId}, groupFriendId={groupFriendId}, userRight={userRight})");

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

                //var groupFriend = _dataRepo.Get<GroupFriend>(x => x.Id == groupFriendId);
                var groupFriend = _dataRepo.GetMany<GroupFriend>(x => x.Id == groupFriendId)
                    .AsQueryable().Include(x => x.Friend).Include(x => x.Group).FirstOrDefault();
                if (groupFriend == null)
                {
                    if (userRight < UserType.Normal)
                    {
                        Logger.Warn($"Subscription {groupFriendId} is already rejected.");
                        return Ok();
                    }
                    else
                    {
                        Logger.Warn($"GroupFriend {groupFriendId} not found.");
                        return BadRequest(Message.MsgInvalidData);
                    }
                }

                var isApproving = userRight >= UserType.Normal;
                if (isApproving) //Approve a subscription
                {
                    if (groupFriend.Active || groupFriend.UserRight >= UserType.Normal)
                    {
                        Logger.Warn($"User {groupFriend.FriendId} is already active in the group {groupFriend.GroupId}");
                        return Ok();
                    }
                }

                var myGroupFriend =
                    _dataRepo.Get<GroupFriend>(x => x.FriendId == friendId && x.GroupId == groupFriend.GroupId);
                if (myGroupFriend == null)
                {
                    Logger.Warn($"User {friendId} is not a member of the group {groupFriendId}.");
                    return BadRequest(Message.MsgInvalidData);
                }

                if (myGroupFriend.UserRight < UserType.Admin)
                {
                    Logger.Warn($"User is not Admin: {Message.MsgUserNoPermission.Msg}");
                    return BadRequest(Message.MsgUserNoPermission);
                }

                if (isApproving) //approve
                {
                    Logger.Debug("Subscription approved");
                    groupFriend.UserRight = userRight;
                    groupFriend.Active = true;
                }
                else
                {
                    Logger.Debug("Subscription rejected");
                    _dataRepo.Delete<GroupFriend>(x => x.Id == groupFriendId); //reject
                }
                var groupAdmins = _dataRepo.GetMany<GroupFriend>(
                    x => x.GroupId == groupFriend.GroupId && x.UserRight >= UserType.Admin).Select(x => x.FriendId).ToList();
                var jointGroupAdmins = string.Join(Extension.Sep, groupAdmins.Select(x => $"u{x}"));
                _dataRepo.Add(new Notification
                {
                    CreatedDate = DateTime.Now,
                    OwnerId = friendId,
                    Destination = isApproving ? // if approve -> notify all members of group
                        $"g{groupFriend.GroupId}" : $"u{groupFriend.FriendId},{jointGroupAdmins}", // if rejected --> notify only this user and admins
                    NotificationObject = groupFriend.UserRight >= UserType.Normal ?
                        (GroupSubscriptionNotifBase)new NotifSubscriptionApproved
                        {
                            FriendId = groupFriend.FriendId,
                            FriendName = groupFriend.Friend.Name,
                            FacebookId = groupFriend.Friend.FacebookId,
                            GroupId = groupFriend.GroupId,
                            GroupName = groupFriend.Group.Name
                        } : new NotifSubscriptionRejected
                        {
                            FriendId = groupFriend.FriendId,
                            FriendName = groupFriend.Friend.Name,
                            FacebookId = groupFriend.Friend.FacebookId,
                            GroupId = groupFriend.GroupId,
                            GroupName = groupFriend.Group.Name
                        }
                });
                _dataRepo.Commit();

                //remove Cache
                _cacheService.Remove($".GetGroupFriends.{groupFriend.GroupId}."); //refresh group members, pending members and also active members
                _cacheService.Remove($".GetMyGroups.{groupFriend.FriendId}."); // refresh my groups of the approved/rejected user
                groupAdmins.ForEach(x =>
                {
                    if (isApproving)
                    {
                        _cacheService.Remove($".GetMyGroups.{x}."); // refresh my groups of the admin users
                    }
                    _cacheService.Remove($".GetNotifications.{x}."); // refresh notification of all Admin
                });

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

                var arrGroups = _dataRepo.GetMany<Group>(x => x.Active && x.Id == groupFriend.GroupId).ToList();
                if (arrGroups.Count != 1)
                {
                    Logger.Warn(Message.MsgGroupNotFound.Msg);
                    return BadRequest(Message.MsgGroupNotFound);
                }

                #endregion

                if (_dataRepo.GetMany<GroupFriend>(x => x.Active && x.GroupId == groupFriend.GroupId && x.FriendId == groupFriend.FriendId).Any())
                {
                    Logger.Warn("GroupFriend already exists.");
                    return BadRequest(Message.MsgInvalidData);
                }

                var groupFixedCatValues = _dataRepo.Get<GroupFixedCatValues>(x => x.GroupId == groupFriend.GroupId, true)
                                              ?.GetCatList().ToList() ?? new List<string>();
                var startCatIdx = groupFixedCatValues.Count + 1;

                var group = arrGroups.Single();
                var endCatIdx = group.GetCatDescList().Count() + 1;

                var newGroupFriend = _dataRepo.Get<GroupFriend>(x => x.GroupId == groupFriend.GroupId && x.FriendId == groupFriend.FriendId);
                var groupAdmins = _dataRepo.GetMany<GroupFriend>(
                    x => x.GroupId == groupFriend.GroupId && x.UserRight >= UserType.Admin).Select(x => x.FriendId).ToList();
                var jointGroupAdmins = string.Join(Extension.Sep, groupAdmins.Select(x => $"u{x}"));
                Logger.Debug($"Notification sent to the groupAdmins={jointGroupAdmins}");
                var f = _dataRepo.Get<Friend>(x => x.Id == groupFriend.FriendId);
                var g = _dataRepo.Get<Group>(x => x.Id == groupFriend.GroupId);
                if (newGroupFriend != null)
                {
                    Logger.Debug("GroupFriend exists already. Just modify it");
                    //new subscription --> notify all group's admin
                    _dataRepo.Add(new Notification
                    {
                        CreatedDate = DateTime.Now,
                        OwnerId = groupFriend.FriendId,
                        Destination = $"{jointGroupAdmins},u{groupFriend.FriendId}", //admins and the owner himself
                        NotificationObject = new NotifUpdateSubscriptionRequest
                        {
                            FriendId = groupFriend.FriendId,
                            FriendName = f?.Name,
                            FacebookId = f?.FacebookId,
                            GroupId = groupFriend.GroupId,
                            GroupName = g?.Name
                        }
                    });
                }
                else
                {
                    newGroupFriend = new GroupFriend
                    {
                        FriendId = groupFriend.FriendId,
                        GroupId = groupFriend.GroupId,
                        Active = false,
                        CreatedDate = DateTime.Now,
                        UserRight = UserType.Pending
                    };
                    _dataRepo.Add(newGroupFriend);

                    //new subscription --> notify all group's admin
                    _dataRepo.Add(new Notification
                    {
                        CreatedDate = DateTime.Now,
                        OwnerId = groupFriend.FriendId,
                        Destination = $"{jointGroupAdmins},u{groupFriend.FriendId}", //admins and the owner himself
                        NotificationObject = new NotifNewSubscriptionRequest
                        {
                            FriendId = groupFriend.FriendId,
                            FriendName = f?.Name,
                            FacebookId = f?.FacebookId,
                            GroupId = groupFriend.GroupId,
                            GroupName = g?.Name
                        }
                    });
                }
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

                newGroupFriend.ModifiedDate = DateTime.Now;

                _dataRepo.Commit();

                //remove Cache
                _cacheService.Remove($".GetMyGroups.{groupFriend.FriendId}."); //Active and Inactive
                groupAdmins.ForEach(x =>
                {
                    _cacheService.Remove($".GetGroupFriends.{x}."); // refresh subscription of all Admin
                });
                //there is a change in Notification but we leave the Cache expiration doing the job

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

        [HttpGet]
        [Route("GetConfiguration/{key}/{useCache}")]
        public ActionResult<string> GetConfiguration([FromRoute] string key, bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            ActionResult<string> result = null;
            try
            {
                Logger.Debug($"BEGIN(key={key}, useCache={useCache})");

                var cachePrefix = $"{CacheNameSpace}.{MethodBase.GetCurrentMethod().Name}";
                var cacheTimeout = _cacheService.GetCacheTimeout(_dataRepo, cachePrefix);
                var cacheKey = $"{cachePrefix}.{key}.";
                Logger.Debug($"cacheKey={cacheKey}, cacheTimeout={cacheTimeout}");

                if (useCache)
                {
                    result = _cacheService.Get(cacheKey) as ActionResult<string>;
                    if (result != null)
                    {
                        Logger.Debug("Cache found. Return value in cache.");
                        return result;
                    }
                }

                var config = _dataRepo.Get<Configuration>(x => x.Key == key && x.Enabled, true);
                result = config?.Value;

                //Logger.Debug($"result={JsonConvert.SerializeObject(result)}");
                _cacheService.Set(cacheKey, result, DateTimeOffset.Now.AddMinutes(cacheTimeout));
                return result;
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
        [Route("GetConfigurations/{friendId}/{useCache}")]
        public ActionResult<IEnumerable<Configuration>> GetConfigurations([FromHeader] string token, [FromRoute] int friendId,
            bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            ActionResult<IEnumerable<Configuration>> result = null;
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
                    result = _cacheService.Get(cacheKey) as ActionResult<IEnumerable<Configuration>>;
                    if (result != null)
                    {
                        Logger.Debug("Cache found. Return value in cache.");
                        return result;
                    }
                }

                var res = _dataRepo.GetMany<Configuration>(x => x.Enabled).ToList().Where(
                    x => Regex.Match($"|{friend.DeviceInfo}|{friend.Info}|Email={friend.Email}|FullName={friend.Name}|FriendId={friend.Id}|",
                    x.Rule, RegexOptions.IgnoreCase).Success).ToList();

                //group by key and take only the first elements of each group
                result = (from x in res
                         group x by x.Key
                         into groups
                         select groups.OrderBy(x => x.Order).First()).ToList();

                //Logger.Debug($"result={JsonConvert.SerializeObject(result)}");

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
