using Acr.UserDialogs;
using goFriend.Controls;
using goFriend.DataModel;
using goFriend.Helpers;
using goFriend.Services;
using goFriend.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapOnlinePage : ContentPage
    {
        private const int COMMAND_DISABLING_TIMEOUT = 1; // in minutes

        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());
        public static readonly Dictionary<int, MapOnlineViewModel> MapOnlineInfo = new Dictionary<int, MapOnlineViewModel>();
        private static readonly Dictionary<int, List<DphPin>> MapOnlinePins = new Dictionary<int, List<DphPin>>();
        private readonly DphTimer _timer;

        public MapOnlinePage()
        {
            InitializeComponent();
            _timer = new DphTimer(() => ((MapOnlineViewModel)BindingContext).DisabledExpiredTime = DateTime.MinValue);
            CmdPlay.BackgroundColor = CmdStop.BackgroundColor = CmdRefresh.BackgroundColor = BackgroundColor;
            PickerRadius.Title = $"{res.Select} {res.Radius}";
            LblRadius.Text = $"{res.Radius}:";

            DphFriendSelection.SelectedGroupName = Settings.LastMapPageGroupName;
            DphFriendSelection.Initialize(async (selectedGroup, searchText, arrFixedCats, arrCatValues) =>
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
                    await RecenterMap();
                }
            });

            App.NotificationService.CancelNotification(Models.NotificationType.AppearOnMap);
        }

        private async Task RecenterMap()
        {
            try
            {
                var vm = (MapOnlineViewModel)BindingContext;
                UserDialogs.Instance.ShowLoading(res.Processing);
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(Constants.GeolocationRequestTimeout));
                var location = await Geolocation.GetLocationAsync(request);

                if (location == null)
                {
                    App.DisplayMsgInfo(res.MsgNoGpsWarning);
                    return;
                }
                //set up Pins
                var pin = new DphPin
                {
                    Position = new Position(location.Latitude, location.Longitude),
                    Title = App.User.Name,
                    SubTitle1 = $"{res.Groups} {vm.Group.Name}",
                    SubTitle2 = vm.GroupFriend.GetCatValueDisplay(vm.FixedCatsCount),
                    IconUrl = App.User.GetImageUrl(),
                    UserRight = Constants.SuperUserIds.Contains(App.User.Id) ? UserType.Normal : vm.GroupFriend.UserRight,
                    //Url = $"facebook://facebook.com/info?user={_viewModel.Friend.FacebookId}",
                    IsDraggable = false,
                    Type = PinType.Place
                };

                pin.Pin.Tag = pin;
                Map.Pins.Clear();
                Map.Pins.Add(pin.Pin);

                Map.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Position(pin.Position.Latitude, pin.Position.Longitude),
                    Distance.FromKilometers(vm.Radius == 0 ? 10 : vm.Radius * 2)));
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        private async void CmdPlay_Clicked(object sender, EventArgs e)
        {
            var vm = (MapOnlineViewModel)BindingContext;
            vm.IsRunning = !vm.IsRunning;
            if (!App.LocationService.IsRunning())
            {
                App.LocationService.Start();
            }
            App.LocationService.RefreshStatus();
            vm.DisabledExpiredTime = DateTime.Now.AddMinutes(COMMAND_DISABLING_TIMEOUT);
            _timer.StartingTime = vm.DisabledExpiredTime;
            _timer.Start();
            await RecenterMap();
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
            vm.DisabledExpiredTime = DateTime.Now.AddMinutes(COMMAND_DISABLING_TIMEOUT);
            _timer.StartingTime = vm.DisabledExpiredTime;
            _timer.Start();
        }

        private async void PickerRadius_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(PickerRadius.SelectedItem is RadiusItemModel selectedRadius)) return;
            var vm = (MapOnlineViewModel)BindingContext;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (vm.Radius != selectedRadius.Radius)
            {
                vm.Radius = selectedRadius.Radius;
                if (vm.IsRunning)
                {
                    vm.DisabledExpiredTime = DateTime.Now.AddMinutes(COMMAND_DISABLING_TIMEOUT);
                    _timer.StartingTime = vm.DisabledExpiredTime;
                    _timer.Start();
                    App.LocationService.RefreshStatus();
                    await RecenterMap();
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

        public static void ReceiveLocation(FriendLocation friendLocation)
        {
            if (friendLocation.SharingInfo == null) return;
            friendLocation.ModifiedDate = DateTime.Now;
            friendLocation.SharingInfo.Split(DataModel.Extension.SepMain).ToList().ForEach(x =>
            {
                var groupId = int.Parse(x.Split(DataModel.Extension.SepSub)[0]);
                if (!MapOnlinePins.ContainsKey(groupId))
                {
                }
            });
        }

        private void CmdRefresh_Clicked(object sender, EventArgs e)
        {
        }
    }
}