using goFriend.Services;
using System.Windows.Input;
using Xamarin.Forms;

namespace goFriend.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private static readonly ILogger logger = DependencyService.Get<ILogManager>().GetLog();

        public ICommand OnFacebookLoginSuccessCmd { get; }
        public ICommand OnFacebookLoginErrorCmd { get; }
        public ICommand OnFacebookLoginCancelCmd { get; }

        public LoginViewModel()
        {
            OnFacebookLoginSuccessCmd = new Command<string>(async authToken => {
                logger.Debug($"Success: {authToken}");
                App.IsUserLoggedIn = true;
                Settings.IsUserLoggedIn = App.IsUserLoggedIn;
                Application.Current.MainPage = new AppShell();
            });

            OnFacebookLoginErrorCmd = new Command<string>(
                (err) => DisplayAlert("Error", $"Authentication failed: { err }"));

            OnFacebookLoginCancelCmd = new Command(() =>
            {
                logger.Debug($"Authentication cancelled by the user");
            });
        }

        void DisplayAlert(string title, string msg) =>
            (Application.Current as App).MainPage.DisplayAlert(title, msg, "OK");
    }
}
