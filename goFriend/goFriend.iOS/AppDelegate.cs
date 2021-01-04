using Facebook.CoreKit;
using FFImageLoading.Forms.Platform;
using Foundation;
using goFriend.Services;
using Xamarin.Forms;
using UIKit;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using UserNotifications;
using ImageCircle.Forms.Plugin.iOS;

namespace goFriend.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        private ILogger Logger;

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
            /* If sending profile from Client
            Profile.Notifications.ObserveDidChange((sender, notification) => {
                if (notification.NewProfile != null)
                {
                    var friendProfile = new Friend
                    {
                        Name = notification.NewProfile.Name,
                        FirstName = notification.NewProfile.FirstName,
                        LastName = notification.NewProfile.LastName,
                        MiddleName = notification.NewProfile.MiddleName,
                        FacebookId = notification.NewProfile.UserId
                    };
                    _logger.Debug($"Send profile: {friendProfile}");
                    MessagingCenter.Send(Xamarin.Forms.Application.Current as App, Constants.MsgProfile, friendProfile);
                    var request = new GraphRequest("/" + notification.NewProfile.UserId, new NSDictionary("fields", "id,name,email,gender,birthday"), AccessToken.CurrentAccessToken.TokenString, null, "GET");
                    request.Start((connection, result, error) =>
                    {
                        try
                        {
                            var userInfo = result as NSDictionary;
                            _logger.Debug($"Send profile extension: {userInfo}");
                            var email = userInfo?["email"]?.ToString();
                            var birthdayStr = userInfo?["birthday"]?.ToString();
                            DateTime? birthday = null;
                            try
                            {
                                birthday = DateTime.ParseExact(birthdayStr, "MM/dd/yyyy", null);
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                            var gender = userInfo?["gender"]?.ToString();
                            var friendProfileExt = new Friend
                            {
                                Email = email,
                                Birthday = birthday,
                                Gender = gender
                            };
                            _logger.Debug($"Send profile extension: Email={friendProfileExt.Email}|Birthday={friendProfileExt.Birthday}|Gender={friendProfileExt.Gender}");
                            MessagingCenter.Send(Xamarin.Forms.Application.Current as App, Constants.MsgProfileExt, friendProfileExt);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e.ToString());
                        }
                    });
                }
                else
                {
                    _logger.Debug("Profile null");
                    MessagingCenter.Send(Xamarin.Forms.Application.Current as App, Constants.MsgProfile, (Friend)null);
                }
            });
            */

            Rg.Plugins.Popup.Popup.Init();
            Forms.SetFlags("Shell_Experimental", "Visual_Experimental", "CollectionView_Experimental", "FastRenderers_Experimental");
            Forms.Init();
            FormsMaterial.Init();
            ImageCircleRenderer.Init();

            Xamarin.FormsGoogleMaps.Init("AIzaSyAxU0F02kSfdb8oVeFoFWKwK619RK5HkqU");
            CachedImageRenderer.Init();
            //This line causes error: "Cannot access a disposed object.\nObject name: 'PrimaryToolbarItem'.
            CachedImageRenderer.InitImageSourceHandler(); ;

            AppCenter.Start(Constants.AppCenterAppSecretiOS, typeof(Analytics), typeof(Crashes));

            // set a delegate to handle incoming notifications
            UNUserNotificationCenter.Current.Delegate = new iOSNotificationReceiver();

            InitializeNLog();
            Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());
            Logger.Debug("Loading application...");
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        private void InitializeNLog()
        {
            var assembly = GetType().Assembly;
            var assemblyName = assembly.GetName().Name;
            new LogService().Initialize(assembly, assemblyName);
        }

        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            // We need to handle URLs by passing them to their own OpenUrl in order to make the SSO authentication works.
            return ApplicationDelegate.SharedInstance.OpenUrl(application, url, sourceApplication, annotation);
        }

        public override void OnResignActivation(UIApplication application)
        {
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
            Logger.Debug("App moving to inactive state.");
        }

        public override void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the user quits.
            Logger.Debug("App entering background state.");
        }

        public override void WillEnterForeground(UIApplication application)
        {
            // Called as part of the transiton from background to active state.
            // Here you can undo many of the changes made on entering the background.
            Logger.Debug("App will enter foreground");
        }

        public override void OnActivated(UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the user interface.
            Logger.Debug("App is becoming active");
        }

        public override void WillTerminate(UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        }
    }
}
