using System;
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

            BindingContext = _viewModel = new AccountViewModel();
            LblFullName.Text = App.User.Name;
            LblMemberSince.Text = string.Format(res.MemberSince, DateTime.Now.ToShortDateString());

            LblBasicInfos.Tapped += (s, e) => { Navigation.PushAsync(new AccountBasicInfosPage()); };
            LblLogout.Tapped += (s, e) =>
            {
                App.FaceBookManager.Logout();
                App.IsUserLoggedIn = false;
                Settings.IsUserLoggedIn = App.IsUserLoggedIn;
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            };
        }
    }
}