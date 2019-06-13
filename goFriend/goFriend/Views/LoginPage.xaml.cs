using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Services;
using goFriend.Services;
using goFriend.Models;
using Newtonsoft.Json;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        private static readonly ILogger logger = DependencyService.Get<ILogManager>().GetLog();

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
            logger.Debug("OnAppearing.Unsubscribe");
            MessagingCenter.Unsubscribe<App, string>(this, Constants.MsgProfile);
            logger.Debug("OnAppearing.Subscribe");
            MessagingCenter.Subscribe<App, User>(this, Constants.MsgProfile, (arg1, user) =>
            {
                logger.Debug($"Received: {user}");
                App.User = user;
                Settings.LastUser = user;
                //(Application.Current as App).MainPage.DisplayAlert("Success", $"Authentication succeed: {user.Name}", "OK");
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            logger.Debug("OnDisappearing.Unsubscribe");
            MessagingCenter.Unsubscribe<App, string>(this, Constants.MsgProfile);
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