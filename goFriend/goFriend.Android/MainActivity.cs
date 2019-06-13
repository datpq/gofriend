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
        private ILogger logger;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
            {
                logger.Error(args.Exception.ToString());
            };

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            var facebookProfileTracker = new FacebookProfileTracker();
            facebookProfileTracker.mOnProfileChanged += mProfileTracker_mOnProfileChanged;
            facebookProfileTracker.StartTracking();
            // Create callback manager using CallbackManagerFactory
            CallbackManager = CallbackManagerFactory.Create();

            base.OnCreate(savedInstanceState);

            //FacebookSdk.SdkInitialize(ApplicationContext);

            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.SetFlags("Shell_Experimental", "Visual_Experimental", "CollectionView_Experimental", "FastRenderers_Experimental");
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            ImageCircleRenderer.Init();
            logger = DependencyService.Get<ILogManager>().GetLog();
            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        void mProfileTracker_mOnProfileChanged(object sender, OnProfileChangedEventArgs e)
        {
            if (e.mProfile != null)
            {
                try
                {
                    logger.Debug("Send profile");
                    MessagingCenter.Send(Xamarin.Forms.Application.Current as App, Constants.MsgProfile,
                        e.mProfile.CreateUserFromProfile());
                }
                catch (Java.Lang.Exception ex) { }
            }
            else
            {
                logger.Debug("Profile null");
                MessagingCenter.Send(Xamarin.Forms.Application.Current as App, Constants.MsgProfile, (User)null);
                //mprofile.ProfileId = null;
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