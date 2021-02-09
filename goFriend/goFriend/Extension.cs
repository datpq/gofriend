using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using goFriend.Controls;
using goFriend.DataModel;
using goFriend.Services;
using goFriend.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;

namespace goFriend
{
    public static class Extension
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        public static string GetSpentTime(this DateTime dateTime)
        {
            string result;
            var spentTime = DateTime.Now - dateTime.ToLocalTime();
            if (spentTime > TimeSpan.FromDays(3))
            {
                result = $"{spentTime.Days}{res.SpentTimeInDays}";
            } else if (spentTime > TimeSpan.FromHours(1))
            {
                result = $"{spentTime.Days * 12 + spentTime.Hours}{res.SpentTimeInHours}";
            } else
            {
                result = $"{spentTime.Minutes}{res.SpentTimeInMinutes}";
            }
            return result;
        }

        public static FormattedString GetNotificationMessage(this Notification notification, int friendId = 0)
        {
            FormattedString result;
            var groupSubscriptionNotif = notification.NotificationObject as GroupSubscriptionNotifBase;
            switch (notification.Type)
            {
                case NotificationType.NewSubscriptionRequest:
                case NotificationType.UpdateSubscriptionRequest:
                case NotificationType.SubscriptionApproved:
                case NotificationType.SubscriptionRejected:
                    var notifMsg = notification.Type == NotificationType.NewSubscriptionRequest ? res.AskedToJoinGroup :
                        notification.Type == NotificationType.UpdateSubscriptionRequest ? res.NotifUpdateSubscriptionRequest :
                        notification.Type == NotificationType.SubscriptionApproved ? res.NotifSubscriptionApproved : res.NotifSubscriptionRejected;

                    result = new FormattedString
                    {
                        Spans =
                        {
                            new Span
                            {
                                Text = groupSubscriptionNotif?.FriendName, FontAttributes = FontAttributes.Bold,
                                FontSize = (double)Application.Current.Resources["LblDetailFontSize"]
                            },
                            new Span { Text = $" {notifMsg} " },
                            new Span
                            {
                                Text = groupSubscriptionNotif?.GroupName, FontAttributes = FontAttributes.Bold,
                                FontSize = (double)Application.Current.Resources["LblDetailFontSize"]
                            },
                            new Span { Text = Environment.NewLine },
                            new Span { Text = notification.CreatedDate.HasValue ? notification.CreatedDate.Value.GetSpentTime() : string.Empty, LineHeight = 1.3 }
                        }
                    };
                    break;
                default:
                    throw new ArgumentException();
            }
            return result;
        }

