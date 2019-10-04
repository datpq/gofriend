using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using goFriend.DataModel;

namespace goFriend.Services
{
    public interface IFriendStore
    {
        Task<Friend> LoginWithFacebook(string authToken, string deviceInfo);
        Task<bool> SaveBasicInfo(Friend friend);
        Task<IEnumerable<ApiGetGroupsModel>> GetGroups(bool useCache = true);
        Task<GroupCategory> GetGroupCategory(int groupId, bool useCache = true);
    }
}
