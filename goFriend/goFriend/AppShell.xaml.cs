using Xamarin.Forms;

namespace goFriend
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            //TabAdmin.Icon = Extension.GetImageSourceFromFile("admin.png");
            //TabAdmin.Icon = Extension.GetImageUrl("admin.png");
            //TabNotification.Appearing += (sender, args) => TabNotification.Icon = "tab_notification_selected.png";
            //TabNotification.Disappearing += (sender, args) => TabNotification.Icon = "tab_notification.png";
        }

        public void RefreshTabs()
        {
            TabBrowse.IsEnabled = TabMap.IsEnabled = TabChat.IsEnabled = TabNotification.IsEnabled =
                App.IsUserLoggedIn && App.User != null && App.User.Active && App.User.Location != null;
        }
    }
}
