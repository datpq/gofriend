using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using goFriend.Controls;
using goFriend.DataModel;
using goFriend.Services;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public MapPage()
        {
            InitializeComponent();

            DphFriendSelection.SelectedGroupName = Settings.LastMapPageGroupNme;
            DphFriendSelection.Initialize((selectedGroup, searchText, arrFixedCats, arrCatValues) =>
            {
                Settings.LastMapPageGroupNme = selectedGroup.Group.Name;
                RefreshComponentsVisibility();
                if (!Map.IsVisible) return;
                App.FriendStore.GetGroupFriends(selectedGroup.Group.Id, true, 0, 0, true, searchText, arrCatValues).ContinueWith(task =>
                {
                    var catGroupFriends = task.Result;
                    Map.Pins.Clear();
                    foreach (var groupFriend in catGroupFriends)
                    {
                        if (groupFriend.Friend.Location != null && groupFriend.Friend.ShowLocation == true)
                        {
                            var dphPin = new DphPin(Map)
                            {
                                Position = new Position(groupFriend.Friend.Location.Y, groupFriend.Friend.Location.X),
                                Title = groupFriend.Friend.Name,
                                SubTitle1 = $"{res.Groups} {selectedGroup.Group.Name}",
                                SubTitle2 = groupFriend.GetCatValueDisplay(arrFixedCats.Count),
                                IconUrl = groupFriend.Friend.GetImageUrl(),
                                IsDraggable = false,
                                Type = PinType.Place
                            };
                            dphPin.Pin.Tag = dphPin;
                            Map.Pins.Add(dphPin.Pin);
                        }
                    }
                    Map.MoveToRegionToCoverAllPins();
                }, TaskScheduler.FromCurrentSynchronizationContext());
            });
        }

        protected override async void OnAppearing()
        {
            if (Device.RuntimePlatform == Device.iOS && App.User.ShowLocation == true)
            {
                var setting = await App.FriendStore.GetSetting();
                if (!setting.DefaultShowLocation)
                {
                    Logger.Debug("iOS Location Check Compliance");
                    try
                    {
                        UserDialogs.Instance.ShowLoading(res.Processing);
                        if (!await Extension.CheckIfLocationIsGranted())
                        {
                            Logger.Debug("LocationAccess is not Granted. Update ShowLocation");
                            var result = await App.FriendStore.SaveBasicInfo(new Friend { Id = App.User.Id, ShowLocation = false });
                            if (!result) return;
                            App.User.ShowLocation = false;
                            Settings.LastUser = App.User;
                            RefreshComponentsVisibility();
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    finally
                    {
                        UserDialogs.Instance.HideLoading();
                    }
                }
            }
        }

        private void RefreshComponentsVisibility()
        {
            Map.IsVisible = App.User.Location != null && App.User.ShowLocation == true;
            LabelNoLocation.IsVisible = !Map.IsVisible;
        }
    }
}