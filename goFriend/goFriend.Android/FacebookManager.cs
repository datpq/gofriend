using System;
using Android.Content;
using goFriend.Controls;
using goFriend.Droid;
using goFriend.Services;
using Android.OS;
using goFriend.DataModel;
using Newtonsoft.Json.Linq;
using Org.Json;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Facebook.Login.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(FacebookLoginButton), typeof(FacebookLoginButtonRenderer))]
[assembly: Dependency(typeof(FacebookManager))]
namespace goFriend.Droid
{
    public class FacebookLoginButtonRenderer : ViewRenderer
    {
        private readonly Context _ctx;
        private bool _disposed;
        public FacebookLoginButtonRenderer(Context ctx) : base(ctx)
        {
            _ctx = ctx;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            if (Control == null)
            {
                var fbLoginBtnView = e.NewElement as FacebookLoginButton;
                var fbLoginbBtnCtrl = new LoginButton(_ctx)
                {
                    LoginBehavior = LoginBehavior.NativeWithFallback
                };

                fbLoginbBtnCtrl.SetReadPermissions(fbLoginBtnView?.Permissions);
                fbLoginbBtnCtrl.RegisterCallback(MainActivity.CallbackManager, new MyFacebookCallback(Element as FacebookLoginButton));

                SetNativeControl(fbLoginbBtnCtrl);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && !_disposed)
                {
                    if (Control != null)
                    {
                        ((LoginButton) Control).UnregisterCallback(MainActivity.CallbackManager);
                        Control.Dispose();
                    }
                    _disposed = true;
                }
                base.Dispose(disposing);
            }
            catch
            {
                // ignored
            }
        }

        class MyFacebookCallback : Java.Lang.Object, IFacebookCallback, GraphRequest.IGraphJSONObjectCallback
        {
            FacebookLoginButton view;
            private readonly ILogger _logger;

            public MyFacebookCallback(FacebookLoginButton view)
            {
                _logger = DependencyService.Get<ILogManager>().GetLog();
                this.view = view;
            }

            public void OnCancel() =>
                view.OnCancel?.Execute(null);

            public void OnError(FacebookException fbException) =>
                view.OnError?.Execute(fbException.Message);

            public void OnSuccess(Java.Lang.Object result)
            {
                _logger.Debug("OnSuccess.BEGIN");
                var accessToken = ((LoginResult) result).AccessToken;
                view.OnSuccess?.Execute(accessToken.Token);

                //var accessToken = AccessToken.CurrentAccessToken;
                var request = GraphRequest.NewMeRequest(accessToken, this);
                var parameters = new Bundle();
                parameters.PutString("fields", "id,name,email,gender,birthday");
                request.Parameters = parameters;
                request.ExecuteAsync();

                _logger.Debug("OnSuccess.END");
            }

            public void OnCompleted(JSONObject json, GraphResponse response)
            {
                _logger.Debug("OnCompleted.BEGIN");
                var data = json.ToString();
                dynamic result = JObject.Parse(data);
                string birthdayStr = result.birthday;
                var birthday = DateTime.ParseExact(birthdayStr, "MM/dd/yyyy", null);
                try
                {
                    _logger.Debug($"Send profile extension: {result.email} {birthdayStr} {result.gender}");
                    MessagingCenter.Send(Application.Current as App, Constants.MsgProfileExt,
                        new Friend
                        {
                            Email = result.email,
                            Birthday = birthday,
                            Gender = result.gender
                        });
                }
                catch (Java.Lang.Exception) { }
                _logger.Debug("OnCompleted.END");
            }
        }
    }

    public class FacebookProfileTracker : ProfileTracker
    {
        private readonly ILogger _logger = DependencyService.Get<ILogManager>().GetLog();

        protected override void OnCurrentProfileChanged(Profile oldProfile, Profile newProfile)
        {
            if (newProfile != null)
            {
                try
                {
                    _logger.Debug("Send profile");
                    MessagingCenter.Send(Application.Current as App, Constants.MsgProfile,
                        new Friend
                        {
                            Name = newProfile.Name,
                            FirstName = newProfile.FirstName,
                            LastName = newProfile.LastName,
                            MiddleName = newProfile.MiddleName,
                            FacebookId = newProfile.Id
                        });
                }
                catch (Java.Lang.Exception) { }
            }
            else
            {
                _logger.Debug("Profile null");
                MessagingCenter.Send(Application.Current as App, Constants.MsgProfile, (Friend)null);
            }
        }
    }

    public class FacebookManager : IFacebookManager
    {
        public void Logout()
        {
            LoginManager.Instance.LogOut();
        }

        public bool IsLoggedIn()
        {
            var accessToken = AccessToken.CurrentAccessToken;
            return accessToken != null;
        }

        //public User GetCurrentUser()
        //{
        //    return Profile.CurrentProfile?.CreateUserFromProfile();
        //}
    }
}