using Facebook.CoreKit;
using FFImageLoading.Forms.Platform;
using Foundation;
using goFriend.Services;
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

            CachedImageRenderer.Init();
            CachedImageRenderer.InitImageSourceHandler();;

            _logger = DependencyService.Get<ILogManager>().GetLog();
            _logger.Debug("Loading application...");
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
