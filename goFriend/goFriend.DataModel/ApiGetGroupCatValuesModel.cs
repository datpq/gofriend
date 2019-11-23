using System;

namespace goFriend.DataModel
{
    public class ApiGetGroupCatValuesModel
    {
        public string CatValue { get; set; }
        public int MemberCount { get; set; }

        private string _display;
        public string Display
        {
            get => _display ?? $"{CatValue} ({MemberCount})";
            set => _display = value;
        }
    }
}