        public static async Task<bool> CheckIfLocationIsGranted()
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Lowest, TimeSpan.FromSeconds(Constants.GeolocationRequestTimeout));
            var location = await Geolocation.GetLocationAsync(request);
            return location != null;
        }

        public static async Task<Position> GetPosition(this NetTopologySuite.Geometries.Point point, bool useGpsAsDefault = true)
        {
            if (point != null)
            {
                return new Position(point.Y, point.X);
            }

            if (useGpsAsDefault)
            {
                try
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(Constants.GeolocationRequestTimeout));
                    var location = await Geolocation.GetLocationAsync(request);

                    if (location != null)
                    {
                        return new Position(location.Latitude, location.Longitude);
                    }
                }
                catch (Exception)
                {
                    // GPS disabled
                }
            }

            return MapExtension.DefaultPosition;
        }

        public static string GetVersionTrackingInfo()
        {
            return $"CurrentVersion={VersionTracking.CurrentVersion}.{VersionTracking.CurrentBuild}|PreviousVersion={VersionTracking.PreviousVersion}.{VersionTracking.PreviousBuild}";
        }

        public static string GetDeviceInfo()
        {
            return $"Name={DeviceInfo.Name}|Type={DeviceInfo.DeviceType}|Model={DeviceInfo.Model}|Manufacturer={DeviceInfo.Manufacturer}|Platform={DeviceInfo.Platform}|Version={DeviceInfo.Version}";
        }

        public static Stream GetStreamFromFile(string filename)
        {
            var assembly = typeof(App).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream($"goFriend.{filename}");

            return stream;
        }

        public static async Task<string> GetMemberNames(this Chat chat)
        {
            var arrIds = chat.GetMemberIds();
            var arrNames = new string[arrIds.Length];
            for (var i = 0; i < arrNames.Length; i++)
            {
                var friend = await App.FriendStore.GetFriendInfo(arrIds[i]);
                arrNames[i] = friend.FirstName;
            }
            return string.Join(", ", arrNames);
        }

        public static string GetOverlapImageFromSubscription(this GroupSubscriptionNotifBase subscriptionNotifBase)
        {
            switch (subscriptionNotifBase.Type)
            {
                case NotificationType.SubscriptionApproved:
                    return $"{Constants.IconAccept}{DataModel.Extension.Sep}#61A830";
                case NotificationType.SubscriptionRejected:
                    return $"{Constants.IconRefuse}{DataModel.Extension.Sep}#FF0000";
                case NotificationType.NewSubscriptionRequest:
                    return $"{Constants.IconAddPerson}{DataModel.Extension.Sep}#61A830";
                case NotificationType.UpdateSubscriptionRequest:
                    return $"{Constants.IconUpdateInfo}{DataModel.Extension.Sep}#61A830";
            }
            return null;
        }

        public static async Task<DphListViewItemModel> GetOverlapImageInfo(this Chat chat)
        {
            var result = new DphListViewItemModel();
            if (chat.GetChatType() == ChatType.StandardGroup)
            {
                result.ImageUrl = chat.LogoUrl;
                result.OverlappingImageUrl = null;
                result.OverlapType = OverlapType.Notification;
            }
            else if (chat.GetChatType() == ChatType.Individual)
            {
                foreach (var memberId in chat.GetMemberIds())
                {
                    if (memberId != App.User.Id)
                    {
                        var friend = await App.FriendStore.GetFriendInfo(memberId);
                        result.ImageUrl = friend.GetImageUrl(FacebookImageType.small);
                        break;
                    }
                }
                result.OverlappingImageUrl = null;
                result.OverlapType = OverlapType.Notification;
            }
            else if (chat.GetChatType() == ChatType.MixedGroup)
            {
                var firstImageDone = false;
                foreach (var memberId in chat.GetMemberIds())
                {
                    if (memberId != App.User.Id)
                    {
                        var friend = await App.FriendStore.GetFriendInfo(memberId);
                        var imageUrl = friend.GetImageUrl(FacebookImageType.small);
                        if (!firstImageDone)
                        {
                            result.ImageUrl = imageUrl;
                            firstImageDone = true;
                        } else
                        {
                            result.OverlappingImageUrl = imageUrl;
                            break;
                        }
                    }
                }
                result.OverlapType = OverlapType.GroupChat;
            }

            return result;
        }

        public static bool IsOnlineActive(this FriendLocation friendLocation)
        {
            return friendLocation.ModifiedDate.Value.AddMinutes(Constants.MAPONLINE_ACTIVE_TIMEOUT) >= DateTime.Now;
        }
        public static bool IsOnlineInactive(this FriendLocation friendLocation)
        {
            return friendLocation.ModifiedDate.Value.AddMinutes(Constants.MAPONLINE_ACTIVE_TIMEOUT) < DateTime.Now
                && friendLocation.ModifiedDate.Value.AddMinutes(Constants.MAPONLINE_ONLINE_TIMEOUT) >= DateTime.Now;
        }
        public static bool IsOnline(this FriendLocation friendLocation)
        {
            return friendLocation.ModifiedDate.Value.AddMinutes(Constants.MAPONLINE_ONLINE_TIMEOUT) >= DateTime.Now;
        }
        public static bool IsOffline(this FriendLocation friendLocation)
        {
            return friendLocation.ModifiedDate.Value.AddMinutes(Constants.MAPONLINE_ONLINE_TIMEOUT) < DateTime.Now;
        }
        public static bool IsRefreshNeeded(this FriendLocation friendLocation)
        {
            //if Online Active Timeout is 10 minutes, at the end of 9 minutes, new location need to be sent
            return friendLocation.ModifiedDate.Value.AddMinutes(Constants.MAPONLINE_ACTIVE_TIMEOUT - 1) < DateTime.Now;
        }

        public static bool IsSuperUser(this int friendId)
        {
            return Constants.SuperUserIds.Contains(friendId);
        }

        private const string Marks = ",:;";
        public static string RemoveEndingMark(this string s)
        {
            if (!string.IsNullOrEmpty(s) && Marks.IndexOf(s[s.Length - 1]) >= 0)
            {
                return s.Substring(0, s.Length - 1);
            }
            return s;
        }

        /*
        public static ImageSource GetImageSource(this Friend friend, FacebookImageType imageType = FacebookImageType.normal)
        {
            ImageSource result;
            if (!string.IsNullOrEmpty(friend.FacebookId))
            {
                var url = friend.GetImageUrl(imageType);
                Logger.Debug($"ImageUrl url = {url}");
                //result = ImageSource.FromUri(new Uri(url));
                using (var webClient = new WebClient())
                {
                    var imageBytes = webClient.DownloadData(url);
                    result = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                }
            }
            else
            {
                result = GetImageSourceFromFile(friend.Gender == "female" ? "default_female.jpg" : "default_male.jpg");
            }

            return result;
        }

        public static ImageSource GetImageSourceFromFile(string fileName)
        {
            return ImageSource.FromResource($"goFriend.Images.{fileName}", typeof(ImageResourceExtension).GetTypeInfo().Assembly);
        }
        */
    }
}
