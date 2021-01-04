namespace goFriend
{
    public static class Constants
    {
        public const string MsgProfile = "MsgProfile";
        public const string MsgProfileExt = "MsgProfileExt";
        public const string MsgLocationChanged = "MsgLocationChanged";
        public const string MsgLogout = "MsgLogout";
        public const int SearchCommandDelayTime = 1000; // in milliseconds
        public const int GeolocationRequestTimeout = 5; // in seconds
        public static readonly int[] SuperUserIds = {4, 5};
        public const int ChatMessagePageSize = 10; // number of message in a page (bundle)
        public const int ChatMaxPagesFetched = 5; // maximum number of fetching when there are unread messages to scroll
        public const int ChatMaxPendingMsg = 50;
        public const int ChatMinPendingMsg = 10;
        public const int ChatStartIdxToShowScrollDown = 15;
        public const int ChatStartIdxToHideScrollUp = 10;
        public const int ChatPingFrequence = 5; //in minutes

        public static string ChatFuncUrl { get; set; } = "https://gofriendfuncapp.azurewebsites.net";

        public const string AzureBackendUrl = "https://gofriend.azurewebsites.net";
        public const string AzureBackendUrlDev = "https://gofrienddev.azurewebsites.net";
        public const string AzureBackendUrlChat = "https://gofriendchat.azurewebsites.net";

        public const string ROUTE_HOME = "Home";
        public const string ROUTE_HOME_GROUPCONNECTION = "//Home/GroupConnection";
        public const string ROUTE_HOME_MAP = "//Home/Map";
        public const string ROUTE_HOME_ADMIN = "//Home/Admin";
        public const string ROUTE_HOME_ABOUT = "//Home/About";
        public const string ROUTE_BROWSE = "Browse";
        public const string ROUTE_MAPONLINE = "MapOnline";
        public const string ROUTE_CHAT = "Chat";
        public const string ROUTE_NOTIFICATION = "Notification";

        public const string ImgAccountInfo = "account_info.png";
        public const string ImgGroup = "group.png";
        public const string ImgGroupAdmin = "group_admin";
        public const string ImgCopy = "copy.png";
        public const string ImgDelete = "delete.png";
        public const string ImgMute = "mute.png";
        public const string ImgUnMute = "unmute.png";
        public const string ImgSearch = "search.png";
        public const string ImgAccept = "accept.png";
        public const string ImgDeny = "deny.png";

        public const string IconAccept = "\uF16D";
        public const string IconRefuse = "\uF2BB";
        public const string IconAddPerson = "\uF211";
        public const string IconUpdateInfo = "\uF339";

        public const string IconSend = "\uF1D8";
        public const string IconThumbsUp = "\uF164";

        public const string AppCenterAppSecretiOS = "f2c053eb-a08d-4c4d-8d54-c63711322c18";
        public const string AppCenterAppSecretAndroid = "56010775-c4f6-46f9-9de5-a13a547d22c2";

        //Service
        public const double MOVING_DISTANCE_THRESHOLD = 0.010; // km
        public const double MAPONLINE_DEFAULT_RADIUS = 0.2; // km (200m)
        public const int SERVICE_RUNNING_NOTIFICATION_ID = 12976;
        public const string SERVICE_STARTED_KEY = "service_started";
        public const string SERVICE_EXTRAID_KEY = "service_extraid";
        public const string SERVICE_BROADCAST_MESSAGE_KEY = "service_broadcast_message";
        public const string NOTIFICATION_BROADCAST_ACTION = "GoFriend.Notification.Action";

        public const string ACTION_START_SERVICE = "GoFriend.action.START_SERVICE";
        public const string ACTION_STOP_SERVICE = "GoFriend.action.STOP_SERVICE";
        public const string ACTION_STARTSTOP_TRACING = "GoFriend.action.STOP_TRACING";
        public const string ACTION_GOTO_HOME = "GoFriend.action.GOTO_HOME";
        public const string ACTION_GOTO_MAPONLINE = "GoFriend.action.GOTO_MAPONLINE";
        public const string ACTION_GOTO_CHAT = "GoFriend.action.GOTO_CHAT";
    }
}
