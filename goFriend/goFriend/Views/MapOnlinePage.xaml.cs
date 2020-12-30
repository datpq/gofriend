using Acr.UserDialogs;
using goFriend.Controls;
using goFriend.DataModel;
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
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());
        public static readonly Dictionary<int, MapOnlineViewModel> MapOnlineInfo = new Dictionary<int, MapOnlineViewModel>();
        private bool _firingRadiusSelectedIndexChanged = true;

        public MapOnlinePage()
        {
            InitializeComponent();
            ImgPlay.BackgroundColor = ImgStop.BackgroundColor = ImgRefresh.BackgroundColor = BackgroundColor;
            PickerRadius.Title = $"{res.Select} {res.Radius}";
            LblRadius.Text = $"{res.Radius}:";

            DphFriendSelection.SelectedGroupName = Settings.LastMapPageGroupName;
            DphFriendSelection.Initialize((selectedGroup, searchText, arrFixedCats, arrCatValues) =>
            {
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
                RefreshRadiusDisplay();
            });

            App.NotificationService.CancelNotification(Models.NotificationType.AppearOnMap);

            App.LocationService.StateChanged += LocationService_StateChanged;
        }

        ~MapOnlinePage()
        {
            App.LocationService.StateChanged -= LocationService_StateChanged;
        }

        private async void RefreshRadiusDisplay()
        {
            _firingRadiusSelectedIndexChanged = false;
            var viewModel = (MapOnlineViewModel)BindingContext;
            for (var i = 0; i < PickerRadius.Items.Count; i++)
            {
                if (PickerRadius.Items[i].Equals(viewModel.RadiusDisplay))
                {
                    PickerRadius.SelectedIndex = i;
                    break;
                }
            }
            if (PickerRadius.SelectedIndex < 0 && PickerRadius.Items.Count > 0)
            {
                Logger.Error("Something went wrong here. Radius not found. Select the first one");
                PickerRadius.SelectedIndex = 0;
            }
            if (viewModel.IsRunning)
            {
                App.LocationService.RefreshStatus();
                await RecenterMap();
            }
            _firingRadiusSelectedIndexChanged = true;
        }

        private static void LocationService_StateChanged(object sender, System.EventArgs e)
        {
            if (!App.LocationService.IsRunning())
            {
                MapOnlineInfo.Where(x => x.Value.IsRunning).ToList().ForEach(x => x.Value.IsRunning = false);
            }
        }

        private async Task RecenterMap()
        {
            try
            {
                var viewModel = (MapOnlineViewModel)BindingContext;
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
                    SubTitle1 = $"{res.Groups} {viewModel.Group.Name}",
                    SubTitle2 = viewModel.GroupFriend.GetCatValueDisplay(viewModel.FixedCatsCount),
                    IconUrl = App.User.GetImageUrl(),
                    UserRight = Constants.SuperUserIds.Contains(App.User.Id) ? UserType.Normal : viewModel.GroupFriend.UserRight,
                    //Url = $"facebook://facebook.com/info?user={_viewModel.Friend.FacebookId}",
                    IsDraggable = false,
                    Type = PinType.Place
                };

                pin.Pin.Tag = pin;
                Map.Pins.Clear();
                Map.Pins.Add(pin.Pin);

                Map.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Position(pin.Position.Latitude, pin.Position.Longitude),
                    Distance.FromKilometers(viewModel.Radius == 0 ? 10 : viewModel.Radius * 2)));
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

        private async void CmdPlay_Clicked(object sender, System.EventArgs e)
        {
            var viewModel = (MapOnlineViewModel)BindingContext;
            viewModel.IsRunning = !viewModel.IsRunning;
            if (!App.LocationService.IsRunning())
            {
                App.LocationService.Start();
            }
            App.LocationService.RefreshStatus();
            await RecenterMap();
        }

        private void CmdPause_Clicked(object sender, System.EventArgs e)
        {
            var viewModel = (MapOnlineViewModel)BindingContext;
            viewModel.IsRunning = !viewModel.IsRunning;
            if (MapOnlineInfo.All(x => !x.Value.IsRunning))
            {
                if (App.LocationService.IsRunning())
                {
                    App.LocationService.Pause();
                }
            }
            App.LocationService.RefreshStatus();
        }

        private async void PickerRadius_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (!_firingRadiusSelectedIndexChanged) return;
            if (!(PickerRadius.SelectedItem is RadiusItemModel selectedRadius)) return;
            var viewModel = (MapOnlineViewModel)BindingContext;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (viewModel.Radius != selectedRadius.Radius)
            {
                viewModel.Radius = selectedRadius.Radius;
            }
            if  (viewModel.IsRunning)
            {
                App.LocationService.RefreshStatus();
                await RecenterMap();
            }
        }

        public static List<string[]> GetMapOnlineStatus()
        {
            return MapOnlineInfo.Where(x => x.Value.IsRunning).Select(
                x => new[] { x.Value.Group.Name, x.Value.RadiusDisplay }).ToList();
        }
    }
}