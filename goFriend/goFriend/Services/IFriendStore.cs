﻿using System.Collections.Generic;
using System.Threading.Tasks;
using goFriend.DataModel;

namespace goFriend.Services
{
    public interface IFriendStore
    {
        Task<Friend> LoginWithFacebook(string authToken, string deviceInfo);
        Task<bool> SaveBasicInfo(Friend friend);
        Task<IEnumerable<ApiGetGroupsModel>> GetMyGroups(bool useCache = true);
        Task<IEnumerable<ApiGetGroupsModel>> GetGroups(string searchText = null, bool useCache = true);
        Task<IEnumerable<ApiGetGroupCatValuesModel>> GetGroupCatValues(int groupId, bool useCache = true, params string[] arrCatValues);
        Task<IEnumerable<GroupFriend>> GetGroupFriends(int groupId, bool isActive = true, bool useCache = true, params string[] arrCatValues);
        Task<GroupFixedCatValues> GetGroupFixedCatValues(int groupId, bool useCache = true);
        Task<Friend> GetFriend(int groupId, int otherFriendId, bool useCache = true);
        Task<bool> SubscribeGroup(GroupFriend groupFriend);
    }
}
