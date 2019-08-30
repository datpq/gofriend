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

            CellBasicInfo.Tapped += (s, e) => { Navigation.PushAsync(new AccountBasicInfosPage()); };
            CellLogin.Tapped += (s, e) =>
            {
                Navigation.PushModalAsync(LoginPage.GetInstance(this));
                //await LoginPage.GetInstance().Wait();
                //RefreshMenu();
            };
            CellLogout.Tapped += async (s, e) =>
            {
                var answer = await App.DisplayMsgQuestion(res.MsgLogoutConfirm);
                if (!answer) return;
                App.FaceBookManager.Logout();
                App.IsUserLoggedIn = false;
                Settings.IsUserLoggedIn = App.IsUserLoggedIn;
                RefreshMenu();
            };
            CellAbout.Tapped += (s, e) => { Navigation.PushAsync(new AboutPage()); };
        }

        public void RefreshMenu()
        {
            (App.Current.MainPage as AppShell).RefreshTabs();
            TsShells.Clear();
            if (App.IsUserLoggedIn && App.User != null)
            {
                TsShells.Add(CellAvatar);
                TsShells.Add(CellBasicInfo);
                TsShells.Add(CellInfo);
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
                LblMemberSince.Text = string.Format(res.MemberSince, App.User.CreatedDate?.ToShortDateString());
            }
            else
            {
                TsShells.Add(CellLogin);
            }
            TsShells.Add(CellAbout);
        }
    }
}