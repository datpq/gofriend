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
            TabBrowse.IsEnabled = App.IsUserLoggedIn;
            TabSearch.IsEnabled = App.IsUserLoggedIn;
            TabMap.IsEnabled = App.IsUserLoggedIn;
        }
    }
}
