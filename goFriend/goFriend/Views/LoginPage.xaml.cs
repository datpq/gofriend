using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using goFriend.Services;
using Plugin.DeviceInfo;
using goFriend.AppleSignIn;
using goFriend.DataModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        public LoginPage()
        {
            Title = res.Login;
            InitializeComponent();
            BtnSignInApple.IsVisible = Device.RuntimePlatform == Device.iOS && Constants.AppleSignInButtonVisible;

            //Facebook event handlers
            BtnFacebook.OnSuccess = new Command<string>(async authToken =>
            {
                Logger.Debug("Facebook logged-in with success");
                App.IsUserLoggedIn = true;
                Settings.IsUserLoggedIn = App.IsUserLoggedIn;
                await Navigation.PopAsync();
                try
                {
                    UserDialogs.Instance.ShowLoading(res.Processing);
                    App.User = await App.FriendStore.LoginWithFacebook(authToken, Extension.GetDeviceInfo());
                }
                finally
                {
                    UserDialogs.Instance.HideLoading();
                }
                Settings.LastUser = App.User;
                App.Initialize(); //redo the initialization background task
                App.Current.MainPage = new AppShell();
            });
            BtnFacebook.OnError = new Command<string>(err => App.DisplayMsgError($"Authentication failed: { err }"));
            BtnFacebook.OnCancel = new Command(() =>
            {
                Logger.Debug("Authentication cancelled by the user");
            });
        }

        private async void AppleSignInButton_OnSignIn(object sender, EventArgs e)
        {
            var appleSignIn = DependencyService.Get<IAppleSignInService>();
            Task<AppleAccount> theTask = appleSignIn.SignInAsync();
            try
            {
                var account = await theTask;
                var friend = new Friend
                {
                    Name = account.Name,
                    FirstName = account.FirstName,
                    MiddleName = account.MiddleName,
                    LastName = account.LastName,
                    Email = account.Email,
                    ThirdPartyUserId = account.UserId,
                    ThirdPartyToken = account.AccessToken,
                    ThirdPartyLogin = ThirdPartyLogin.Apple,
                    Info = Extension.GetVersionTrackingInfo()
                };
                try
                {
                    UserDialogs.Instance.ShowLoading(res.Processing);
                    App.User = await App.FriendStore.LoginWithThirdParty(friend, Extension.GetDeviceInfo());
                }
                finally
                {
                    UserDialogs.Instance.HideLoading();
                }
                if (App.User != null)
                {
                    Logger.Debug("Apple logged-in with success");
                    App.IsUserLoggedIn = true;
                    Settings.IsUserLoggedIn = App.IsUserLoggedIn;
                    await Navigation.PopAsync();
                    Settings.LastUser = App.User;
                    App.Initialize(); //redo the initialization background task
                    App.Current.MainPage = new AppShell();
                }
                else
                {
                    if (account.Name == string.Empty)
                    {
                        App.DisplayMsgError(res.MsgAppleSignInNoName);
                    }
                    else
                    {
                        App.DisplayMsgError(res.MsgAppleSignInServer);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.ToString());
            }
            Logger.Debug($"Task IsCanceled: {theTask.IsCanceled}, Task IsFaulted: {theTask.IsFaulted}");
            if (theTask.Exception != null)
            {
                Logger.Warn($"Exception Message: {theTask.Exception.Message}, Inner Exception Message: {theTask.Exception.InnerException?.Message}");
            }
        }
    }
}