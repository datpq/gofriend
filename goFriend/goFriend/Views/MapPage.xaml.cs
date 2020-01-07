using System.Threading.Tasks;
using goFriend.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        public MapPage()
        {
            InitializeComponent();

            DphFriendSelection.Initialize((selectedGroup, arrFixedCats, arrCatValues) =>
            {
                App.FriendStore.GetGroupFriends(selectedGroup.Group.Id, true, true, arrCatValues).ContinueWith(task =>
                {
                    var catGroupFriends = task.Result;
                    Map.Pins.Clear();
                    foreach (var groupFriend in catGroupFriends)
                    {
                        Map.Pins.Add(new DphPin
                        {
                            Position = groupFriend.Friend.Location == null ? DphMap.DefaultPosition :
                                new Position(groupFriend.Friend.Location.Y, groupFriend.Friend.Location.X),
                            Title = groupFriend.Friend.Name,
                            SubTitle1 = $"{res.Groups} {selectedGroup.Group.Name}",
                            SubTitle2 = groupFriend.GetCatValueDisplay(arrFixedCats.Count),
                            IconUrl = groupFriend.Friend.GetImageUrl(),
                            Draggable = false,
                            Type = PinType.Place
                        });
                    }
                    Map.MoveToRegionToCoverAllPins();
                }, TaskScheduler.FromCurrentSynchronizationContext());
            });
        }
    }
}