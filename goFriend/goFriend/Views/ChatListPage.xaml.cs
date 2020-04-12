using System;
using goFriend.ViewModels;
using PCLAppConfig;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatListPage : ContentPage
    {
        private readonly ChatListViewModel _viewModel;
        private static readonly int Timeout = int.Parse(ConfigurationManager.AppSettings["ListViewTimeout"]);
        private DateTime _lastRefreshDateTime = DateTime.Today.AddYears(-1); //default value is a very small value

        public ChatListPage()
        {
            InitializeComponent();
            Appearing += (sender, args) =>
            {
                if (!_viewModel.IsRefreshing && (_lastRefreshDateTime.AddMinutes(Timeout) < DateTime.Now))
                {
                    _viewModel.RefreshCommand.Execute(null);
                    _lastRefreshDateTime = DateTime.Now;
                }
            };
            BindingContext = _viewModel = new ChatListViewModel();
        }

        private async void Cell_OnTapped(object sender, EventArgs e)
        {
            var selectedItem = (ChatListItemViewModel)Lv.SelectedItem;
            var chatPage = new NavigationPage(new ChatPage(selectedItem));
            await Navigation.PushModalAsync(chatPage);
        }
    }
}