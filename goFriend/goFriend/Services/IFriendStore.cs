using System.Collections.Generic;
using System.Threading.Tasks;
using goFriend.DataModel;
using Microsoft.AspNetCore.SignalR.Client;

namespace goFriend.Services
{
    public interface IFriendStore
    {
        Task<Friend> LoginWithThirdParty(Friend friend, string deviceInfo);
        Task<Friend> LoginWithFacebook(string authToken, string deviceInfo);
        Task<bool> SaveBasicInfo(Friend friend);
        Task<IEnumerable<ApiGetGroupsModel>> GetMyGroups(bool useCache = true);
        Task<IEnumerable<Notification>> GetNotifications(int top = 0, int skip = 0, bool useCache = true);
        Task<IEnumerable<ApiGetGroupsModel>> GetGroups(string searchText = null, bool useCache = true);
        Task<IEnumerable<ApiGetGroupCatValuesModel>> GetGroupCatValues(int groupId, bool useCache = true, params string[] arrCatValues);
        Task<IEnumerable<GroupFriend>> GetGroupFriends(int groupId, bool isActive = true, int top = 0, int skip = 0,
            bool useCache = true, string searchText = null, params string[] arrCatValues);
        Task<GroupFriend> GetGroupFriend(int groupId, int otherFriendId, bool useCache = true);
        Task<GroupFixedCatValues> GetGroupFixedCatValues(int groupId, bool useClientCache = true, bool useCache = true);
        Task<Friend> GetProfile(bool useCache = true);
        Task<Friend> GetFriend(int groupId, int otherFriendId, bool useCache = true);
        Task<bool> ReadNotification(string notifIds);
        Task<bool> GroupSubscriptionReact(int groupFriendId, UserType userRight);
        Task<bool> SubscribeGroup(GroupFriend groupFriend);
        Task<Setting> GetSetting(bool useClientCache = true, bool useCache = true);

        HubConnection ChatHubConnection { get; }
        Task ChatDisconnect();
        Task ChatConnect(ChatJoinChatModel joinChatModel);
        Task SendText(ChatMessage chatMessage);
        Task SendAttachment(ChatMessage chatMessage);
        Task<IEnumerable<ChatMessage>> ChatGetMessages(int chatId, int startMsgIdx, int stopMsgIdx, int pageSize);
        Task<IEnumerable<Chat>> ChatGetChats(bool useCache = true);
    }
}
