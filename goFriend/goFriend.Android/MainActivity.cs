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
using goFriend.Views;

namespace goFriend.Droid
{
    [Activity(Label = "@string/app_label", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, LaunchMode = LaunchMode.SingleTop)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static ICallbackManager CallbackManager;
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());
        private Intent _startServiceIntent;
        private Intent _stopServiceIntent;
        private bool _isServiceStarted = false;

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

            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);
            Forms.SetFlags("Shell_Experimental", "Visual_Experimental", "CollectionView_Experimental", "FastRenderers_Experimental");
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);
            FormsMaterial.Init(this, savedInstanceState);
            UserDialogs.Init(this);

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
                _isServiceStarted = savedInstanceState.GetBoolean(Constants.SERVICE_STARTED_KEY, false);
            }

            _startServiceIntent = new Intent(this, typeof(LocationService));
            _startServiceIntent.SetAction(Constants.ACTION_START_SERVICE);

            _stopServiceIntent = new Intent(this, typeof(LocationService));
            _stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);

            if (!_isServiceStarted) {
                Logger.Debug("Service's not started. Starting now...");
                StartService(_startServiceIntent);
                _isServiceStarted = true;
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            if (intent == null)
            {
                return;
            }

            var bundle = intent.Extras;
            if (bundle != null)
            {
                if (bundle.ContainsKey(Constants.SERVICE_STARTED_KEY))
                {
                    _isServiceStarted = true;
                }
            }

            switch(intent.Action)
            {
                case Constants.ACTION_GOTO_MAP:
                    App.Current.MainPage.Navigation.PushAsync(new MapPage());
                    break;
                case Constants.ACTION_GOTO_CHAT:
                    App.Current.MainPage.Navigation.PushAsync(new ChatListPage());
                    break;
                case Constants.ACTION_GOTO_HOME:
                default:
                    break;
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutBoolean(Constants.SERVICE_STARTED_KEY, _isServiceStarted);
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
            if (requestCode == RequestLocationId)
            {
                if ((grantResults.Length == 1) && (grantResults[0] == (int) Permission.Granted))
                {
                    // Permissions granted - display a message.
                }
                else
                {
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