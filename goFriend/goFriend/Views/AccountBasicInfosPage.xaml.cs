using System;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using goFriend.Controls;
using goFriend.DataModel;
using goFriend.Services;
using goFriend.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;
using Point = NetTopologySuite.Geometries.Point;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AccountBasicInfosPage : ContentPage
    {
        private AccountPage _accountPage;
        private DphPin _pin;
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();
        private AccountBasicInfosViewModel _viewModel;
        private bool _isMoveToRegionDone;

        public AccountBasicInfosPage()
        {
            InitializeComponent();

            Map.UiSettings.MyLocationButtonEnabled = true;
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
                            SwitchShowLocation.IsToggled = false;
                            SwitchShowLocation_OnToggled(null, null);
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

        public async Task Initialize(AccountPage accountPage, Friend friend)
        {
            _accountPage = accountPage;
            BindingContext = _viewModel = new AccountBasicInfosViewModel
            {
                Friend = friend,
                Editable = true
            };
            var setting = await App.FriendStore.GetSetting();
            if (setting == null) return;
            LabelShowLocation.IsVisible = SwitchShowLocation.IsVisible = setting.LocationSwitch;

            CmdSave.IsEnabled = _viewModel.Friend.Location == null;
            CmdReset.IsEnabled = false;
            CmdSetGps.IsEnabled = true;

            MessagingCenter.Subscribe<Application, DphPin>(Application.Current,
                Constants.MsgLocationChanged, (sender, dphPin) => CmdSave.IsEnabled = CmdReset.IsEnabled = true);

            //set up Pins
            var position = await _viewModel.Friend.Location.GetPosition();
            _pin = new DphPin
            {
                Position = position,
                Title = _viewModel.Name,
                SubTitle1 = _viewModel.Address,
                SubTitle2 = _viewModel.CountryName,
                IconUrl = _viewModel.ImageUrl,
                UserRight = UserType.Normal,
                //Url = $"facebook://facebook.com/info?user={_viewModel.Friend.FacebookId}",
                IsDraggable = _viewModel.Editable,
                Type = PinType.Place
            };

            PostInitialize();
        }

        public async Task Initialize(Group group, GroupFriend groupFriend, int fixedCatsCount)
        {
            UserDialogs.Instance.ShowLoading(res.Processing);
            var otherFriend = await App.FriendStore.GetFriend(group.Id, groupFriend.FriendId);
            UserDialogs.Instance.HideLoading();

            BindingContext = _viewModel = new AccountBasicInfosViewModel
            {
                Group = group,
                GroupFriend = groupFriend,
                FixedCatsCount = fixedCatsCount,
                Friend = otherFriend,
                Editable = false
            };
            LabelShowLocation.IsVisible = SwitchShowLocation.IsVisible = false;

            //Load connection info section
            GroupConnectionSection.Children.Clear();
            GroupConnectionSection.Children.Add(new BoxView
            {
                HorizontalOptions = LayoutOptions.Fill,
                HeightRequest = 1,
                Color = Color.LightGray
            });
            GroupConnectionSection.Children.Add(new Label
            {
                VerticalOptions = LayoutOptions.Center,
                FontSize = (double)Application.Current.Resources["LblFontSize"],
                TextColor = (Color)Application.Current.Resources["ColorLabel"],
                Text = _viewModel.Group.Name
            });
            var gr = new Grid
            {
                ColumnSpacing = 30,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };
            GroupConnectionSection.Children.Add(gr);
            var selectedGroup = App.MyGroups.FirstOrDefault(x => x.Group.Id == groupFriend.GroupId);
            if (selectedGroup == null) return;
            var arrCats = selectedGroup.Group.GetCatDescList().ToList();
            for (var i = _viewModel.FixedCatsCount; i < arrCats.Count; i++)
            {
                var lblCat = new Label
                {
                    Text = $"{arrCats[i]}:",
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = (double)Application.Current.Resources["LblDetailFontSize"],
                    TextColor = (Color)Application.Current.Resources["ColorLabel"]
                };
                Grid.SetColumn(lblCat, 0);
                Grid.SetRow(lblCat, i - _viewModel.FixedCatsCount);
                gr.Children.Add(lblCat);
                var lblCatVal = new Label
                {
                    Text = groupFriend.GetCatByIdx(i + 1),
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = lblCat.FontSize,
                    TextColor = lblCat.TextColor
                };
                Grid.SetColumn(lblCatVal, 1);
                Grid.SetRow(lblCatVal, i - _viewModel.FixedCatsCount);
                gr.Children.Add(lblCatVal);
            }

            //set up Pins
            var position = await _viewModel.Friend.Location.GetPosition(false);
            _pin = new DphPin
            {
                Position = position,
                Title = _viewModel.Name,
                SubTitle1 = $"{res.Groups} {_viewModel.Group.Name}",
                SubTitle2 =  _viewModel.GroupFriend.GetCatValueDisplay(_viewModel.FixedCatsCount),
                IconUrl = _viewModel.ImageUrl,
                UserRight = Constants.SuperUserIds.Contains(_viewModel.GroupFriend.FriendId) ? UserType.Normal : _viewModel.GroupFriend.UserRight,
                //Url = $"facebook://facebook.com/info?user={_viewModel.Friend.FacebookId}",
                IsDraggable = _viewModel.Editable,
                Type = PinType.Place
            };

            PostInitialize();
        }

        private void PostInitialize()
        {
            //ImageService.Instance.LoadUrl(pin.IconUrl).Preload();
            _pin.Pin.Tag = _pin;
            Map.Pins.Add(_pin.Pin);
            SwitchShowLocation_OnToggled(null, null);
        }

        private async void CmdSetGps_Click(object sender, EventArgs e)
        {
            try
            {
                UserDialogs.Instance.ShowLoading(res.Processing);
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(Constants.GeolocationRequestTimeout));
                var location = await Geolocation.GetLocationAsync(request);

                if (location == null) return;
                _pin.Position = new Position(location.Latitude, location.Longitude);
                Map.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Position(_pin.Position.Latitude, _pin.Position.Longitude), Distance.FromKilometers(MapExtension.DefaultDistance)));
                CmdReset.IsEnabled = CmdSave.IsEnabled = true;
            }
            catch (Exception)
            {
                App.DisplayMsgError(res.MsgGpsDisabledWarning);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        private async void CmdReset_Click(object sender, EventArgs e)
        {
            _pin.Position = await _viewModel.Friend.Location.GetPosition();
            Map.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Position(_pin.Position.Latitude, _pin.Position.Longitude), Distance.FromKilometers(MapExtension.DefaultDistance)));
            CmdReset.IsEnabled = false;
            CmdSave.IsEnabled = _viewModel.Friend.Location == null;
        }

        private async void CmdSave_Click(object sender, EventArgs e)
        {
            if (!await App.DisplayMsgQuestion(res.MsgSaveConfirm)) return;
            try
            {
                UserDialogs.Instance.ShowLoading(res.Processing);
                Logger.Debug("CmdSave_Click.BEGIN");
                var oldLocation = App.User.Location;
                var oldAddress = App.User.Address;
                var oldCountryName = App.User.CountryName;
                var pin = Map.Pins.Single();
                var dphPin = pin.Tag as DphPin;
                var position = pin.Position;
                var geoCoder = new Geocoder();
                var approximateLocations = await geoCoder.GetAddressesForPositionAsync(position);
                var placeMarks = await Geocoding.GetPlacemarksAsync(position.Latitude, position.Longitude);
                var placeMark = placeMarks?.FirstOrDefault();
                App.User.Location = new Point(position.Longitude, position.Latitude);
                App.User.Address = approximateLocations.FirstOrDefault();
                App.User.CountryName = placeMark?.CountryName;
                Logger.Debug($"Latitude={App.User.Location.Y}, Longitude={App.User.Location.X}, Address={App.User.Address}");

                var result = await App.FriendStore.SaveBasicInfo(App.User);
                if (result)
                {
                    CmdSave.IsEnabled = CmdReset.IsEnabled = false;
                    Settings.LastUser = App.User;
                    dphPin.SubTitle1 = App.User.Address;
                    dphPin.SubTitle2 = App.User.CountryName;
                    _accountPage?.RefreshMenu();
                    App.DisplayMsgInfo(res.SaveSuccess);
                }
                else
                {
                    Logger.Error("Saving failed.");
                    App.User.Location = oldLocation;
                    App.User.Address = oldAddress;
                    App.User.CountryName = oldCountryName;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
                Logger.Debug("CmdSave_Click.END");
            }
        }

        private async void SwitchShowLocation_OnToggled(object sender, ToggledEventArgs e)
        {
            if (e != null && SwitchShowLocation.IsToggled != App.User.ShowLocation)
            {
                if (SwitchShowLocation.IsToggled) //Check if Location is accessible
                {
                    try
                    {
                        UserDialogs.Instance.ShowLoading(res.Processing);
                        if (!await Extension.CheckIfLocationIsGranted())
                        {
                            SwitchShowLocation.IsToggled = false;
                            App.DisplayMsgInfo(res.MsgGpsDisabledWarning);
                            return;
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
                Logger.Debug($"Update ShowLocation({SwitchShowLocation.IsToggled})");
                var result = await App.FriendStore.SaveBasicInfo(new Friend { Id = App.User.Id, ShowLocation = SwitchShowLocation.IsToggled });
                if (!result) return;
                App.User.ShowLocation = SwitchShowLocation.IsToggled;
                Settings.LastUser = App.User;
            }
            Map.IsVisible = CmdSetGps.IsVisible = CmdReset.IsVisible = CmdSave.IsVisible = SwitchShowLocation.IsToggled;
            LabelNoLocation.IsVisible = !Map.IsVisible && (App.User.Location == null || App.User.ShowLocation != true);
            if (e != null && SwitchShowLocation.IsToggled && App.User.Location == null && Map.IsVisible)
            {
                App.DisplayMsgInfo(res.MsgNoLocationSuggestion);
            }
            if (Map.IsVisible && !_isMoveToRegionDone && _pin != null)
            {
                _isMoveToRegionDone = true;
                Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
                {
                    Map.MoveToRegion(MapSpan.FromCenterAndRadius(
                        new Position(_pin.Position.Latitude, _pin.Position.Longitude), Distance.FromKilometers(MapExtension.DefaultDistance)));
                    return false;
                });
            }
        }
    }
}