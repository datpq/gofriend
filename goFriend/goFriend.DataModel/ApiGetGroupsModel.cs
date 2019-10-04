namespace goFriend.DataModel
{
    public class ApiGetGroupsModel
    {
        public Group Group { get; set; }
        public bool IsMember { get; set; }
        public bool IsActiveMember { get; set; }
        public int MemberCount { get; set; }
    }
}
