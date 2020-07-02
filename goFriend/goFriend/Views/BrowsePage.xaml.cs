using goFriend.DataModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BrowsePage : ContentPage
    {
        public BrowsePage()
        {
            InitializeComponent();

            DphFriendList.Initialize(async (selectedItem) =>
            {
                var selectedGroupFriend = (GroupFriend)selectedItem.SelectedObject;
                var accountBasicInfoPage = new AccountBasicInfosPage();
                //DphFriendList.DphFriendSelection
                await accountBasicInfoPage.Initialize(DphFriendList.DphFriendSelection.SelectedGroup.Group,
                    selectedGroupFriend, DphFriendList.DphFriendSelection.ArrFixedCats.Count);
                await Navigation.PushAsync(accountBasicInfoPage);
            });
        }
    }
}