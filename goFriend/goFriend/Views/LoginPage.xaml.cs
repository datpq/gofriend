using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Services;
using goFriend.Services;
using goFriend.Models;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        private readonly ILogger _logger = DependencyService.Get<ILogManager>().GetLog();

        public LoginPage()
        {
            InitializeComponent();
        }

        async void CmdSignUp_Click(object sender, EventArgs e)
        {
            //await Navigation.PushAsync(new SignUpPage());
        }

        private void CmdLogin_Click(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PushAsync(new LoginManual());
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _logger.Debug("OnAppearing.Unsubscribe");
            MessagingCenter.Unsubscribe<App, string>(this, Constants.MsgProfile);
            MessagingCenter.Unsubscribe<App, string>(this, Constants.MsgProfileExt);

            _logger.Debug("OnAppearing.Subscribe");
            MessagingCenter.Subscribe<App, User>(this, Constants.MsgProfile, (sender, user) =>
            {
                _logger.Debug($"Received: {Constants.MsgProfile} {user?.ToString()}");
                App.User = user;
                Settings.LastUser = App.User;
                //(Application.Current as App).MainPage.DisplayAlert("Success", $"Authentication succeed: {user.Name}", "OK");
            });
            MessagingCenter.Subscribe<App, User>(this, Constants.MsgProfileExt, (sender, user) =>
            {
                _logger.Debug($"Received: {Constants.MsgProfileExt} {user?.ToString()}");
                App.User.Email = user?.Email;
                App.User.Birthday = user?.Birthday;
                Settings.LastUser = App.User;
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _logger.Debug("OnDisappearing.Unsubscribe");
            MessagingCenter.Unsubscribe<App, string>(this, Constants.MsgProfile);
            MessagingCenter.Unsubscribe<App, string>(this, Constants.MsgProfileExt);
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