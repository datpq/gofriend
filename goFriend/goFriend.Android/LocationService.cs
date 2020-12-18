using Android.App;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Text;
using Android.Text.Style;
using goFriend.Droid;
using goFriend.Models;
using goFriend.Services;
using Microsoft.Extensions.Caching.Memory;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Color = Android.Graphics.Color;

[assembly: Dependency(typeof(LocationService))]
namespace goFriend.Droid
{
    public class LocationService : ILocationService
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());
        private IMemoryCache _memoryCache;

        public static MainActivity MainActivity;
        public static LocationForegroundService LocationForegroundService;
        public static AsyncAutoResetEvent StandByWhenStart = new AsyncAutoResetEvent();
        public static bool IsTracing = false;

        const string CacheBitmapPrefix = "CacheBitmap.";

        public LocationService()
        {
            Logger.Debug($"LocationService.BEGIN");
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            Logger.Debug($"LocationService.END");
        }

        public event EventHandler StateChanged;

        private async Task Start(bool startOrPause)
        {
            Logger.Debug($"Start.BEGIN(startOrPause={startOrPause})");
            if (!MainActivity.IsServiceStarted)
            {
                Logger.Debug("Service's not started. Starting now...");
                MainActivity.StartService(MainActivity.StartServiceIntent);
                MainActivity.IsServiceStarted = true;
                StandByWhenStart = new AsyncAutoResetEvent();
                await StandByWhenStart.WaitAsync().ConfigureAwait(true);
            }
            if (LocationForegroundService != null)
            {
                IsTracing = startOrPause;
                await LocationForegroundService.StartForegroundService();
                StateChanged?.Invoke(this, null);
            }
            else
            {
                Logger.Error("Service started but LocationForegroundService is null");
            }
            Logger.Debug("Start.END");
        }

        public async Task Start()
        {
            await Start(true);
        }

        public async Task Pause()
        {
            Logger.Debug($"Pause.BEGIN");

            IsTracing = !IsTracing;
            Logger.Debug("Tracing is " + (IsTracing ? "on" : "off"));

            await Start(IsTracing); // update just the interface (Activate and Stop button)

            Logger.Debug($"Pause.END");
        }

        public async Task Stop()
        {
            Logger.Debug("Stop.BEGIN");
            if (LocationForegroundService != null)
            {
                await LocationForegroundService.StopForegroundService();
                MainActivity.IsServiceStarted = false;
                IsTracing = false;
                StateChanged?.Invoke(this, null);
            }
            else
            {
                Logger.Error("Stopping but LocationForegroundService is null");
            }
            Logger.Debug("Stop.END");
        }

        public bool IsRunning()
        {
            //return MainActivity.IsServiceStarted;
            return MainActivity.IsServiceStarted && IsTracing;
        }

        public void CancelNotification(NotificationType? notificationType = null)
        {
            if (LocationForegroundService != null)
            {
                if (notificationType.HasValue)
                {
                    LocationForegroundService.NotificationManager.Cancel((int)notificationType.Value);
                }
                else
                {
                    LocationForegroundService.NotificationManager.CancelAll();
                }
            }
            else
            {
                Logger.Error("Canceling notification but LocationForegroundService is null");
            }
        }

        public void SendNotification(ServiceNotification serviceNotification)
        {
            if (serviceNotification == null || !IsRunning() || LocationForegroundService == null) return;

            Logger.Debug($"SendNotification.BEGIN(NotificationType={serviceNotification?.NotificationType})");
            var builder = new Notification.Builder(LocationForegroundService)
                    .SetColor(new Color(ContextCompat.GetColor(LocationForegroundService, Resource.Color.colorPrimary)))
                    .SetContentTitle(serviceNotification.ContentTitle)
                    .SetContentText(serviceNotification.ContentText)
                    .SetSmallIcon(Resource.Drawable.hn9194_25)
                    .SetShowWhen(true)
                    .SetAutoCancel(true);

            try
            {
                var cacheKey = $"{CacheBitmapPrefix}{serviceNotification.LargeIconUrl}.";
                var circleLargeIcon = _memoryCache.Get(cacheKey) as Bitmap;
                if (circleLargeIcon == null)
                {
                    Logger.Debug($"First time getting url: {serviceNotification.LargeIconUrl}");
                    var largeIcon = ExtensionAndroid.GetImageBitmapFromUrl(serviceNotification.LargeIconUrl);
                    circleLargeIcon = largeIcon.CreateRoundedBitmap(/*10*/);
                    Logger.Debug($"CreateRoundedBitmap done.");
                    _memoryCache.Set(cacheKey, circleLargeIcon, DateTimeOffset.Now.AddDays(1));
                }

                builder.SetLargeIcon(circleLargeIcon);
                //builder.SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.hn9194_25));
            }
            catch (Exception e)
            {
                Logger.TrackError(e);
            }

            switch (serviceNotification.NotificationType)
            {
                case NotificationType.AppearOnMap:
                    builder.SetContentIntent(LocationForegroundService.BuildIntentToShowMainActivity(Constants.ACTION_GOTO_MAPONLINE));
                    BuildNotificationStyle(builder, serviceNotification);
                    break;
                case NotificationType.ChatReceiveMessage:
                    builder.SetContentIntent(LocationForegroundService.BuildIntentToShowMainActivity(
                        Constants.ACTION_GOTO_CHAT, serviceNotification.ExtraId));
                    BuildNotificationStyle(builder, serviceNotification);
                    break;
                case NotificationType.ChatReceiveCreateChat:
                    builder.SetContentIntent(LocationForegroundService.BuildIntentToShowMainActivity(
                        Constants.ACTION_GOTO_CHAT, serviceNotification.ExtraId));
                    var textStyle = new Notification.BigTextStyle();

                    textStyle.BigText(serviceNotification.ContentText);
                    textStyle.SetSummaryText(serviceNotification.SummaryText);
                    builder.SetStyle(textStyle);
                    break;
                default:
                    builder.SetContentIntent(LocationForegroundService.BuildIntentToShowMainActivity(Constants.ACTION_GOTO_HOME));
                    break;
            }

            builder.SetVisibility(NotificationVisibility.Public);
            builder.SetPriority((int)NotificationPriority.High);

            // Build the notification:
            var notification = builder.Build();
            //notification.Defaults |= NotificationDefaults.Sound;
            notification.Defaults |= NotificationDefaults.Vibrate;

            // Launch notification:
            LocationForegroundService.NotificationManager.Notify((int)serviceNotification.NotificationType, notification);

            // Uncomment this code to update the notification 5 seconds later:
            // Thread.Sleep(5000);
            // builder.SetContentTitle("Updated Notification");
            // builder.SetContentText("Changed to this message after five seconds.");
            // notification = builder.Build();
            // notificationManager.Notify(notificationId, notification);
            Logger.Debug($"SendNotification.END");
        }

        private void BuildNotificationStyle(Notification.Builder builder, ServiceNotification serviceNotification)
        {
            var inboxStyle = new Notification.InboxStyle();
            for (var i = 0; i < serviceNotification.InboxLines.Count; i++)
            {
                var inboxLine = serviceNotification.InboxLines[i];
                var sb = new SpannableString($"{inboxLine[0]}: {inboxLine[1]}");
                sb.SetSpan(new StyleSpan(TypefaceStyle.Bold), 0, inboxLine[0].Length, SpanTypes.ExclusiveExclusive);
                inboxStyle.AddLine(sb);
                if (i == serviceNotification.InboxLines.Count - 1)
                {
                    builder.SetContentText(sb); //the last line is also the content text
                }
            }

            if (serviceNotification.InboxLines.Count > 1)
            {
                inboxStyle.SetSummaryText(string.Format(res.MoreMessages, serviceNotification.InboxLines.Count - 1));
            }
            builder.SetStyle(inboxStyle);
        }
    }
}