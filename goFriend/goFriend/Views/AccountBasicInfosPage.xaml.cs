using System;
using System.Linq;
using Acr.UserDialogs;
using goFriend.Controls;
using goFriend.DataModel;
using goFriend.Services;
using goFriend.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
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

        public AccountBasicInfosPage()
        {
            InitializeComponent();
        }

        public async void Initialize(AccountPage accountPage, Friend friend)
        {
            _accountPage = accountPage;
            BindingContext = _viewModel = new AccountBasicInfosViewModel
            {
                Friend = friend,
                PositionDraggable = true
            };
            CmdSave.IsEnabled = _viewModel.Friend.Location == null;
            CmdReset.IsEnabled = false;
            CmdSetGps.IsEnabled = true;

            MessagingCenter.Subscribe<Application, Pin>(this,
                Constants.MsgLocationChanged, (obj, pin) => CmdSave.IsEnabled = CmdReset.IsEnabled = true);

            //set up Pins
            var position = await _viewModel.Friend.Location.GetPosition();
            _pin = new DphPin
            {
                Position = position,
                Title = _viewModel.Name,
                SubTitle1 = _viewModel.Address,
                SubTitle2 = _viewModel.CountryName,
                IconUrl = _viewModel.ImageUrl,
                Draggable = _viewModel.PositionDraggable,
                Type = PinType.Place
            };
            //ImageService.Instance.LoadUrl(pin.IconUrl).Preload();
            Map.Pins.Clear();
            Map.Pins.Add(_pin);
            Map.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Position(_pin.Position.Latitude, _pin.Position.Longitude), Distance.FromKilometers(DphMap.DefaultDistance)));
        }

        public async void Initialize(Group group, GroupFriend groupFriend, int fixedCatsCount)
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
                PositionDraggable = false
            };

            //Load connection info section
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
                Draggable = _viewModel.PositionDraggable,
                Type = PinType.Place
            };
            //ImageService.Instance.LoadUrl(pin.IconUrl).Preload();
            Map.Pins.Clear();
            Map.Pins.Add(_pin);
            Map.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Position(_pin.Position.Latitude, _pin.Position.Longitude), Distance.FromKilometers(DphMap.DefaultDistance)));
        }

        private async void CmdSetGps_Click(object sender, EventArgs e)
        {
            try
            {
                UserDialogs.Instance.ShowLoading(res.Processing);
                var request = new GeolocationRequest(GeolocationAccuracy.High);
                var location = await Geolocation.GetLocationAsync(request);

                if (location == null) return;
                _pin.Position = new Position(location.Latitude, location.Longitude);
                Map.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Position(_pin.Position.Latitude, _pin.Position.Longitude), Distance.FromKilometers(DphMap.DefaultDistance)));
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
                new Position(_pin.Position.Latitude, _pin.Position.Longitude), Distance.FromKilometers(DphMap.DefaultDistance)));
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
                var pin = (DphPin)Map.Pins.Single();
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
                    pin.SubTitle1 = App.User.Address;
                    pin.SubTitle2 = App.User.CountryName;
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
    }
}