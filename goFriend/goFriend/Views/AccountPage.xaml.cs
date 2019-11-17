using System;
using goFriend.Services;
using goFriend.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AccountPage : ContentPage
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();
        private AccountViewModel _viewModel;

        public AccountPage()
        {
            InitializeComponent();
            Tv.Margin = DeviceInfo.Platform == DevicePlatform.iOS ?
                DeviceInfo.Version <= new Version(10, 3, 4) ?
                    new Thickness(0, -62, 0, 0) : new Thickness(0, -30, 0, 0)
                : new Thickness(0, 0, 0, 0);
            Logger.Debug($"Platform={DeviceInfo.Platform}, Version={DeviceInfo.Version}, Margin.Top={Tv.Margin.Top}");

            RefreshMenu();

            BindingContext = _viewModel = new AccountViewModel();

            CellBasicInfo.Tapped += (s, e) => { Navigation.PushAsync(new AccountBasicInfosPage(App.User)); };
            CellGroups.Tapped += (s, e) => { Navigation.PushAsync(new GroupConnectionPage()); };
            CellLogin.Tapped += (s, e) =>
            {
                Navigation.PushAsync(LoginPage.GetInstance(this));
                //await LoginPage.GetInstance().Wait();
                //RefreshMenu();
            };
            CellLogout.Tapped += async (s, e) =>
            {
                if (!await App.DisplayMsgQuestion(res.MsgLogoutConfirm)) return;
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
                if (App.User.Active)
                {
                    TsShells.Add(CellBasicInfo);
                }
                TsShells.Add(CellGroups);
                TsShells.Add(CellLogout);
                ImgAvatar.Source = App.User.GetImageUrl(); // normal 100 x 100
                //ImgAvatar.Source = Extension.GetImageSourceFromFile("admin.png"); // normal 100 x 100
                LblFullName.Text = App.User.Name;
                LblMemberSince.Text = string.Format(res.MemberSince, App.User.CreatedDate?.ToShortDateString());
                if (!App.User.Active)
                {
                    App.DisplayMsgInfo(res.MsgInactiveUserWarning);
                }
            }
            else
            {
                TsShells.Add(CellLogin);
            }
            TsShells.Add(CellAbout);
        }
    }
}