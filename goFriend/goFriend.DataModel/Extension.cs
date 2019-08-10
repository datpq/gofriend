using System;

namespace goFriend.DataModel
{
    public static class Extension
    {
        public static string ToStringStandardFormat(this TimeSpan ts)
        {
            return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
        }
    }
}
