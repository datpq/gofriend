namespace goFriend.DataModel
{
    public class ApiGetGroupCatValuesModel
    {
        public string CatValue { get; set; }
        public int MemberCount { get; set; }

        public string Display => $"{CatValue} ({MemberCount})";
    }
}
