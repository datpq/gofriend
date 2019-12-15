using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using goFriend.Controls;
using goFriend.DataModel;
using goFriend.Services;
using goFriend.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;
using Point = NetTopologySuite.Geometries.Point;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AccountBasicInfosPage : ContentPage
    {
        private readonly AccountPage _accountPage;
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();
        private AccountBasicInfosViewModel _viewModel;

        public AccountBasicInfosPage(AccountPage accountPage, Friend friend)
        {
            _accountPage = accountPage;
            InitializeComponent();
            BindingContext = _viewModel = new AccountBasicInfosViewModel
            {
                Friend = friend,
                PositionDraggable = true
            };
            Initialize();
        }

        public AccountBasicInfosPage(int groupId, int otherFriendId)
        {
            InitializeComponent();
            UserDialogs.Instance.ShowLoading(res.Processing);
            Task.Run(() => App.FriendStore.GetFriend(groupId, otherFriendId)).ContinueWith(friendTask =>
            {
                UserDialogs.Instance.HideLoading();
                var otherFriend = friendTask.Result;
                BindingContext = _viewModel = new AccountBasicInfosViewModel
                {
                    Friend = otherFriend,
                    PositionDraggable = false
                };
                Initialize();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async void Initialize()
        {
            var position = await _viewModel.GetPosition();
            var pin = new DphPin
            {
                Position = position,
                Label = _viewModel.Name,
                Address = _viewModel.Email,
                IconUrl = _viewModel.ImageUrl,
                Draggable = _viewModel.PositionDraggable,
                Type = PinType.Place
            };
            //ImageService.Instance.LoadUrl(pin.IconUrl).Preload();
            Map.AllPins = new List<DphPin> { pin };
            Map.Pins.Add(pin);
            Map.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Position(pin.Position.Latitude, pin.Position.Longitude), Distance.FromKilometers(5)));
        }

        public void LoadGroupConnectionInfo(string groupName, GroupFriend groupFriend, int startCatIdx)
        {
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
                Text = groupName
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
            for (var i = startCatIdx; i < arrCats.Count; i++)
            {
                var lblCat = new Label
                {
                    Text = $"{arrCats[i]}:",
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = (double)Application.Current.Resources["LblDetailFontSize"],
                    TextColor = (Color)Application.Current.Resources["ColorLabel"]
                };
                Grid.SetColumn(lblCat, 0);
                Grid.SetRow(lblCat, i - startCatIdx);
                gr.Children.Add(lblCat);
                var lblCatVal = new Label
                {
                    Text = groupFriend.GetCatByIdx(i + 1),
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = lblCat.FontSize,
                    TextColor = lblCat.TextColor
                };
                Grid.SetColumn(lblCatVal, 1);
                Grid.SetRow(lblCatVal, i - startCatIdx);
                gr.Children.Add(lblCatVal);
            }
        }

        private async void CmdSave_Click(object sender, EventArgs e)
        {
            if (!await App.DisplayMsgQuestion(res.MsgSaveConfirm)) return;
            try
            {
                Logger.Debug("CmdSave_Click.BEGIN");
                var oldLocation = App.User.Location;
                var position = Map.AllPins.Single().Position;
                App.User.Location = new Point(position.Longitude, position.Latitude);
                Logger.Debug($"Latitude={App.User.Location.Y}, Longitude={App.User.Location.X}");
                var result = await App.FriendStore.SaveBasicInfo(App.User);
                if (result)
                {
                    Settings.LastUser = App.User;
                    _accountPage?.RefreshMenu();
                    App.DisplayMsgInfo(res.SaveSuccess);
                }
                else
                {
                    Logger.Error("Saving failed.");
                    App.User.Location = oldLocation;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
            finally
            {
                Logger.Debug("CmdSave_Click.END");
            }
        }
    }
}