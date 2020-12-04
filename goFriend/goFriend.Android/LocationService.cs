using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Text;
using Android.Text.Style;
using goFriend.Models;
using goFriend.Services;
using Microsoft.Extensions.Caching.Memory;
using System;
using Color = Android.Graphics.Color;

//[assembly: Dependency(typeof(LocationService))
namespace goFriend.Droid
{
    [Service]
    public class LocationService : Service, INotificationService
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());
        private bool _isStarted = false;
        //private Timer _timer;
        private Handler _handler;
        private Action _runnable;
        private NotificationManager _notificationManager;
        private IMemoryCache _memoryCache;
        private int ServiceSleepTimeout = 10000;

        const string CacheBitmapPrefix = "CacheBitmap.";

        public override void OnCreate()
        {
            base.OnCreate();
            Logger.Info($"OnCreate.BEGIN");
            _handler = new Handler();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            // Get the notifications manager:
            _notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;

            // This Action is only for demonstration purposes.
            _runnable = new Action(async () =>
            {
                if (!_isStarted) return;
                var serviceNotification = await App.RunService();
                if (serviceNotification != null)
                {
                    Intent i = new Intent(Constants.NOTIFICATION_BROADCAST_ACTION);
                    i.PutExtra(Constants.SERVICE_BROADCAST_MESSAGE_KEY, "getting location done");
                    Android.Support.V4.Content.LocalBroadcastManager.GetInstance(this).SendBroadcast(i);
                    SendNotification(serviceNotification);
                }
                Logger.Debug($"Waiting for {ServiceSleepTimeout / 1000} seconds...");
                _handler.PostDelayed(_runnable, ServiceSleepTimeout);
            });

            Logger.Info($"OnCreate.END");
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Logger.Debug($"OnStartCommand.BEGIN(flags={flags}, startid={startId})");
            App.NotificationService = this;

            if (intent.Action.Equals(Constants.ACTION_START_SERVICE))
            {
                if (_isStarted)
                {
                    Logger.Debug($"Service was already started.");
                }
                else
                {
                    //_timer = new Timer(HandleTimerCallback, null, 0, Constants.SERVICE_TIMER_WAIT);
                    StartService();
                    _handler.PostDelayed(_runnable, ServiceSleepTimeout);
                }
            }
            else if (intent.Action.Equals(Constants.ACTION_STOP_SERVICE))
            {
                StopService();
            }
            else if (intent.Action.Equals(Constants.ACTION_STARTSTOP_TRACING))
            {
                PauseService();
            }

            Logger.Debug($"OnStartCommand.END");
            // This tells Android not to restart the service if it is killed to reclaim resources.
            return StartCommandResult.Sticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            // Return null because this is a pure started service. A hybrid service would return a binder that would
            // allow access to the GetFormattedStamp() method.
            return null;
        }

        public override void OnDestroy()
        {
            Logger.Info("OnDestroy: The started service is shutting down.");
            //_timer.Dispose();
            //_timer = null;

            // Stop the handler.
            _handler.RemoveCallbacks(_runnable);

            // Remove the notification from the status bar.
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Cancel(Constants.SERVICE_RUNNING_NOTIFICATION_ID);

            _isStarted = false;
            Logger.Debug("OnDestroy. Service is destroyed");
            base.OnDestroy();
        }

        //void HandleTimerCallback(object state)
        //{
        //    Logger.Debug("Getting location here...");
        //}

        /// <summary>
        /// Builds a PendingIntent that will display the main activity of the app. This is used when the 
        /// user taps on the notification; it will take them to the main activity of the app.
        /// </summary>
        /// <returns>The content intent.</returns>
        PendingIntent BuildIntentToShowMainActivity(string action)
        {
            var notificationIntent = new Intent(this, typeof(MainActivity));
            notificationIntent.SetAction(action);
            notificationIntent.SetFlags(ActivityFlags.SingleTop);
            notificationIntent.PutExtra(Constants.SERVICE_STARTED_KEY, true);

            var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.UpdateCurrent);
            return pendingIntent;
        }

        /// <summary>
        /// Builds a Notification.Action that will instruct the service to stop tracing.
        /// </summary>
        /// <returns>The stop tracing action.</returns>
        Notification.Action BuildStartStopTracingAction()
        {
            var startStopTracingIntent = new Intent(this, GetType());
            startStopTracingIntent.SetAction(Constants.ACTION_STARTSTOP_TRACING);
            var startStopTracingPendingIntent = PendingIntent.GetService(this, 0, startStopTracingIntent, 0);

            var builder = new Notification.Action.Builder(
                Settings.IsTracing ? Resource.Drawable.folder_open : Resource.Drawable.folder_close,
                Settings.IsTracing ? res.Pause : res.Play, startStopTracingPendingIntent);

            return builder.Build();
        }

        /// <summary>
        /// Builds the Notification.Action that will allow the user to stop the service via the
        /// notification in the status bar
        /// </summary>
        /// <returns>The stop service action.</returns>
        Notification.Action BuildStopServiceAction()
        {
            var stopServiceIntent = new Intent(this, GetType());
            stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);
            var stopServicePendingIntent = PendingIntent.GetService(this, 0, stopServiceIntent, 0);

            var builder = new Notification.Action.Builder(
                Android.Resource.Drawable.IcMediaPause, res.Exit, stopServicePendingIntent);

            return builder.Build();
        }

        public void StartService()
        {
            Logger.Debug($"StartService.BEGIN");

            //Register a foreground service
            var builder = new Notification.Builder(this)
                .SetColor(new Color(ContextCompat.GetColor(this, Resource.Color.colorPrimary)))
                .SetContentTitle(res.SvcBackground)
                //.SetWhen(Java.Lang.JavaSystem.CurrentTimeMillis() / 1000)
                //.SetShowWhen(true)
                .SetContentText(res.SvcBackgroundContentText)
                .SetSmallIcon(Resource.Drawable.hn9194_25)
                .SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.hn9194_25))
                .SetContentIntent(BuildIntentToShowMainActivity(Constants.ACTION_GOTO_HOME))
                .SetOngoing(true)
                .AddAction(BuildStartStopTracingAction())
                .AddAction(BuildStopServiceAction());

            // Using the Big Text style:
            var textStyle = new Notification.BigTextStyle();

            // Use the text in the edit box at the top of the screen.
            textStyle.BigText(res.SvcBackgroundContentText);
            //textStyle.SetSummaryText("The summary text goes here.");

            // Plug this style into the builder:
            builder.SetStyle(textStyle);

            var notification = builder.Build();

            // Enlist this instance of the service as a foreground service
            StartForeground(Constants.SERVICE_RUNNING_NOTIFICATION_ID, notification);

            _isStarted = true;

            Logger.Debug($"StartService.END");
        }

        public void PauseService()
        {
            Logger.Debug($"PauseService.BEGIN");

            Settings.IsTracing = !Settings.IsTracing;
            Logger.Debug("Tracing is " + (Settings.IsTracing ? "on" : "off"));
            StartService();

            Logger.Debug($"PauseService.END");
        }

        public void StopService()
        {
            Logger.Debug($"StopService.BEGIN");

            StopForeground(true);
            StopSelf();
            _isStarted = false;

            Logger.Debug($"StopService.END");
        }

        public void CancelNotification(NotificationType? notificationType = null)
        {
            if (notificationType.HasValue)
            {
                _notificationManager.Cancel((int)notificationType.Value);
            }
            else
            {
                _notificationManager.CancelAll();
            }
        }

        public void SendNotification(ServiceNotification serviceNotification)
        {
            Logger.Debug($"SendNotification.BEGIN(NotificationType={serviceNotification?.NotificationType})");
            if (serviceNotification != null)
            {
                var builder = new Notification.Builder(this)
                        .SetColor(new Color(ContextCompat.GetColor(this, Resource.Color.colorPrimary)))
                        .SetContentTitle(serviceNotification.ContentTitle)
                        .SetContentText(serviceNotification.ContentText)
                        .SetSmallIcon(Resource.Drawable.hn9194_25)
                        .SetShowWhen(true)
                        .SetAutoCancel(true);

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

                switch (serviceNotification.NotificationType)
                {
                    case NotificationType.AppearOnMap:
                        builder.SetContentIntent(BuildIntentToShowMainActivity(Constants.ACTION_GOTO_MAP));
                        BuildNotificationStyle(builder, serviceNotification);
                        break;
                    case NotificationType.ChatReceiveMessage:
                        builder.SetContentIntent(BuildIntentToShowMainActivity(Constants.ACTION_GOTO_CHAT));
                        BuildNotificationStyle(builder, serviceNotification);
                        break;
                    case NotificationType.ChatReceiveCreateChat:
                        builder.SetContentIntent(BuildIntentToShowMainActivity(Constants.ACTION_GOTO_CHAT));
                        var textStyle = new Notification.BigTextStyle();

                        textStyle.BigText(serviceNotification.ContentText);
                        textStyle.SetSummaryText(serviceNotification.SummaryText);
                        builder.SetStyle(textStyle);
                        break;
                    default:
                        builder.SetContentIntent(BuildIntentToShowMainActivity(Constants.ACTION_GOTO_HOME));
                        break;
                }

                builder.SetVisibility(NotificationVisibility.Public);
                builder.SetPriority((int)NotificationPriority.High);

                // Build the notification:
                var notification = builder.Build();
                //notification.Defaults |= NotificationDefaults.Sound;
                notification.Defaults |= NotificationDefaults.Vibrate;

                // Launch notification:
                _notificationManager.Notify((int)serviceNotification.NotificationType, notification);

                // Uncomment this code to update the notification 5 seconds later:
                // Thread.Sleep(5000);
                // builder.SetContentTitle("Updated Notification");
                // builder.SetContentText("Changed to this message after five seconds.");
                // notification = builder.Build();
                // notificationManager.Notify(notificationId, notification);
            }
            Logger.Debug($"SendNotification.END");
        }

        private void BuildNotificationStyle(Notification.Builder builder, ServiceNotification serviceNotification)
        {
            var inboxStyle = new Notification.InboxStyle();
            var startIdx = serviceNotification.InboxLines.Count > Constants.NOTIFICATION_MAX_MESSAGES ?
                serviceNotification.InboxLines.Count - Constants.NOTIFICATION_MAX_MESSAGES : 0;
            for (var i = startIdx; i < serviceNotification.InboxLines.Count; i++)
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

            if (serviceNotification.InboxLines.Count > Constants.NOTIFICATION_MAX_MESSAGES + 1)
            {
                inboxStyle.SetSummaryText(string.Format(res.MoreMessages, $"+{Constants.NOTIFICATION_MAX_MESSAGES}"));
            }
            else if (serviceNotification.InboxLines.Count > 1)
            {
                inboxStyle.SetSummaryText(string.Format(res.MoreMessages, serviceNotification.InboxLines.Count - 1));
            }
            builder.SetStyle(inboxStyle);
        }
    }
}