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
        }
    }
}