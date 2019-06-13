using Android.Content;
using goFriend.Controls;
using goFriend.Droid;
using goFriend.Models;
using goFriend.Services;
using System;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(FacebookLoginButton), typeof(FacebookLoginButtonRenderer))]
[assembly: Dependency(typeof(FacebookManager))]
namespace goFriend.Droid
{
    public class FacebookLoginButtonRenderer : ViewRenderer
    {
        Context ctx;
        bool disposed;
        public FacebookLoginButtonRenderer(Context ctx) : base(ctx)
        {
            this.ctx = ctx;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            if (Control == null)
            {
                var fbLoginBtnView = e.NewElement as FacebookLoginButton;
                var fbLoginbBtnCtrl = new Xamarin.Facebook.Login.Widget.LoginButton(ctx)
                {
                    LoginBehavior = LoginBehavior.NativeWithFallback
                };

                fbLoginbBtnCtrl.SetReadPermissions(fbLoginBtnView.Permissions);
                fbLoginbBtnCtrl.RegisterCallback(MainActivity.CallbackManager, new MyFacebookCallback(this.Element as FacebookLoginButton));

                SetNativeControl(fbLoginbBtnCtrl);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && !this.disposed)
                {
                    if (this.Control != null)
                    {
                        (this.Control as Xamarin.Facebook.Login.Widget.LoginButton).UnregisterCallback(MainActivity.CallbackManager);
                        this.Control.Dispose();
                    }
                    this.disposed = true;
                }
                base.Dispose(disposing);
            }
            catch { }
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
        public event EventHandler<OnProfileChangedEventArgs> mOnProfileChanged;
        protected override void OnCurrentProfileChanged(Profile oldProfile, Profile newProfile)
        {
            if (mOnProfileChanged != null)
            {
                mOnProfileChanged.Invoke(this, new OnProfileChangedEventArgs(newProfile));
            }
        }
    }
    public class OnProfileChangedEventArgs : EventArgs
    {
        public Profile mProfile;
        public OnProfileChangedEventArgs(Profile profile)
        {
            mProfile = profile;
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

        public User GetCurrentUser()
        {
            return Profile.CurrentProfile?.CreateUserFromProfile();
        }
    }

    public static class GoFriendExtension
    {
        public static User CreateUserFromProfile(this Profile profile)
        {
            return new User
            {
                Name = profile.Name,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                FacebookId = profile.Id
            };
        }
    }
}