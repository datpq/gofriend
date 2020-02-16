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

        //public void RefreshTabs()
        //{
        //    TabBrowse.IsEnabled = TabMap.IsEnabled = TabNotification.IsEnabled =
        //        (App.IsUserLoggedIn && App.User != null && App.User.Active && App.User.Location != null);
        //    foreach (var tab in new[] {TabBrowse, TabMap, TabNotification})
        //    {
        //        if (tab.IsEnabled && !Tabs.Items.Contains(tab))
        //        {
        //            Tabs.Items.Add(tab);
        //        }
        //        else if (!tab.IsEnabled && Tabs.Items.Contains(tab))
        //        {
        //            Tabs.Items.Remove(tab);
        //        }
        //    }
            //while (Tabs.Items.Count > 1)
            //{
            //    Tabs.Items.RemoveAt(1);
            //}

            //if (App.IsUserLoggedIn && App.User != null && App.User.Active && App.User.Location != null)
            //{
            //    Tabs.Items.Add(TabBrowse);
            //    Tabs.Items.Add(TabMap);
            //    //Tabs.Items.Add(TabChat);
            //    Tabs.Items.Add(TabNotification);
            //}
        //}
    }
}
