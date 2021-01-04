using Foundation;
using goFriend.Services;
using System;
using UserNotifications;
using Xamarin.Forms;

namespace goFriend.iOS
{
    public class iOSNotificationReceiver : UNUserNotificationCenterDelegate
    {
        private ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        public override async void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
        {
            Logger.Debug("WillPresentNotification...");
            //DependencyService.Get<INotificationManager>().ReceiveNotification(notification.Request.Content.Title, notification.Request.Content.Body);
            //var action = (notification.ValueForKey(new NSString(NotificationService.SERVICE_ACTION_KEY)) as NSString).ToString();

            //switch (action)
            //{
            //    case Constants.ACTION_GOTO_MAPONLINE:
            //        await Shell.Current.GoToAsync($"{Constants.ROUTE_HOME_MAPONLINE}");
            //        break;
            //    case Constants.ACTION_GOTO_CHAT:
            //        await Shell.Current.GoToAsync($"//{Constants.ROUTE_CHAT}");
            //        var chatId = notification.ValueForKey(new NSString(NotificationService.SERVICE_EXTRAID_KEY)) as NSString;
            //        await App.DoNotificationAction(action, int.Parse(chatId.ToString()));
            //        break;
            //    case Constants.ACTION_GOTO_HOME:
            //    default:
            //        await Shell.Current.GoToAsync($"//{Constants.ROUTE_HOME}");
            //        break;
            //}

            // alerts are always shown for demonstration but this can be set to "None"
            // to avoid showing alerts if the app is in the foreground
            completionHandler(UNNotificationPresentationOptions.Alert);
        }

        public override async void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
        {
            if (!response.IsDefaultAction) return;

            // Take action based on Action ID
            switch (response.ActionIdentifier)
            {
                case Constants.ACTION_GOTO_MAPONLINE:
                    await Shell.Current.GoToAsync($"//{Constants.ROUTE_MAPONLINE}");
                    break;
                case Constants.ACTION_GOTO_CHAT:
                    await Shell.Current.GoToAsync($"//{Constants.ROUTE_CHAT}");
                    //var chatId = bundle.GetInt(Constants.SERVICE_EXTRAID_KEY);
                    //await App.DoNotificationAction(intent.Action, chatId);
                    break;
                case Constants.ACTION_GOTO_HOME:
                default:
                    await Shell.Current.GoToAsync($"//{Constants.ROUTE_HOME}");
                    break;
            }

            // Inform caller it has been handled
            completionHandler();
        }
    }
}