using goFriend.Views;
using System.Linq;
using Xamarin.Forms;

namespace goFriend
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            //Routing.RegisterRoute(Constants.ROUTE_HOME, typeof(AccountPage));
            Routing.RegisterRoute(Constants.ROUTE_HOME_GROUPCONNECTION, typeof(GroupConnectionPage));
            Routing.RegisterRoute(Constants.ROUTE_HOME_MAP, typeof(MapPage));
            Routing.RegisterRoute(Constants.ROUTE_HOME_ADMIN, typeof(AdminPage));
            Routing.RegisterRoute(Constants.ROUTE_HOME_LOGIN, typeof(LoginPage));
            Routing.RegisterRoute(Constants.ROUTE_HOME_ABOUT, typeof(AboutPage));

            RefreshTabs();
            //TabAdmin.Icon = Extension.GetImageSourceFromFile("admin.png");
            //TabAdmin.Icon = Extension.GetImageUrl("admin.png");
            //TabNotification.Appearing += (sender, args) => TabNotification.Icon = "tab_notification_selected.png";
            //TabNotification.Disappearing += (sender, args) => TabNotification.Icon = "tab_notification.png";
        }

        public void RefreshTabs()
        {
            TabBrowse.IsEnabled = TabMap.IsEnabled = TabChat.IsEnabled =
                App.IsUserLoggedIn && App.User != null && App.User.Active
                && App.MyGroups != null && App.MyGroups.Any(x => x.GroupFriend.Active);
            TabNotification.IsEnabled = App.IsUserLoggedIn && App.User != null && App.User.Active;
        }
    }
}
