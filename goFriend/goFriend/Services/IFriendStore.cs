using System.Collections.Generic;
using System.Threading.Tasks;
using goFriend.DataModel;
using goFriend.ViewModels;

namespace goFriend.Services
{
    public interface IFriendStore
    {
        Task<Friend> LoginWithThirdParty(Friend friend, string deviceInfo);
        Task<Friend> LoginWithFacebook(string authToken, string deviceInfo);
        Task<bool> SaveBasicInfo(Friend friend);
        Task<IEnumerable<MyGroupViewModel>> GetMyGroups(bool useClientCache = true, bool useCache = true);
        Task<IEnumerable<Notification>> GetNotifications(int top = 0, int skip = 0, bool useCache = true);
        Task<IEnumerable<MyGroupViewModel>> GetGroups(string searchText = null, bool useClientCache = true, bool useCache = true);
        Task<IEnumerable<ApiGetGroupCatValuesModel>> GetGroupCatValues(int groupId, bool useClientCache = true, bool useCache = true, params string[] arrCatValues);
        Task<IEnumerable<Friend>> GetFriends(string searchText, bool isActive = true, int top = 0, int skip = 0, bool useCache = true);
        Task<IEnumerable<GroupFriend>> GetGroupFriends(int groupId, bool isActive = true, int top = 0, int skip = 0,
            bool useClientCache = true, bool useCache = true, string searchText = null, params string[] arrCatValues);
        Task<GroupFriend> GetGroupFriend(int groupId, int otherFriendId, bool useClientCache = true, bool useCache = true);
        Task<GroupFixedCatValues> GetGroupFixedCatValues(int groupId, bool useClientCache = true, bool useCache = true);
        Task<Friend> GetProfile(bool useClientCache = true, bool useCache = true);
        Task<Friend> GetFriend(int groupId, int otherFriendId, bool useClientCache = true, bool useCache = true);
        Task<Friend> GetFriendInfo(int otherFriendId, bool useClientCache = true, bool useCache = true);
        Task<bool> ReadNotification(string notifIds);
        Task<bool> GroupSubscriptionReact(int groupFriendId, UserType userRight);
        Task<bool> SubscribeGroupMultiple(int groupId, Friend[] friends);
        Task<bool> SubscribeGroup(GroupFriend groupFriend);
        Task<Setting> GetSetting(bool useClientCache = true, bool useCache = true);
        Task<string> GetConfiguration(string key, bool useClientCache = true, bool useCache = true);
        Task<IEnumerable<Configuration>> GetConfigurations(bool useClientCache = true, bool useCache = true);

        SignalRService SignalR { get; }
        Task SendText(ChatMessage chatMessage);
        Task SendAttachment(ChatMessage chatMessage);
        Task<IEnumerable<ChatFriendOnline>> SendPing(int chatId);
        Task SendCreateChat(Chat chat);
        Task SendLocation(double latitude, double longitude);
        Task<IEnumerable<ChatMessage>> ChatGetMessages(int chatId, int startMsgIdx, int stopMsgIdx, int pageSize);
        Task<IEnumerable<Chat>> ChatGetChats(bool useCache = true);
    }
}
