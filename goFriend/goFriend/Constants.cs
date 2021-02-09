using goFriend.Services;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace goFriend
{
    public static class Constants
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        static Constants()
        {
            res.Culture = new CultureInfo("vi-VN");
            GenderDictionary = new Dictionary<string, string>()
            {
                { "male", res.Male },
                { "female",  res.Female }
            };
            SourceGenders = GenderDictionary.Values.ToArray();
            RelationshipDictionary = new Dictionary<string, string>()
            {
                { "single", res.RelationshipSingle },
                { "married",  res.RelationshipMarried },
                { "engaged",  res.RelationshipEngaged },
                { "divorced", res.RelationshipDivorced },
                { "separated",  res.RelationshipSeparated },
                { "complicated",  res.RelationshipComplicated }
            };
            SourceRelationships = RelationshipDictionary.Values.ToArray();
        }

        public const string MsgProfile = "MsgProfile";
        public const string MsgProfileExt = "MsgProfileExt";
        public const string MsgLocationChanged = "MsgLocationChanged";
        public const string MsgLogout = "MsgLogout";
        public const int ListViewPageSize = 10;
        public const int MinimumClusterSize = 20;
        public const int SearchCommandDelayTime = 1000; // in milliseconds
        public const int GeolocationRequestTimeout = 5; // in seconds
        public static int[] SuperUserIds = { 4, 5 };
        public const int ChatMessagePageSize = 10; // number of message in a page (bundle)
        public const int ChatMaxPagesFetched = 5; // maximum number of fetching when there are unread messages to scroll
        public const int ChatMaxPendingMsg = 50;
        public const int ChatMinPendingMsg = 10;
        public const int ChatStartIdxToShowScrollDown = 15;
        public const int ChatStartIdxToHideScrollUp = 10;
        public const int ChatPingFrequence = 5; //in minutes
        public const int AccountOnAppearingTimeout = 5; //in minutes

        public static string ChatFuncUrl { get; set; } = "https://gofriendfuncapp.azurewebsites.net";

        public const string AzureBackendUrl = "https://gofriend.azurewebsites.net";
        public const string AzureBackendUrlDev = "https://gofrienddev.azurewebsites.net";
        public const string AzureBackendUrlChat = "https://gofriendchat.azurewebsites.net";
        public static string BackendUrl = AzureBackendUrl;
        public static string HomePageUrl = "http://gofriend.azurewebsites.net";

        public const string CacheTimeoutPrefix = "CacheTimeout.";
        private static Dictionary<string, int> CacheTimeoutDict = new Dictionary<string, int>();

        public const string ROUTE_HOME = "Home";
        public const string ROUTE_HOME_GROUPCONNECTION = "//Home/GroupConnection";
        public const string ROUTE_HOME_MAP = "//Home/Map";
        public const string ROUTE_HOME_ADMIN = "//Home/Admin";
        public const string ROUTE_HOME_LOGIN = "//Home/Login";
        public const string ROUTE_HOME_ABOUT = "//Home/About";
        public const string ROUTE_BROWSE = "Browse";
        public const string ROUTE_MAPONLINE = "MapOnline";
        public const string ROUTE_CHAT = "Chat";
        public const string ROUTE_NOTIFICATION = "Notification";

        public static Dictionary<string, string> GenderDictionary;
        public static Dictionary<string, string> RelationshipDictionary;
        public static string[] SourceGenders;
        public static string[] SourceRelationships;

        public const string ImgAccountInfo = "account_info.png";
        public const string ImgGroup = "group.png";
        public const string ImgGroupAdmin = "group_admin";
        public const string ImgCopy = "copy.png";
        public const string ImgDelete = "delete.png";
        public const string ImgMute = "mute.png";
        public const string ImgUnMute = "unmute.png";
        public const string ImgAccept = "accept.png";
        public const string ImgDeny = "deny.png";

        //DphOverlapImage.xaml ionicons.ttf#Ionicons
        public const string IconAccept = "\uF16D";
        public const string IconRefuse = "\uF2BB";
        public const string IconAddPerson = "\uF211";
        public const string IconUpdateInfo = "\uF339";

        public static string IconFontFamily = Device.RuntimePlatform == Device.Android ?
            "fa-solid-900.ttf#Font Awesome 5 Free" : "Font Awesome 5 Free";
        public static string IoniconsFontFamily = Device.RuntimePlatform == Device.Android ?
            "ionicons.ttf#Ionicons" : "Ionicons";
        //ChatInputBarView.xaml fa-solid-900.ttf#Font Awesome 5 Free
        public const string IconChatGroup = "\uF086";
        public const string IconSend = "\uF1D8";
        public const string IconThumbsUp = "\uF164";
        public const string IconOK = "\uF00C";
        public static ImageSource IconSearch = new FontImageSource
        {
            Glyph = "\uF002",
            Color = Color.FromHex("#61A830"),
            Size = 26,
            FontFamily = IconFontFamily
        };
        public static ImageSource IconMnuMore = new FontImageSource
        {
            Glyph = "\uF142",
            Size = 18,
            FontFamily = IconFontFamily
        };

        public const string AppCenterAppSecretiOS = "f2c053eb-a08d-4c4d-8d54-c63711322c18";
        public const string AppCenterAppSecretAndroid = "56010775-c4f6-46f9-9de5-a13a547d22c2";

        //Service
        public static int LOCATIONSERVICE_UPDATE_INTERVAL = 30; // in seconds
        public static double LOCATIONSERVICE_DISTANCE_THRESHOLD = 10; // meter
        public static double MAPONLINE_DEFAULT_RADIUS = 0.2; // km (200m)
        public static int MAPONLINE_ACTIVE_TIMEOUT = 10; // in minutes
        public static int MAPONLINE_ONLINE_TIMEOUT = 30; // in minutes
        public static int MAPONLINE_OFFLINE_TIMEOUT = 30; // in minutes
        public static int MAPONLINE_REFRESH_INTERVAL = 10; // in seconds
        public static int MAPONLINE_COMMAND_DISABLED_TIMEOUT = 60; // in seconds
        public static double[] MAPONLINE_RADIUS_LIST = { 0.2, 0.5, 1, 10, 0 };
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

        public static async Task InitializeConfiguration()
        {
            Logger.Debug("InitializeConfiguration.BEGIN");
            var configurations = await App.FriendStore.GetConfigurations();
            configurations.ToList().ForEach(x =>
            {
                Logger.Debug($"Key={x.Key}, Value={x.Value}");
                switch(x.Key)
                {
                    case "MAPONLINE_ACTIVE_TIMEOUT":
                        MAPONLINE_ACTIVE_TIMEOUT = int.Parse(x.Value);
                        break;
                    case "MAPONLINE_ONLINE_TIMEOUT":
                        MAPONLINE_ONLINE_TIMEOUT = int.Parse(x.Value);
                        break;
                    case "MAPONLINE_OFFLINE_TIMEOUT":
                        MAPONLINE_OFFLINE_TIMEOUT = int.Parse(x.Value);
                        break;
                    case "SuperUserIds":
                        SuperUserIds = x.Value.Split(";").Where(
                            x => !string.IsNullOrEmpty(x)).Select(x => int.Parse(x)).ToArray();
                        break;
                    case "MAPONLINE_DEFAULT_RADIUS":
                        MAPONLINE_DEFAULT_RADIUS = double.Parse(x.Value);
                        break;
                    case "MAPONLINE_COMMAND_DISABLED_TIMEOUT":
                        MAPONLINE_COMMAND_DISABLED_TIMEOUT = int.Parse(x.Value);
                        break;
                    case "MAPONLINE_REFRESH_INTERVAL":
                        MAPONLINE_REFRESH_INTERVAL = int.Parse(x.Value);
                        break;
                    case "MAPONLINE_RADIUS_LIST":
                        MAPONLINE_RADIUS_LIST = x.Value.Split(";").Where(
                            x => !string.IsNullOrEmpty(x)).Select(x => double.Parse(x)).ToArray();
                        break;
                    case "LOCATIONSERVICE_UPDATE_INTERVAL":
                        LOCATIONSERVICE_UPDATE_INTERVAL = int.Parse(x.Value);
                        break;
                    case "LOCATIONSERVICE_DISTANCE_THRESHOLD":
                        LOCATIONSERVICE_DISTANCE_THRESHOLD = double.Parse(x.Value);
                        break;
                    case "HomePageUrl":
                        HomePageUrl = x.Value;
                        break;
                    default:
                        if (x.Key.StartsWith(CacheTimeoutPrefix))
                        {
                            //CacheTimeoutDict.Add(x.Key, int.Parse(x.Value));
                            CacheTimeoutDict[x.Key] = int.Parse(x.Value);
                        }
                        else
                        {
                            Logger.Warn("Key not managed.");
                        }
                        break;
                }
            });
            Logger.Debug("InitializeConfiguration.END");
        }

        public static int GetCacheTimeout(string cacheKey)
        {
            if (!CacheTimeoutDict.ContainsKey(cacheKey))
            {
                Logger.Warn($"Cache key not found: {cacheKey}. Use default value");
                return 180;
            }
            return CacheTimeoutDict[cacheKey];
        }
    }
}
