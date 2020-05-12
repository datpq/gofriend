namespace goFriend
{
    public class Constants
    {
        public const string MsgProfile = "MsgProfile";
        public const string MsgProfileExt = "MsgProfileExt";
        public const string MsgLocationChanged = "MsgLocationChanged";
        public const string MsgLogout = "MsgLogout";
        public const int SearchCommandDelayTime = 1000; // in milliseconds
        public const int GeolocationRequestTimeout = 5; // in seconds
        public static readonly int[] SuperUserIds = {4, 5};
        public const int ChatMessagePageSize = 10; // number of message in a page (bundle)
        public const int ChatMaxPagesFetched = 10; // maximum number of fetching when there are unread messages to scroll
        public const int ChatMaxPendingMsg = 100;
        public const int ChatMinPendingMsg = 10;
        public const int ChatStartIdxToShowScrollDown = 15;
        public const int ChatStartIdxToHideScrollUp = 10;
    }
}
