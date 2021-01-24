using System;
using Acr.UserDialogs;
using Android;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Xamarin.Facebook;
using Xamarin.Forms;
using Android.Content;
using Android.Views;
using FFImageLoading.Forms.Platform;
using goFriend.Services;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Color = Android.Graphics.Color;
using ImageCircle.Forms.Plugin.Droid;

namespace goFriend.Droid
{
    [Activity(Label = "@string/app_label", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, LaunchMode = LaunchMode.SingleTop)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static ICallbackManager CallbackManager;
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());
        public Intent StartServiceIntent { get; set; }
        private Intent StopServiceIntent { get; set; }
        public bool IsServiceStarted { get; set; } = false;

        static readonly int RC_LAST_LOCATION_PERMISSION_CHECK = 1000;
        static readonly int RC_LOCATION_UPDATES_PERMISSION_CHECK = 1100;
        public static Color COLOR_PRIMARY;

        const int RequestLocationId = 0;

        readonly string[] LocationPermissions =
        {
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation
        };

        protected override void OnStart()
        {
            base.OnStart();

            if ((int)Build.VERSION.SdkInt >= 23)
            {
                if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) != Permission.Granted)
                {
                    RequestPermissions(LocationPermissions, RequestLocationId);
                }
                else
                {
                    // Permissions already granted - display a message.
                }
            }
        }

        private void InitializeColors()
        {
            Android.Util.TypedValue a = new Android.Util.TypedValue();
            Theme.ResolveAttribute(Android.Resource.Attribute.ColorPrimary, a, true);
            var colorPrimarya = ApplicationContext.GetDrawable(a.ResourceId);
            COLOR_PRIMARY = ((Android.Graphics.Drawables.ColorDrawable)colorPrimarya).Color;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
            {
                Logger.Error(args.Exception.ToString());
            };

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            // Create callback manager using CallbackManagerFactory
            CallbackManager = CallbackManagerFactory.Create();

            base.OnCreate(savedInstanceState);

            //FacebookSdk.SdkInitialize(ApplicationContext);

            Rg.Plugins.Popup.Popup.Init(this);
            Forms.SetFlags("Shell_Experimental", "Visual_Experimental", "CollectionView_Experimental", "FastRenderers_Experimental");
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);
            FormsMaterial.Init(this, savedInstanceState);
            UserDialogs.Init(this);
            ImageCircleRenderer.Init();

            Xamarin.FormsGoogleMaps.Init(this, savedInstanceState);
            CachedImageRenderer.Init(true);
            CachedImageRenderer.InitImageViewHandler();

            AppCenter.Start(Constants.AppCenterAppSecretAndroid, typeof(Analytics), typeof(Crashes));

            InitializeNLog();

            //facebook track of profile changing
            FacebookProfileTracker.GetInstance();

            LoadApplication(new App());

            Logger.Debug("Preparing ForegroundService...");
            OnNewIntent(this.Intent);
            if (savedInstanceState != null)
            {
                IsServiceStarted = savedInstanceState.GetBoolean(Constants.SERVICE_STARTED_KEY, false);
            }

            StartServiceIntent = new Intent(this, typeof(LocationForegroundService));
            StartServiceIntent.SetAction(Constants.ACTION_START_SERVICE);

            StopServiceIntent = new Intent(this, typeof(LocationForegroundService));
            StopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);

            Droid.LocationService.MainActivity = this;

            InitializeColors();

            //if (!_isServiceStarted)
            //{
            //    Logger.Debug("Service's not started. Starting now...");
            //    StartService(_startServiceIntent);
            //    _isServiceStarted = true;
            //}
        }

        protected async override void OnNewIntent(Intent intent)
        {
            if (intent == null || intent.Action == null || !App.IsUserLoggedIn || App.User == null)
            {
                return;
            }

            var bundle = intent.Extras;
            if (bundle != null)
            {
                if (bundle.ContainsKey(Constants.SERVICE_STARTED_KEY))
                {
                    IsServiceStarted = true;
                }
            }

            switch(intent.Action)
            {
                case Constants.ACTION_GOTO_MAPONLINE:
                    await Shell.Current.GoToAsync($"//{Constants.ROUTE_MAPONLINE}");
                    break;
                case Constants.ACTION_GOTO_CHAT:
                    await Shell.Current.GoToAsync($"//{Constants.ROUTE_CHAT}");
                    var chatId = bundle.GetInt(Constants.SERVICE_EXTRAID_KEY);
                    await App.DoNotificationAction(intent.Action, chatId);
                    break;
                case Constants.ACTION_GOTO_HOME:
                default:
                    await Shell.Current.GoToAsync($"//{Constants.ROUTE_HOME}");
                    break;
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutBoolean(Constants.SERVICE_STARTED_KEY, IsServiceStarted);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnDestroy()
        {
            //Log.Info(TAG, "Activity is being destroyed; stop the service.");

            //StopService(startServiceIntent);
            base.OnDestroy();
        }

        private void InitializeNLog()
        {
            var assembly = this.GetType().Assembly;
            var assemblyName = assembly.GetName().Name;
            new LogService().Initialize(assembly, assemblyName);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == RC_LAST_LOCATION_PERMISSION_CHECK || requestCode == RC_LOCATION_UPDATES_PERMISSION_CHECK)
            {
                if ((grantResults.Length == 1) && (grantResults[0] == (int) Permission.Granted))
                {
                    if (requestCode == RC_LAST_LOCATION_PERMISSION_CHECK)
                    {
                        //await GetLastLocationFromDevice();
                    }
                    else
                    {
                        //RequestLocationUpdates
                    }
                }
                else
                {
                    App.DisplayMsgInfo(res.MsgNoGpsWarning);
                }
            }
            else
            {
                Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }
        }

        protected override void OnActivityResult(
            int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            CallbackManager.OnActivityResult(requestCode, Convert.ToInt32(resultCode), data);
        }

        public override void OnBackPressed()
        {
            if (Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.Count == 1)
            {
                //Just minimize app when there nothing more to pop up from stack
                Intent main = new Intent(Intent.ActionMain);
                main.AddCategory(Intent.CategoryHome);
                StartActivity(main);
                return;
            }
            if (Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed))
            {
                // Do something if there are some pages in the `PopupStack`
            }
            else
            {
                // Do something if there are not any pages in the `PopupStack`
            }
        }

        //Keep Entry focused when click button
        private bool _lieAboutCurrentFocus;
        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            _lieAboutCurrentFocus = true;
            var result = base.DispatchTouchEvent(ev);
            _lieAboutCurrentFocus = false;

            return result;
        }
        public override Android.Views.View CurrentFocus
        {
            get
            {
                if (_lieAboutCurrentFocus)
                {
                    return null;
                }

                return base.CurrentFocus;
            }
        }
    }
}