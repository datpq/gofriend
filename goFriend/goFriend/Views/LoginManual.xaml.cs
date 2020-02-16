using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginManual : Rg.Plugins.Popup.Pages.PopupPage
    {
        public LoginManual()
        {
            InitializeComponent();
            var lblCancelTap = new TapGestureRecognizer();
            lblCancelTap.Tapped += (s, e) =>
            {
                PopupNavigation.Instance.PopAsync();
            };
            lblCancel.GestureRecognizers.Add(lblCancelTap);
            var lblForgottenPasswordTap = new TapGestureRecognizer();
            lblForgottenPasswordTap.Tapped += (s, e) =>
            {
                Launcher.OpenAsync("http://google.com");
            };
            LblForgottenPassword.GestureRecognizers.Add(lblForgottenPasswordTap);
        }

        public void ImgShowPass_Tap(object sender, EventArgs args)
        {
            EntryPassword.IsPassword = !EntryPassword.IsPassword;
            //ImgShowPass.Source = EntryPassword.IsPassword ? "eye_off.png" : "eye_on.png";
        }

        private void CmdLogin_Click(object sender, EventArgs e)
        {
        }
    }
}