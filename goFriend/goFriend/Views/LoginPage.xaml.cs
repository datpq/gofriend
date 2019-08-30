using System;
using System.Net;
using Acr.UserDialogs;
using goFriend.DataModel;
using goFriend.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        private readonly AccountPage _accountPage;
        private static LoginPage _instance;

        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();
        //private readonly EventWaitHandle _waitHandle;

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
            _accountPage = accountPage;
            InitializeComponent();
            //_waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            //Disappearing += (s, e) =>
            //{
            //    _waitHandle.Set();
            //};

            //Facebook event handlers
            BtnFacebook.OnSuccess = new Command<string>(authToken =>
            {
                Logger.Debug("Facebook logged-in with success");
                App.IsUserLoggedIn = true;
                Settings.IsUserLoggedIn = App.IsUserLoggedIn;
                Navigation.PopModalAsync();
                UserDialogs.Instance.ShowLoading(res.Processing);
            });
            BtnFacebook.OnError = new Command<string>(err => App.DisplayMsgError($"Authentication failed: { err }"));
            BtnFacebook.OnCancel = new Command(() =>
            {
                Logger.Debug("Authentication cancelled by the user");
            });

            //Event message handlers
            Logger.Debug("Subscribe");
            MessagingCenter.Subscribe<App, Friend>(this, Constants.MsgProfile, (sender, user) =>
            {
                if (user == null)
                {
                    Logger.Error("Received NULL");
                    return;
                }
                Logger.Debug($"Received: {Constants.MsgProfile} {user.ToString()}");
                App.User = user;
                App.User.DeviceInfo = $"Name={DeviceInfo.Name}|Type={DeviceInfo.DeviceType}|Model={DeviceInfo.Model}|Manufacturer={DeviceInfo.Manufacturer}|Platform={DeviceInfo.Platform}|Version={DeviceInfo.Version}";
                var avatarUrl = $"https://graph.facebook.com/{user.FacebookId}/picture?type=normal";
                using (var webClient = new WebClient())
                {
                    App.User.Image = webClient.DownloadData(avatarUrl);
                }
                Settings.LastUser = App.User;
                _accountPage.RefreshMenu();
            });
            MessagingCenter.Subscribe<App, Friend>(this, Constants.MsgProfileExt, async (sender, user) =>
            {
                Logger.Debug($"Received: {Constants.MsgProfileExt} {user?.ToString()}");
                App.User.Email = user?.Email;
                App.User.Birthday = user?.Birthday;
                UserDialogs.Instance.HideLoading();
                App.User = await App.FriendStore.LoginWithFacebook(App.User);
                Settings.LastUser = App.User;
                _accountPage.RefreshMenu();
            });
        }

        //public async Task Wait()
        //{
        //    await Task.Run(() => _waitHandle.WaitOne());
        //}

        async void CmdSignUp_Click(object sender, EventArgs e)
        {
            //await Navigation.PushAsync(new SignUpPage());
        }

        private void CmdLogin_Click(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PushAsync(new LoginManual());
        }

        //async void cmdLogin_Click(object sender, EventArgs e)
        //{
        //    return;
        //    //var user = new User
        //    //{
        //    //    Username = usernameEntry.Text,
        //    //    Password = passwordEntry.Text
        //    //};

        //    //var isValid = AreCredentialsCorrect(user);
        //    var isValid = true;
        //    if (isValid)
        //    {
        //        App.IsUserLoggedIn = true;
        //        Navigation.InsertPageBefore(new AppShell(), this);
        //        await Navigation.PopAsync();
        //    }
        //    else
        //    {
        //        //messageLabel.Text = "Login failed";
        //        //passwordEntry.Text = string.Empty;
        //    }
        //}
    }
}