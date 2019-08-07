using System.Collections.Generic;
using goFriend.MobileAppService.Data;
using goFriend.MobileAppService.Models;
using Microsoft.AspNetCore.Mvc;

namespace goFriend.MobileAppService.Controllers
{
    [Produces("application/json")]
    [Route("api/Friend")]
    public class FriendController : Controller
    {
        private readonly IDataRepository _dataRepo;

        public FriendController(IDataRepository dataRepo)
        {
            _dataRepo = dataRepo;
        }

        // GET: api/Friend
        [HttpGet]
        public IEnumerable<Friend> Get()
        {
            var result = _dataRepo.GetAll<Friend>();
            return result;
        }

        // GET: api/Friend/5
        [HttpGet("{id}", Name = "Get")]
        public Friend Get(int id)
        {
            var result = _dataRepo.Get<Friend>(x => x.Id == id);
            return result;
        }
        
        // POST: api/Friend
        [HttpPost]
        public void Post([FromBody]string value)
        {
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
