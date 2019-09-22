namespace goFriend
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        public void RefreshTabs()
        {
            TabBrowse.IsEnabled = TabSearch.IsEnabled = TabMap.IsEnabled = App.IsUserLoggedIn && App.User != null && App.User.Active;
        }
    }
}
