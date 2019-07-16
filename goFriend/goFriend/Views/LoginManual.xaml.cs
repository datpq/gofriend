using Rg.Plugins.Popup.Services;
using System;
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
                var uri = new Uri("http://google.com");
                //Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
                Device.OpenUri(uri);
            };
            LblForgottenPassword.GestureRecognizers.Add(lblForgottenPasswordTap);
        }

        public void ImgShowPass_Tap(object sender, EventArgs args)
        {
            EntryPassword.IsPassword = !EntryPassword.IsPassword;
            ImgShowPass.Source = EntryPassword.IsPassword ? "eye_off24.png" : "eye_on24.png";
        }

        private void CmdLogin_Click(object sender, EventArgs e)
        {
        }
    }
}