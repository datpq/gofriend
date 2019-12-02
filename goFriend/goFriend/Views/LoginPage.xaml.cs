using goFriend.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        private static LoginPage _instance;

        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public static LoginPage GetInstance(AccountPage accountPage)
        {
            if (_instance == null)
            {
                _instance = new LoginPage(accountPage);
            }
            return _instance;
        }

        private LoginPage(AccountPage accountPage)
        {
            Title = res.Login;
            InitializeComponent();

            //Facebook event handlers
            BtnFacebook.OnSuccess = new Command<string>(async authToken =>
            {
                Logger.Debug("Facebook logged-in with success");
                App.IsUserLoggedIn = true;
                Settings.IsUserLoggedIn = App.IsUserLoggedIn;
                await Navigation.PopAsync();
                var deviceInfo = $"Name={DeviceInfo.Name}|Type={DeviceInfo.DeviceType}|Model={DeviceInfo.Model}|Manufacturer={DeviceInfo.Manufacturer}|Platform={DeviceInfo.Platform}|Version={DeviceInfo.Version}";
                App.User = await App.FriendStore.LoginWithFacebook(authToken, deviceInfo);
                Settings.LastUser = App.User;

                App.Initialize(); //redo the initialization background task

                accountPage.RefreshMenu();
            });
            BtnFacebook.OnError = new Command<string>(err => App.DisplayMsgError($"Authentication failed: { err }"));
            BtnFacebook.OnCancel = new Command(() =>
            {
                Logger.Debug("Authentication cancelled by the user");
            });

        }

        //private void CmdLogin_Click(object sender, EventArgs e)
        //{
        //    PopupNavigation.Instance.PushAsync(new LoginManual());
        //}
    }
}