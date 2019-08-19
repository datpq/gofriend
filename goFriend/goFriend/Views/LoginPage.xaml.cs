using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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
        private static LoginPage _instance;

        private readonly ILogger _logger = DependencyService.Get<ILogManager>().GetLog();
        private readonly EventWaitHandle _waitHandle;

        public static LoginPage GetInstance()
        {
            if (_instance == null)
            {
                _instance = new LoginPage();
            }
            return _instance;
        }

        private LoginPage()
        {
            InitializeComponent();
            _waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            Disappearing += (s, e) =>
            {
                _waitHandle.Set();
            };

            //Facebook event handlers
            BtnFacebook.OnSuccess = new Command<string>(authToken =>
            {
                _logger.Debug("Facebook logged-in with success");
                App.IsUserLoggedIn = true;
                Settings.IsUserLoggedIn = App.IsUserLoggedIn;
            });
            BtnFacebook.OnError = new Command<string>(err => App.DisplayMsgError($"Authentication failed: { err }"));
            BtnFacebook.OnCancel = new Command(() =>
            {
                _logger.Debug("Authentication cancelled by the user");
            });

            //Event message handlers
            _logger.Debug("Subscribe");
            MessagingCenter.Subscribe<App, Friend>(this, Constants.MsgProfile, (sender, user) =>
            {
                if (user == null)
                {
                    _logger.Error("Received NULL");
                    return;
                }
                _logger.Debug($"Received: {Constants.MsgProfile} {user.ToString()}");
                App.User = user;
                App.User.DeviceInfo = $"Name={DeviceInfo.Name}|Type={DeviceInfo.DeviceType}|Model={DeviceInfo.Model}|Manufacturer={DeviceInfo.Manufacturer}|Platform={DeviceInfo.Platform}|Version={DeviceInfo.Version}";
                var avatarUrl = $"https://graph.facebook.com/{user.FacebookId}/picture?type=normal";
                using (var webClient = new WebClient())
                {
                    App.User.Image = webClient.DownloadData(avatarUrl);
                }
                Settings.LastUser = App.User;
                Navigation.PopModalAsync();
                //(Application.Current as App).MainPage.DisplayAlert("Success", $"Authentication succeed: {user.Name}", "OK");
            });
            MessagingCenter.Subscribe<App, Friend>(this, Constants.MsgProfileExt, (sender, user) =>
            {
                _logger.Debug($"Received: {Constants.MsgProfileExt} {user?.ToString()}");
                App.User.Email = user?.Email;
                App.User.Birthday = user?.Birthday;
                Settings.LastUser = App.User;
                //App.FriendStore.AddOrUpdateFriendAsync(App.User);
            });
        }

        public async Task Wait()
        {
            await Task.Run(() => _waitHandle.WaitOne());
        }

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