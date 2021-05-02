using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using goFriend.Controls;
using goFriend.DataModel;
using goFriend.Services;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;
using System.Linq;
using System.Collections.Generic;
using Xamarin.Essentials;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());
        private bool _isFirstZoomDone;
        private int _friendId;

        public MapPage() : this(Settings.LastMapPageGroupName) {}

        public MapPage(string selectedGroupName, int friendId = 0)
        {
            InitializeComponent();

            Map.ClusterOptions.Buckets[0] = Constants.MinimumClusterSize;
            Map.ClusterOptions.SetMinimumClusterSize(Constants.MinimumClusterSize);
            Ads.AdUnitId = Device.RuntimePlatform == Device.Android ? Constants.AdBannerIdAndroid : Constants.AdBannerIdiOS;

            DphFriendSelection.SelectedGroupName = selectedGroupName;
            _friendId = friendId;
            _isFirstZoomDone = friendId == 0; // first time initalization, do not zoom to the Friend if there's no friendId
            DphFriendSelection.Initialize((selectedGroup, searchText, arrFixedCats, arrCatValues) =>
            {
                Settings.LastMapPageGroupName = selectedGroup.Group.Name;
                RefreshComponentsVisibility();
                if (!Map.IsVisible) return;
                App.FriendStore.GetGroupFriends(selectedGroup.Group.Id, true, 0, 0, true, true, searchText, arrCatValues).ContinueWith(task =>
                {
                    var catGroupFriends = task.Result;
                    Map.Pins.Clear();
                    foreach (var groupFriend in catGroupFriends)
                    {
                        if (groupFriend.Friend.Location != null && groupFriend.Friend.ShowLocation == true)
                        {
                            var dphPin = new DphPin
                            {
                                Position = new Position(groupFriend.Friend.Location.Y, groupFriend.Friend.Location.X),
                                Title = groupFriend.Friend.Name,
                                SubTitle1 = $"{res.Groups} {selectedGroup.Group.Name}",
                                SubTitle2 = groupFriend.GetCatValueDisplay(arrFixedCats.Count),
                                IconUrl = groupFriend.Friend.GetImageUrl(),
                                UserRight = groupFriend.FriendId.IsSuperUser() ? UserType.Pending :
                                groupFriend.UserRight == UserType.Normal ? UserType.Pending : groupFriend.UserRight, // all normal users have Pending (offline) icon
                                User = groupFriend.Friend,
                                GroupId = groupFriend.GroupId,
                                GroupName = selectedGroup.Group.Name,
                                //UserRight = Constants.SuperUserIds.Contains(groupFriend.FriendId) ? UserType.Normal : groupFriend.UserRight,
                                IsDraggable = false,
                                Type = PinType.Place
                            };
                            dphPin.Pin.Tag = dphPin;
                            Map.Pins.Add(dphPin.Pin);
                        }
                    }
                    if (!_isFirstZoomDone && _friendId != 0)
                    {
                        _isFirstZoomDone = true;
                        var pin = Map.Pins.FirstOrDefault(x => (x.Tag as DphPin)?.User.Id == _friendId);
                        if (pin != null)
                        {
                            Map.MoveToRegion(MapSpan.FromCenterAndRadius(
                                new Position(pin.Position.Latitude, pin.Position.Longitude), Distance.FromKilometers(MapExtension.DefaultDistance)));
                        }
                    }
                    else
                    {
                        Map.MoveToRegionToCoverAllPins();
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            });
        }

        public static async void DisplayContextMenu(DphPin dphPin, bool withOfflineMap)
        {
            var menuItems = new List<string>() { res.NavigateTo, Constants.ImgNavigate };
            var menuItemActions = new List<Action>()
            {
                new Action(async () => {
                    var location = new Location(dphPin.Position.Latitude, dphPin.Position.Longitude);
                    var options =  new MapLaunchOptions { NavigationMode = NavigationMode.Driving };

                    await Xamarin.Essentials.Map.OpenAsync(location, options);
                })
            };
            if (dphPin.GroupId.HasValue)
            {
                menuItems.AddRange(new[] { res.BasicInfos, Constants.ImgAccountInfo });
                menuItemActions.Add(new Action(async () => await App.GotoAccountInfo(dphPin.GroupId.Value, dphPin.User.Id)));
            }
            if (withOfflineMap)
            {
                var friend = await App.FriendStore.GetFriendInfo(dphPin.User.Id);
                if (friend.Location != null)
                {
                    menuItems.AddRange(new[] { res.MapOffline, Constants.ImgMap });
                    menuItemActions.Add(
                        new Action(() => App.Current.MainPage.Navigation.PushAsync(new MapPage(dphPin.GroupName, dphPin.User.Id))));
                }
            }
            App.DisplayContextMenu(menuItems.ToArray(), menuItemActions.ToArray());
        }

        protected override void OnDisappearing()
        {
            MessagingCenter.Unsubscribe<DphClusterMap, DphPin>(Map, Constants.MsgInfoWindowClick);
        }

        protected override async void OnAppearing()
        {
            MessagingCenter.Subscribe<DphClusterMap, DphPin>(Map,
                Constants.MsgInfoWindowClick, (sender, dphPin) => DisplayContextMenu(dphPin, false));
            return;
            if (Device.RuntimePlatform == Device.iOS && App.User.ShowLocation == true)
            {
                var setting = await App.FriendStore.GetSetting();
                if (setting == null) return;
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