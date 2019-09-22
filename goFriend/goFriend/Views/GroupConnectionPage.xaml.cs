using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GroupConnectionPage : ContentPage
    {
        public GroupConnectionPage()
        {
            InitializeComponent();

            var groups = App.FriendStore.GetGroups();

        }
    }
}