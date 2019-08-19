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
            var stopWatch = Stopwatch.StartNew();
            if (Logger.IsDebugEnabled)// && Request != null
            {
                Logger.Debug($"BEGIN({friend})");
            }
            try
            {
                if (friend == null || !ModelState.IsValid)
                {
                    return BadRequest("Invalid State");
                }

                Friend result;
                if (friend.FacebookId != null)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Info($"Facebook logged in: {friend}");
                    }
                    result = _dataRepo.Get<Friend>(x => x.FacebookId == friend.FacebookId);
                    if (result == null)
                    {
                        lock (_dataRepo)
                        {
                            result = _dataRepo.Get<Friend>(x => x.FacebookId == friend.FacebookId);
                            if (result == null)
                            {
                                result = (Friend)friend.Clone();
                                _dataRepo.Add(result);
                                Logger.Info($"New user registered: {result}");
                            }
                            else
                            {
                                Logger.Warn("Duplicate request happend. The second data counts");
                                friend.CopyTo(result);
                            }
                        }
                    }
                    else
                    {
                        friend.CopyTo(result);
                        if (Logger.IsDebugEnabled)
                        {
                            Logger.Debug($"Update info on user: {result}");
                        }
                    }
                }
                else
                {
                    result = (Friend)friend.Clone();
                    _dataRepo.Add(result);
                    Logger.Info($"New user registered: {result}");
                }
                _dataRepo.Commit();
                return Ok(result);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error while creating or updating");
                return BadRequest("Error while creating or updating");
            }
            finally
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug($"END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
                }
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
