using System;
using System.Threading.Tasks;
using goFriend.Controls;
using goFriend.DataModel;
using goFriend.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace goFriend
{
    public enum FacebookImageType
    {
        small, // 50 x 50
        normal, // 100 x 100
        album, // 50 x 50
        large, // 200 x 200
        square // 50 x 50
    }

    public static class Extension
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public static string GetImageUrl(this Friend friend, FacebookImageType imageType = FacebookImageType.normal)
        {
            string result;
            if (!string.IsNullOrEmpty(friend.FacebookId))
            {
                result = $"https://graph.facebook.com/{friend.FacebookId}/picture?type={imageType}";
                Logger.Debug($"URL = {result}");
            }
            else if (friend.ThirdPartyLogin == ThirdPartyLogin.Apple)
            {
                result = GetImageUrl("apple.jpg");
            }
            else
            {
                result = friend.Gender == "female" ? "default_female.jpg" : "default_male.jpg";
                result = GetImageUrl(result);
            }

            return result;
        }

        public static string GetImageUrl(string fileName)
        {
            return $"resource://goFriend.Images.{fileName}";
        }

        public static string GetImageUrlByFacebookId(string facebookId, FacebookImageType imageType = FacebookImageType.normal)
        {
            string result;
            if (!string.IsNullOrEmpty(facebookId))
            {
                result = $"https://graph.facebook.com/{facebookId}/picture?type={imageType}";
                Logger.Debug($"URL = {result}");
            }
            else
            {
                result = GetImageUrl("default_male.jpg");
            }
            return result;
        }

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

            return DphMap.DefaultPosition;
        }

        public static string GetVersionTrackingInfo()
        {
            return $"CurrentVersion={VersionTracking.CurrentVersion}.{VersionTracking.CurrentBuild}|PreviousVersion={VersionTracking.PreviousVersion}.{VersionTracking.PreviousBuild}";
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
