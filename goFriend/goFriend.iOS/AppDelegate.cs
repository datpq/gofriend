using Facebook.CoreKit;
using Foundation;
using goFriend.Models;
using goFriend.Services;
using ImageCircle.Forms.Plugin.iOS;
using Xamarin.Forms;
using UIKit;

namespace goFriend.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        private ILogger _logger;

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            //facebook track of profile changing
            Profile.EnableUpdatesOnAccessTokenChange(true);
            Profile.Notifications.ObserveDidChange((sender, notification) => {
                if (notification.NewProfile != null)
                {
                        _logger.Debug("Send profile");
                        MessagingCenter.Send(Xamarin.Forms.Application.Current as App, Constants.MsgProfile,
                            new User
                            {
                                Name = notification.NewProfile.Name,
                                FirstName = notification.NewProfile.FirstName,
                                LastName = notification.NewProfile.LastName,
                                MiddleName = notification.NewProfile.MiddleName,
                                FacebookId = notification.NewProfile.UserId
                            });
                }
                else
                {
                    _logger.Debug("Profile null");
                    MessagingCenter.Send(Xamarin.Forms.Application.Current as App, Constants.MsgProfile, (User)null);
                }
            });

            Rg.Plugins.Popup.Popup.Init();
            Forms.SetFlags("Shell_Experimental", "Visual_Experimental", "CollectionView_Experimental", "FastRenderers_Experimental");
            Forms.Init();
            ImageCircleRenderer.Init();
            _logger = DependencyService.Get<ILogManager>().GetLog();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            // We need to handle URLs by passing them to their own OpenUrl in order to make the SSO authentication works.
            return ApplicationDelegate.SharedInstance.OpenUrl(application, url, sourceApplication, annotation);
        }
    }
}
