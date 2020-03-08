using System;

namespace goFriend.DataModel
{
    public static class Extension
    {
        public const string ParamSearchText = "SearchText";
        public const string ParamCategory = "Cat";
        public const string ParamOtherFriendId = "OtherFriendId";
        public const string ParamIsActive = "IsActive";

        public static string ToStringStandardFormat(this TimeSpan ts)
        {
            return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
        }
    }
}
