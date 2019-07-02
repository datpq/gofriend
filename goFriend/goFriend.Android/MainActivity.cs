using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Xamarin.Facebook;
using Xamarin.Forms;
using Android.Content;
using goFriend.Services;
using goFriend.Models;
using ImageCircle.Forms.Plugin.Droid;

namespace goFriend.Droid
{
    [Activity(Label = "@string/app_label", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static ICallbackManager CallbackManager;
        private ILogger _logger;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
            {
                _logger.Error(args.Exception.ToString());
            };

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            //facebook track of profile changing
            var facebookProfileTracker = new FacebookProfileTracker();
            facebookProfileTracker.MOnProfileChanged += (sender, e) =>
            {
                if (e.MProfile != null)
                {
                    try
                    {
                        _logger.Debug("Send profile");
                        MessagingCenter.Send(Xamarin.Forms.Application.Current as App, Constants.MsgProfile,
                            new User
                            {
                                Name = e.MProfile.Name,
                                FirstName = e.MProfile.FirstName,
                                LastName = e.MProfile.LastName,
                                MiddleName = e.MProfile.MiddleName,
                                FacebookId = e.MProfile.Id
                            });
                    }
                    catch (Java.Lang.Exception) { }
                }
                else
                {
                    _logger.Debug("Profile null");
                    MessagingCenter.Send(Xamarin.Forms.Application.Current as App, Constants.MsgProfile, (User)null);
                }
            };
            facebookProfileTracker.StartTracking();

            // Create callback manager using CallbackManagerFactory
            CallbackManager = CallbackManagerFactory.Create();

            base.OnCreate(savedInstanceState);

            //FacebookSdk.SdkInitialize(ApplicationContext);

            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);
            Forms.SetFlags("Shell_Experimental", "Visual_Experimental", "CollectionView_Experimental", "FastRenderers_Experimental");
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);
            ImageCircleRenderer.Init();
            _logger = DependencyService.Get<ILogManager>().GetLog();
            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnActivityResult(
            int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            CallbackManager.OnActivityResult(requestCode, Convert.ToInt32(resultCode), data);
        }

        public override void OnBackPressed()
        {
            if (Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed))
            {
                // Do something if there are some pages in the `PopupStack`
            }
            else
            {
                // Do something if there are not any pages in the `PopupStack`
            }
        }
    }
}