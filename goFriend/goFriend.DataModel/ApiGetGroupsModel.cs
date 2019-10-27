namespace goFriend.DataModel
{
    public class ApiGetGroupsModel
    {
        public Group Group { get; set; }
        public GroupFriend GroupFriend { get; set; }
        public UserType UserRight { get; set; }
        public int MemberCount { get; set; }
    }
}
