using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Services;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
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