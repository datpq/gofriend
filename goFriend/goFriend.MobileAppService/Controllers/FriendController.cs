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
        public IActionResult AddOrUpdate([FromBody]Friend friend)
        {
            try
            {
                var stopWatch = Stopwatch.StartNew();
                if (Logger.IsDebugEnabled && Request != null)
                {
                    Logger.Debug($"BEGIN({friend})");
                }
                if (friend == null || !ModelState.IsValid)
                {
                    return BadRequest("Invalid State");
                }

                Friend result;
                if (friend.FacebookId != null)
                {
                    result = _dataRepo.Get<Friend>(x => x.FacebookId == friend.FacebookId);
                    if (result != null)
                    {
                        result.Name = friend.Name;
                        result.FirstName = friend.FirstName;
                        result.LastName = friend.LastName;
                        result.MiddleName = friend.MiddleName;
                        result.Birthday = friend.Birthday;
                        result.Gender = friend.Gender;
                        result.Email = friend.Email;
                    }
                    else
                    {
                        result = new Friend
                        {
                            Name = friend.Name, FirstName = friend.FirstName, LastName = friend.LastName, MiddleName = friend.MiddleName,
                            Birthday = friend.Birthday, Gender = friend.Gender, Email = friend.Email, FacebookId = friend.FacebookId
                        };
                        _dataRepo.Add(result);
                    }
                    _dataRepo.Commit();
                }
                else
                {
                    result = new Friend
                    {
                        Name = friend.Name,
                        FirstName = friend.FirstName,
                        LastName = friend.LastName,
                        MiddleName = friend.MiddleName,
                        Birthday = friend.Birthday,
                        Gender = friend.Gender,
                        Email = friend.Email,
                        FacebookId = friend.FacebookId
                    };
                    _dataRepo.Add(result);
                    _dataRepo.Commit();
                }
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug($"END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
                }
                return Ok(result);
            }
            catch (Exception)
            {
                return BadRequest("Error while creating or updating");
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
