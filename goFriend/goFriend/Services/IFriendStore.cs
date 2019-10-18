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
        Task<IEnumerable<ApiGetGroupCatValuesModel>> GetGroupCatValues(int groupId, bool useCache = true, params string[] arrCatValues);
        Task<GroupFixedCatValues> GetGroupFixedCatValues(int groupId, bool useCache = true);
    }
}
