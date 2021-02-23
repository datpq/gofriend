using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Location;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using goFriend.Services;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using System;
using System.Linq;
using System.Threading.Tasks;
using Color = Android.Graphics.Color;

namespace goFriend.Droid
{
    [Service]
    public class LocationForegroundService : Service
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());
        private bool _isStarted = false;
        //private Timer _timer;
        private Handler _handler;
        private Action _runnable;
        public NotificationManager NotificationManager { get; set; }

        private static LocationForegroundService _serviceUniqueInstance = null;
        public static LocationForegroundService GetInstance() { return _serviceUniqueInstance; }

        //Fused Location Provider
        private bool _isGooglePlayServicesInstalled;
        private static FusedLocationProviderClient _fusedLocationProviderClient;
        private static LocationCallback _locationCallback;
        private static LocationRequest _locationRequest;

        public override void OnCreate()
        {
            base.OnCreate();
            Logger.Info($"OnCreate.BEGIN");
            _handler = new Handler();

            // Get the notifications manager:
            NotificationManager = GetSystemService(Context.NotificationService) as NotificationManager;

            _runnable = new Action(async () =>
            {
                if (!_isStarted) return;
                //var serviceNotification = await App.RunService();
                //if (serviceNotification != null)
                //{
                //    Intent i = new Intent(Constants.NOTIFICATION_BROADCAST_ACTION);
                //    i.PutExtra(Constants.SERVICE_BROADCAST_MESSAGE_KEY, "getting location done");
                //    Android.Support.V4.Content.LocalBroadcastManager.GetInstance(this).SendBroadcast(i);
                //    SendNotification(serviceNotification);
                //}
                Logger.Debug($"Waiting for {Constants.LOCATIONSERVICE_UPDATE_INTERVAL} seconds...");
                _handler.PostDelayed(_runnable, Constants.LOCATIONSERVICE_UPDATE_INTERVAL * 1000);
            });

            Logger.Info($"OnCreate.END");
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Logger.Debug($"OnStartCommand.BEGIN(flags={flags}, startid={startId})");

            if (intent.Action.Equals(Constants.ACTION_START_SERVICE))
            {
                if (_isStarted)
                {
                    Logger.Debug($"Service was already started.");
                }
                else
                {
                    //_timer = new Timer(HandleTimerCallback, null, 0, Constants.SERVICE_TIMER_WAIT);
                    //App.LocationService.Start();
                    //uncomment to activate the looping task of the foreground service
                    //_handler.PostDelayed(_runnable, ServiceSleepTimeout);
                }
            }
            else if (intent.Action.Equals(Constants.ACTION_STOP_SERVICE))
            {
                //run async task in sync method
                AsyncContext.Run(async () => await Task.Run(() => App.LocationService.Stop()));
                //Task.Run(() => App.LocationService.Stop()).GetAwaiter().GetResult();
                //Task.Run(async () => await Task.Run(() =>
                //{
                //    App.LocationService.Stop();
                //})).WaitAndUnwrapException();
                //App.LocationService.Stop();
            }
            else if (intent.Action.Equals(Constants.ACTION_STARTSTOP_TRACING))
            {
                if (Droid.LocationService.IsTracing) // Paused
                {
                    Views.MapOnlinePage.MapOnlineInfo.ToList().ForEach(x => {
                        x.Value.IsRunningSaved = x.Value.IsRunning;
                        x.Value.RadiusSaved = x.Value.Radius;
                        x.Value.IsRunning = false;
                    });
                }
                else //Play
                {
                    Views.MapOnlinePage.MapOnlineInfo.Where(x => x.Value.IsRunningSaved).ToList().ForEach(x => {
                        x.Value.IsRunning = x.Value.IsRunningSaved;
                        x.Value.Radius = x.Value.RadiusSaved;
                        x.Value.IsRunningSaved = false;
                    });
                }
                //Task.Run(() =>
                //{
                //    App.LocationService.Pause();
                //    App.LocationService.RefreshStatus();
                //}).GetAwaiter().GetResult();
                //run async task in sync method
                AsyncContext.Run(async () =>
                {
                    await Task.Run(() => App.LocationService.Pause());
                    await Task.Run(() => App.LocationService.RefreshStatus());
                });
                //Task.Run(async () => await Task.Run(() =>
                //{
                //    App.LocationService.Pause();
                //    App.LocationService.RefreshStatus();
                //})).WaitAndUnwrapException();
            }

            _serviceUniqueInstance = this;
            Droid.LocationService.StandByWhenStart.Set();

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
            //_handler.RemoveCallbacks(_runnable);

            // Remove the notification from the status bar.
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Cancel(Constants.SERVICE_RUNNING_NOTIFICATION_ID);

            _serviceUniqueInstance = null;
            _isStarted = false;
            Logger.Debug("OnDestroy. Service is destroyed");
            base.OnDestroy();
        }

        bool IsGooglePlayServicesInstalled()
        {
            var queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (queryResult == ConnectionResult.Success)
            {
                Logger.Debug("Google Play Services is installed on this device.");
                return true;
            }

            if (GoogleApiAvailability.Instance.IsUserResolvableError(queryResult))
            {
                var errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);
                Logger.Error($"There is a problem with Google Play Services on this device: {queryResult} - {errorString}");
            }

            return false;
        }

        //void HandleTimerCallback(object state)
        //{
        //    Logger.Debug("Getting location here...");
        //}

        public async Task StartForegroundService()
        {
            try
            {
                Logger.Debug($"StartForegroundService.BEGIN");
                if (!App.IsUserLoggedIn || App.User == null)
                {
                    Logger.Debug("User not logged in. Cannot start service!");
                    return;
                }

                //Register a foreground service
                var builder = new Notification.Builder(this)
                    .SetColor(new Color(ContextCompat.GetColor(this, Resource.Color.colorPrimary)))
                    .SetContentTitle(res.SvcBackground)
                    //.SetWhen(Java.Lang.JavaSystem.CurrentTimeMillis() / 1000)
                    //.SetShowWhen(true)
                    .SetSmallIcon(Resource.Drawable.hn9194_25)
                    .SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.hn9194_25))
                    .SetContentIntent(BuildIntentToShowMainActivity(Constants.ACTION_GOTO_MAPONLINE))
                    .SetOngoing(true)
                    .AddAction(BuildStartStopTracingAction())
                    .AddAction(BuildStopServiceAction());

                Droid.NotificationService.BuildNotificationStyle(builder,
                    Views.MapOnlinePage.GetMapOnlineStatus(), res.SvcBackgroundContentText);
                //// Using the Big Text style:
                //var textStyle = new Notification.BigTextStyle();

                //// Use the text in the edit box at the top of the screen.
                //textStyle.BigText(contentText);
                ////textStyle.SetSummaryText("The summary text goes here.");

                //// Plug this style into the builder:
                //builder.SetStyle(textStyle);

                var notification = builder.Build();

                // Enlist this instance of the service as a foreground service
                StartForeground(Constants.SERVICE_RUNNING_NOTIFICATION_ID, notification);

                _isStarted = true;

                //Fused Location Provider RequestLocationUpdates
                if (Droid.LocationService.IsTracing && _fusedLocationProviderClient == null)
                {
                    _isGooglePlayServicesInstalled = IsGooglePlayServicesInstalled();
                    if (_isGooglePlayServicesInstalled)
                    {
                        _locationRequest = new LocationRequest()
                                          .SetPriority(LocationRequest.PriorityHighAccuracy)
                                          .SetInterval(Constants.LOCATIONSERVICE_UPDATE_INTERVAL*1000)
                                          .SetFastestInterval(Constants.LOCATIONSERVICE_UPDATE_INTERVAL*1000);
                        _locationCallback = new FusedLocationProviderCallback();

                        _fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(this);
                        await _fusedLocationProviderClient.RequestLocationUpdatesAsync(_locationRequest, _locationCallback);
                    }
                    else
                    {
                        App.DisplayMsgInfo(res.MsgNoGooglePlayServiceWarning);
                    }
                }
                else if (!Droid.LocationService.IsTracing)
                {
                    await StopRequestLocationUpdates();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                Logger.Debug($"StartForegroundService.END");
            }
        }

        private async Task StopRequestLocationUpdates()
        {
            if (_fusedLocationProviderClient != null)
            {
                Logger.Debug($"StopRequestLocationUpdates.BEGIN");
                await _fusedLocationProviderClient.RemoveLocationUpdatesAsync(_locationCallback);
                _locationRequest = null;
                _locationCallback = null;
                _fusedLocationProviderClient = null;
                Logger.Debug($"StopRequestLocationUpdates.END");
            }
        }

        public async Task StopForegroundService()
        {
            Logger.Debug($"StopForegroundService.BEGIN");

            await StopRequestLocationUpdates();

            StopForeground(true);
            StopSelf();
            _isStarted = false;

            Logger.Debug($"StopForegroundService.END");
        }

        /// <summary>
        /// Builds a PendingIntent that will display the main activity of the app. This is used when the 
        /// user taps on the notification; it will take them to the main activity of the app.
        /// </summary>
        /// <returns>The content intent.</returns>
        public PendingIntent BuildIntentToShowMainActivity(string action, int extraId = 0)
        {
            var notificationIntent = new Intent(this, typeof(MainActivity));
            notificationIntent.SetAction(action);
            notificationIntent.SetFlags(ActivityFlags.SingleTop);
            notificationIntent.PutExtra(Constants.SERVICE_STARTED_KEY, true);
            notificationIntent.PutExtra(Constants.SERVICE_EXTRAID_KEY, extraId);

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
                Droid.LocationService.IsTracing ? Android.Resource.Drawable.IcMediaPause : Android.Resource.Drawable.IcMediaPlay,
                Droid.LocationService.IsTracing ? res.Pause : res.Play, startStopTracingPendingIntent);

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
                Android.Resource.Drawable.IcDelete, res.Exit, stopServicePendingIntent);

            return builder.Build();
        }
    }
}