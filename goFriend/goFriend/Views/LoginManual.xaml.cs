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
            var lblCancel_tap = new TapGestureRecognizer();
            lblCancel_tap.Tapped += (s, e) =>
            {
                PopupNavigation.Instance.PopAsync();
            };
            lblCancel.GestureRecognizers.Add(lblCancel_tap);
            var lblForgottenPassword_tap = new TapGestureRecognizer();
            lblForgottenPassword_tap.Tapped += (s, e) =>
            {
                var uri = new Uri("http://google.com");
                //Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
                Device.OpenUri(uri);
            };
            lblForgottenPassword.GestureRecognizers.Add(lblForgottenPassword_tap);
        }

        public void ImgShowPass_Tap(object sender, EventArgs args)
        {
            entryPassword.IsPassword = entryPassword.IsPassword ? false : true;
            ImgShowPass.Source = entryPassword.IsPassword ? "eye_off24.png" : "eye_on24.png";
        }

        private void CmdLogin_Click(object sender, EventArgs e)
        {
        }
    }
}