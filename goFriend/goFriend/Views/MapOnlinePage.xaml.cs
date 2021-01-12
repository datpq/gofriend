using goFriend.Controls;
using goFriend.DataModel;
using goFriend.Helpers;
using goFriend.Models;
using goFriend.Services;
using goFriend.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapOnlinePage : ContentPage
    {
        public static MapOnlinePage Instance = null;
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());
        public static FriendLocation MyLocation;
        public static readonly Dictionary<int, MapOnlineViewModel> MapOnlineInfo = new Dictionary<int, MapOnlineViewModel>();
        private readonly DphTimer _timer;
        private bool _mapNeedRecentering = true;

        //list of all online Friend Ids of all radius from all groups
        private List<int> _onlineFriendIds = new List<int>();
        //_wentOfflineFriends is used when an user went offline (e.g. wifi swithed off), 5 minutes later he/she goes back online
        //then notificaton will not be sent
        private readonly Dictionary<int, DateTime> _wentOfflineFriends = new Dictionary<int, DateTime>();

        public MapOnlinePage()
        {
            InitializeComponent();
            Instance = this;
            _timer = new DphTimer(() => ((MapOnlineViewModel)BindingContext).DisabledExpiredTime = DateTime.MinValue);
            Device.StartTimer(TimeSpan.FromSeconds(Constants.MAPONLINE_REFRESH_INTERVAL), () =>
            {
                //find new online friends to send notification
                var newOnlineFriendIds = new List<int>();
                MapOnlineInfo.Where(x => x.Value.IsRunning).Select(x => x.Value).ToList().ForEach(x =>
                {
                    x.Refresh();
                    newOnlineFriendIds.AddRange(x.RadiusSelectedItem.FriendLocations.Where(
                        x => x.FriendId != App.User.Id).Select(y => y.FriendId));
                });
                newOnlineFriendIds = newOnlineFriendIds.Distinct().ToList();
                var inboxLines = new List<string[]>();
                //refresh remove all expired items
                _wentOfflineFriends.Where(x => x.Value.AddMinutes(
                    Constants.MAPONLINE_OFFLINE_TIMEOUT) < DateTime.Now).Select(x => x.Key).ToList().ForEach(
                    x => _wentOfflineFriends.Remove(x));
                //appear online and not offline recently
                newOnlineFriendIds.Except(_onlineFriendIds).Except(_wentOfflineFriends.Keys).ToList().ForEach(async x =>
                {
                    var friendInfo = await App.FriendStore.GetFriendInfo(x);
                    Logger.Debug($"{friendInfo.Name} appears online.");
                    inboxLines.Add(new[] { friendInfo.Name, res.AppearOnline });
                });
                if (inboxLines.Count > 0)
                {
                    var appearOnlineNotif = new ServiceNotification
                    {
                        ContentTitle = AppInfo.Name,
                        ContentText = null,
                        SummaryText = null,
                        LargeIconUrl = $"{Constants.HomePageUrl}/logos/g1.png",
                        NotificationType = Models.NotificationType.AppearOnMap,
                        InboxLines = inboxLines
                    };
                    App.NotificationService.SendNotification(appearOnlineNotif);
                }
                //refresh wentoffline list
                _onlineFriendIds.Except(newOnlineFriendIds).ToList().ForEach(async x =>
                {
                    var friendInfo = await App.FriendStore.GetFriendInfo(x);
                    Logger.Debug($"{friendInfo.Name} went offline.");
                    _wentOfflineFriends[x] = DateTime.Now;
                });
                //update new online list
                _onlineFriendIds = newOnlineFriendIds;
                Logger.Debug($"online={_onlineFriendIds.Count}, offline={_wentOfflineFriends.Count}");

                var vm = (MapOnlineViewModel)BindingContext;
                if (vm.IsRunning)
                {
                    var pins = vm.GetPins();
                    Map.Pins.Clear();
                    pins.ForEach(x =>
                    {
                        x.Pin.Tag = x;
                        Map.Pins.Add(x.Pin);
                    });
                    Logger.Debug($"Pins count = {pins.Count}");
                }
                return true; // True = Repeat again, False = Stop the timer
            });
            CmdPlay.BackgroundColor = CmdStop.BackgroundColor = BackgroundColor;
            PickerRadius.Title = $"{res.Select} {res.Radius}";
            LblRadius.Text = $"{res.Radius}:";

            DphFriendSelection.SelectedGroupName = Settings.LastMapPageGroupName;
            DphFriendSelection.Initialize((selectedGroup, searchText, arrFixedCats, arrCatValues) =>
            {
                _timer.Stop();
                Settings.LastMapPageGroupName = selectedGroup.Group.Name;
                if (!MapOnlineInfo.ContainsKey(selectedGroup.Group.Id))
                {
                    selectedGroup.GroupFriend.Group = selectedGroup.Group;
                    MapOnlineInfo.Add(selectedGroup.Group.Id, new MapOnlineViewModel {
                        Group = selectedGroup.Group,
                        GroupFriend = selectedGroup.GroupFriend,
                        FixedCatsCount = arrFixedCats.Count
                    });
                }
                BindingContext = MapOnlineInfo[selectedGroup.Group.Id];
                var vm = MapOnlineInfo[selectedGroup.Group.Id];
                if (!vm.IsEnabled) //start timer to make it Enabled after some time
                {
                    _timer.StartingTime = vm.DisabledExpiredTime;
                    _timer.Start();
                }
                if (vm.IsRunning)
                {
                    _mapNeedRecentering = true;
                    RecenterMap();
                }
            });

            App.NotificationService.CancelNotification(Models.NotificationType.AppearOnMap);
        }

        private void RecenterMap()
        {
            if (_mapNeedRecentering && MyLocation != null && MyLocation.IsOnline())
            {
                var vm = (MapOnlineViewModel)BindingContext;
                Map.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Position(MyLocation.Location.Y, MyLocation.Location.X),
                    Distance.FromKilometers(vm.Radius == 0 ? 10 : vm.Radius * 1.2)));
                _mapNeedRecentering = false;
            }
        }

        private void CmdPlay_Clicked(object sender, EventArgs e)
        {
            var vm = (MapOnlineViewModel)BindingContext;
            vm.IsRunning = !vm.IsRunning;
            if (!App.LocationService.IsRunning())
            {
                App.LocationService.Start();
            }
            App.LocationService.RefreshStatus();
            vm.DisabledExpiredTime = DateTime.Now.AddSeconds(Constants.MAPONLINE_COMMAND_DISABLED_TIMEOUT);
            _timer.StartingTime = vm.DisabledExpiredTime;
            _timer.Start();
            _mapNeedRecentering = true;
            RecenterMap();
        }

        private void CmdStop_Clicked(object sender, EventArgs e)
        {
            var vm = (MapOnlineViewModel)BindingContext;
            vm.IsRunning = !vm.IsRunning;
            vm.Items.ToList().ForEach(x => x.OnlineFriends = 0);
            if (MapOnlineInfo.All(x => !x.Value.IsRunning))
            {
                if (App.LocationService.IsRunning())
                {
                    App.LocationService.Pause();
                }
            }
            App.LocationService.RefreshStatus();
            vm.DisabledExpiredTime = DateTime.Now.AddSeconds(Constants.MAPONLINE_COMMAND_DISABLED_TIMEOUT);
            _timer.StartingTime = vm.DisabledExpiredTime;
            _timer.Start();
        }

        private void PickerRadius_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(PickerRadius.SelectedItem is RadiusItemModel selectedRadius)) return;
            var vm = (MapOnlineViewModel)BindingContext;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (vm.Radius != selectedRadius.Radius)
            {
                vm.Radius = selectedRadius.Radius;
                if (vm.IsRunning)
                {
                    vm.DisabledExpiredTime = DateTime.Now.AddSeconds(Constants.MAPONLINE_COMMAND_DISABLED_TIMEOUT);
                    _timer.StartingTime = vm.DisabledExpiredTime;
                    _timer.Start();
                    App.LocationService.RefreshStatus();
                    _mapNeedRecentering = true;
                    RecenterMap();
                }
            }
        }

        public static List<string[]> GetMapOnlineStatus()
        {
            return MapOnlineInfo.Where(x => x.Value.IsRunning).Select(
                x => new[] { x.Value.Group.Name, x.Value.RadiusSelectedItem.Display }).ToList();
        }

        public static string GetSharingInfo()
        {
            return MapOnlineInfo.Where(x => x.Value.IsRunning)
                .Select(x => $"{x.Key}{DataModel.Extension.SepSub}{x.Value.Radius}")
                .Aggregate((i, j) => $"{i}{DataModel.Extension.SepSub}{j}");
        }

        public async void ReceiveLocation(FriendLocation friendLocation)
        {
            if (friendLocation.SharingInfo == null) return;
            friendLocation.ModifiedDate = DateTime.Now;
            if (friendLocation.FriendId == App.User.Id) //receive my own location. Stored to use in distance calculation
            {
                friendLocation.Friend = App.User;
                MyLocation = friendLocation;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    RecenterMap();//First time receiving Location, recenter the map
                });
            }
            else
            {
                var friend = await App.FriendStore.GetFriendInfo(friendLocation.FriendId);
                friendLocation.Friend = friend;
            }
            friendLocation.SharingInfo.Split(DataModel.Extension.SepMain).ToList().ForEach(x =>
            {
                var groupId = int.Parse(x.Split(DataModel.Extension.SepSub)[0]);
                if (MapOnlineInfo.ContainsKey(groupId))
                {
                    MapOnlineInfo[groupId].ReceiveLocation(friendLocation); //await ?
                }
            });
        }
    }
}