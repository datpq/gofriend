using Facebook.CoreKit;
using Facebook.LoginKit;
using goFriend.Controls;
using goFriend.iOS;
using goFriend.Services;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(FacebookLoginButton), typeof(FacebookLoginButtonRenderer))]
[assembly: Dependency(typeof(FacebookManager))]
namespace goFriend.iOS
{
    public class FacebookManager : IFacebookManager
    {
        public void Logout()
        {
            //LoginManager.Instance.LogOut(); //Android
            new LoginManager().LogOut();
        }

        public bool IsLoggedIn()
        {
            var accessToken = AccessToken.CurrentAccessToken;
            return accessToken != null;
        }
    }

    public class FacebookLoginButtonRenderer : ViewRenderer
    {
        private bool _disposed;
        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);
            if (Control == null)
            {
                var fbLoginBtnView = e.NewElement as FacebookLoginButton;
                var fbLoginBtnCtrl = new LoginButton
                {
                    LoginBehavior = LoginBehavior.Browser,
                    Permissions = fbLoginBtnView?.Permissions
                };

                fbLoginBtnCtrl.Completed += AuthCompleted;
                SetNativeControl(fbLoginBtnCtrl);
            }
        }

        private void AuthCompleted(object sender, LoginButtonCompletedEventArgs args)
        {
            var view = (Element as FacebookLoginButton);
            if (args.Error != null)
            {
                // Handle if there was an error
                view?.OnError?.Execute(args.Error.ToString());

            }
            else if (args.Result.IsCancelled)
            {
                // Handle if the user cancelled the login request
                view?.OnCancel?.Execute(null);
            }
            else
            {
                // Handle your successful login
                view?.OnSuccess?.Execute(args.Result.Token.TokenString);
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
                        ((LoginButton) Control).Completed -= AuthCompleted;
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
    }
}