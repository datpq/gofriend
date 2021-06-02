using goFriend.DataModel;

namespace goFriend.ViewModels
{
    public class MyGroupViewModel
    {
        public MyGroupViewModel() { }

        public MyGroupViewModel(ApiGetGroupsModel apiGroup)
        {
            Group = apiGroup.Group;
            GroupFriend = apiGroup.GroupFriend;
            UserRight = apiGroup.UserRight;
            MemberCount = apiGroup.MemberCount;
            ChatOwnerId = apiGroup.ChatOwnerId;
        }

        public Group Group { get; set; }
        public GroupFriend GroupFriend { get; set; }
        public UserType UserRight { get; set; }
        public int MemberCount { get; set; }
        public int? ChatOwnerId { get; set; }

        public string Display => Group == null ? res.All : $"{Group.Name} ({MemberCount})";
        public string DisplayNameOnly => Group?.Name;

    }
}
