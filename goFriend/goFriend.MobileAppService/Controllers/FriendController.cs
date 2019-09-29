using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Facebook;
using goFriend.DataModel;
using goFriend.MobileAppService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NLog;

namespace goFriend.MobileAppService.Controllers
{
    [Produces("application/json")]
    [Route("api/Friend")]
    public class FriendController : Controller
    {
        private readonly IDataRepository _dataRepo;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Message MsgMissingToken = new Message { Code = MessageCode.MissingToken, Msg = "Missing Token" };
        private static readonly Message MsgInvalidState = new Message { Code = MessageCode.InvalidState, Msg = "Invalid State" };
        private static readonly Message MsgFacebookIdNull = new Message { Code = MessageCode.FacebookIdNull, Msg = "FacebookId is null" };
        private static readonly Message MsgIdOrEmailNull = new Message { Code = MessageCode.IdOrEmailNull, Msg = "Id or Email is null" };
        private static readonly Message MsgUnknown = new Message { Code = MessageCode.Unknown, Msg = "Unknown error" };
        private static readonly Message MsgUserNotFound = new Message { Code = MessageCode.UserNotFound, Msg = "User not found." };
        private static readonly Message MsgWrongToken = new Message { Code = MessageCode.UserTokenError, Msg = "Wrong Token" };

        public FriendController(IDataRepository dataRepo)
        {
            _dataRepo = dataRepo;
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

        /**
         * return a collection of (Group, IsSubscribed, IsSubscriptionActive)
         * Group, false, false ==> user is not member of this group
         * Group, true, false ==> user has already subscribed to this group, but the membership is not active yet
         * Group, true, true ==> user is totally active is this group
         */
        [HttpGet]
        [Route("GetGroups/{friendId}")]
        public ActionResult<IEnumerable<Tuple<Group, bool, bool>>> GetGroups([FromRoute] int friendId, [FromHeader] string token)
        {
            var stopWatch = Stopwatch.StartNew();
            ActionResult<IEnumerable<Tuple<Group, bool, bool>>> result = null;
            try
            {
                Logger.Debug($"BEGIN(friendId={friendId}, token={token})");
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
                result = registeredGroups.Select(x => new Tuple<Group, bool, bool>(x.Group, true, x.Active))
                    .Union(_dataRepo.GetMany<Group>(x => x.Active && !registeredGroups.Select(y => y.GroupId).Contains(x.Id))
                        .Select(x => new Tuple<Group, bool, bool>(x, false, false))).ToList();
                Logger.Debug($"result={JsonConvert.SerializeObject(result)}");
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
