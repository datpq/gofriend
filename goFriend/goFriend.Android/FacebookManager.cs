using Android.Content;
using goFriend.Controls;
using goFriend.Droid;
using goFriend.Services;
using System;
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

        class MyFacebookCallback : Java.Lang.Object, IFacebookCallback
        {
            FacebookLoginButton view;

            public MyFacebookCallback(FacebookLoginButton view)
            {
                this.view = view;
            }

            public void OnCancel() =>
                view.OnCancel?.Execute(null);

            public void OnError(FacebookException fbException) =>
                view.OnError?.Execute(fbException.Message);

            public void OnSuccess(Java.Lang.Object result) =>
                view.OnSuccess?.Execute(((LoginResult)result).AccessToken.Token);

        }
    }

    public class FacebookProfileTracker : ProfileTracker
    {
        public event EventHandler<OnProfileChangedEventArgs> MOnProfileChanged;
        protected override void OnCurrentProfileChanged(Profile oldProfile, Profile newProfile)
        {
            MOnProfileChanged?.Invoke(this, new OnProfileChangedEventArgs(newProfile));
        }
    }
    public class OnProfileChangedEventArgs : EventArgs
    {
        public Profile MProfile;
        public OnProfileChangedEventArgs(Profile profile)
        {
            MProfile = profile;
        }
        //Extract or delete HTML tags based on their name or whether or not they contain some attributes or content with the HTML editor pro online program.
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