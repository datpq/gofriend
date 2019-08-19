using System;
using System.IO;
using goFriend.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AccountPage : ContentPage
    {
        private AccountViewModel _viewModel;

        public AccountPage()
        {
            InitializeComponent();
            RefreshMenu();

            BindingContext = _viewModel = new AccountViewModel();

            CellBasicInfos.Tapped += (s, e) => { Navigation.PushAsync(new AccountBasicInfosPage()); };
            CellLogin.Tapped += async (s, e) =>
            {
                await Navigation.PushModalAsync(LoginPage.GetInstance());
                await LoginPage.GetInstance().Wait();
                (App.Current.MainPage as AppShell).RefreshTabs();
                RefreshMenu();
            };
            CellLogout.Tapped += async (s, e) =>
            {
                var answer = await App.DisplayMsgQuestion(res.MsgLogoutConfirm);
                if (!answer) return;
                App.FaceBookManager.Logout();
                App.IsUserLoggedIn = false;
                Settings.IsUserLoggedIn = App.IsUserLoggedIn;
                (App.Current.MainPage as AppShell).RefreshTabs();
                RefreshMenu();
            };
            CellAbout.Tapped += (s, e) => { Navigation.PushAsync(new AboutPage()); };
        }

        public void RefreshMenu()
        {
            TsShells.Clear();
            if (App.IsUserLoggedIn)
            {
                TsShells.Add(CellAvatar);
                TsShells.Add(CellBasicInfos);
                TsShells.Add(CellLogout);
                if (App.User.Image != null)
                {
                    ImgAvatar.Source = ImageSource.FromStream(() => new MemoryStream(App.User.Image));
                }
                else
                {
                    ImgAvatar.Source = App.User.Gender == "female" ? "default_female.jpg" : "default_male.jpg";
                }
                LblFullName.Text = App.User.Name;
                LblMemberSince.Text = string.Format(res.MemberSince, DateTime.Now.ToShortDateString());
            }
            else
            {
                TsShells.Add(CellLogin);
            }
            TsShells.Add(CellAbout);
        }
    }
}