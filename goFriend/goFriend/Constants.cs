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
        public const int ChatMaxPagesFetched = 5; // maximum number of fetching when there are unread messages to scroll
        public const int ChatMaxPendingMsg = 50;
        public const int ChatMinPendingMsg = 10;
        public const int ChatStartIdxToShowScrollDown = 15;
        public const int ChatStartIdxToHideScrollUp = 10;
        public const int ChatPingFrequence = 5; //in minutes

        public const string AzureBackendUrl = "https://gofriend.azurewebsites.net";
        public const string AzureBackendUrlDev = "https://gofrienddev.azurewebsites.net";
        public const string AzureBackendUrlChat = "https://gofriendchat.azurewebsites.net";

        public const string ImgFolderOpen = "folder_open.png";
        public const string ImgFolderClose = "folder_close.png";
        public const string ImgAccountInfo = "account_info.png";
        public const string ImgGroup = "group.png";
        public const string ImgGroupAdmin = "group_admin";
        public const string ImgCopy = "copy.png";
        public const string ImgDelete = "delete.png";
        public const string ImgSend = "send.png";
        public const string ImgThumbsUp = "thumbsup.png";
        public const string ImgMute = "mute.png";
        public const string ImgUnMute = "unmute.png";
        public const string ImgPhoto = "photo.png";
        public const string ImgCamera = "camera.png";
        public const string ImgSearch = "search.png";
        public const string ImgAccept = "accept.png";
        public const string ImgDeny = "deny.png";

        public const string AppCenterAppSecretiOS = "f2c053eb-a08d-4c4d-8d54-c63711322c18";
        public const string AppCenterAppSecretAndroid = "56010775-c4f6-46f9-9de5-a13a547d22c2";
    }
}
