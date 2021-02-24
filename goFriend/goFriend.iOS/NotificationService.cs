using Foundation;
using goFriend.iOS;
using goFriend.Models;
using goFriend.Services;
using UserNotifications;
using Xamarin.Forms;

[assembly: Dependency(typeof(NotificationService))]
namespace goFriend.iOS
{
    public class NotificationService : INotificationService
    {
        private bool _hasNotificationsPermission = false;
        private ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        public NotificationService()
        {
            // request the permission to use local notifications
            UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert, (approved, err) =>
            {
                _hasNotificationsPermission = approved;
            });
        }

        public void CancelNotification(NotificationType? notificationType = null)
        {
            return;
        }

        public void SendNotification(ServiceNotification serviceNotification)
        {
            if (serviceNotification == null) return;

            Logger.Debug($"SendNotification.BEGIN(NotificationType={serviceNotification?.NotificationType})");

            // EARLY OUT: app doesn't have permissions
            if (!_hasNotificationsPermission)
            {
                Logger.Warn("No notification permission granted.");
                return;
            }

            var content = new UNMutableNotificationContent()
            {
                Title = serviceNotification.ContentTitle,
                Subtitle = string.Empty,
                Body = serviceNotification.GetContentBody(),
                UserInfo = NSDictionary.FromObjectAndKey(new NSString(
                    serviceNotification.ExtraId.ToString()), new NSString(Constants.SERVICE_EXTRAID_KEY)),
                Badge = 1
            };

            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(0.25, false);
            var request = UNNotificationRequest.FromIdentifier(serviceNotification.NotificationType.ToString(), content, trigger);

            UNUserNotificationCenter.Current.AddNotificationRequest(request, (err) =>
            {
                if (err != null)
                {
                    // Report error
                    Logger.Error("Error: {0}", err);
                }
                else
                {
                    // Report Success
                    Logger.Debug("Notification Scheduled: {0}", request);
                }
            });

            Logger.Debug($"SendNotification.END");
        }
    }
}