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

        public const string ImgGroup = "group.png";
        public const string ImgCopy = "copy.png";
        public const string ImgDelete = "delete.png";
        public const string ImgSend = "send.png";
        public const string ImgThumbsUp = "thumbsup.png";
        public const string ImgMute = "mute.png";
        public const string ImgUnMute = "unmute.png";
        public const string ImgPhoto = "photo.png";
        public const string ImgCamera = "camera.png";

        public const string AppCenterAppSecretiOS = "f2c053eb-a08d-4c4d-8d54-c63711322c18";
        public const string AppCenterAppSecretAndroid = "56010775-c4f6-46f9-9de5-a13a547d22c2";
    }
}
