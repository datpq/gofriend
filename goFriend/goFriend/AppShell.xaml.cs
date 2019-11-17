using System.Linq;
using goFriend.DataModel;

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

        public async void RefreshTabs()
        {
            TabBrowse.IsEnabled = TabMap.IsEnabled = App.IsUserLoggedIn && App.User != null && App.User.Active;
            if (App.User != null && App.User.Active)
            {
                await App.TaskGetMyGroups;
                TabAdmin.IsEnabled = App.MyGroups != null && App.MyGroups.Any(x => x.GroupFriend.UserRight >= UserType.Admin);
            }
            else
            {
                TabAdmin.IsEnabled = false;
            }
        }
    }
}
