namespace goFriend
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            //TabAdmin.Icon = Extension.GetImageSourceFromFile("admin.png");
            //TabAdmin.Icon = Extension.GetImageUrl("admin.png");
        }

        public void RefreshTabs()
        {
            TabBrowse.IsEnabled = TabAdmin.IsEnabled = TabMap.IsEnabled = App.IsUserLoggedIn && App.User != null && App.User.Active;
        }
    }
}
