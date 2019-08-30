using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using goFriend.DataModel;
using goFriend.MobileAppService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace goFriend.MobileAppService.Controllers
{
    [Produces("application/json")]
    [Route("api/Friend")]
    public class FriendController : Controller
    {
        private readonly IDataRepository _dataRepo;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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

        [HttpPost]
        [Route("LoginWithFacebook")]
        public ActionResult<Friend> LoginWithFacebook([FromBody] Friend friend)
        {
            var stopWatch = Stopwatch.StartNew();
            Logger.Debug($"BEGIN({friend})");
            try
            {
                if (friend == null || !ModelState.IsValid)
                {
                    return BadRequest(new Message{ Code = MessageCode.InvalidState, Msg = "Invalid State" });
                } else if (friend.FacebookId == null)
                {
                    return BadRequest(new Message { Code = MessageCode.FacebookIdNull, Msg = "FacebookId is null" });
                }
                var result = _dataRepo.Get<Friend>(x => x.FacebookId == friend.FacebookId);
                if (result == null) //register with facebook
                {
                    lock (_dataRepo)
                    {
                        result = _dataRepo.Get<Friend>(x => x.FacebookId == friend.FacebookId);
                        if (result == null)
                        {
                            result = (Friend)friend.Clone();
                            result.Active = true; // new user is always not active
                            _dataRepo.Add(result);
                            Logger.Debug($"New user registered: {result}");
                        }
                        else
                        {
                            Logger.Warn("Duplicate request happened. The second data counts");
                            friend.CopyToIfNull(result); //CreatedDate does not change
                        }
                    }
                }
                else//already registered. Logged-in again
                {
                    friend.CopyToIfNull(result);
                    Logger.Debug($"Update info on user: {result}");
                }
                _dataRepo.Commit();
                //return Ok(result);
                return result;
            }
            catch (Exception e)
            {
                Logger.Error(e, "FacebookLogin error");
                return BadRequest(new Message { Code = MessageCode.Unknown, Msg = "Unknown FacebookLogin error" });
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
                    return BadRequest(new Message { Code = MessageCode.InvalidState, Msg = "Invalid State" });
                }
                else if (friend.Id == 0 || string.IsNullOrEmpty(friend.Email))
                {
                    return BadRequest(new Message { Code = MessageCode.IdOrEmailNull, Msg = "Id or Email is null" });
                }
                var result = _dataRepo.Get<Friend>(x => x.Id == friend.Id);
                if (result == null)
                {
                    return BadRequest(new Message { Code = MessageCode.UserNotFound, Msg = "User not found." });
                }

                result.Name = friend.Name;
                result.FirstName = friend.FirstName;
                result.LastName = friend.LastName;
                result.MiddleName = friend.MiddleName;
                result.Birthday = friend.Birthday;
                result.Gender = friend.Gender;
                result.ModifiedDate = DateTime.Now;
                Logger.Debug($"SaveBasicInfo ok: {result}");
                _dataRepo.Commit();
                return Ok();
            }
            catch (Exception e)
            {
                Logger.Error(e, "SaveBasicInfo error");
                return BadRequest(new Message { Code = MessageCode.Unknown, Msg = "Unknown SaveBasicInfo error" });
            }
            finally
            {
                Logger.Debug($"END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
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
