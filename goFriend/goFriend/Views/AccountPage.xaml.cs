using System;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using goFriend.DataModel;
using goFriend.Services;
using goFriend.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.LatestVersion;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AccountPage : ContentPage
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());
        private DateTime _lastOnAppearing = DateTime.MinValue;
        private bool _isInitializing = false;

        public AccountPage()
        {
            InitializeComponent();
            _isInitializing = true;
            Tv.Margin = DeviceInfo.Platform == DevicePlatform.iOS ?
                DeviceInfo.Version <= new Version(10, 3, 4) ?
                    new Thickness(0, -62, 0, 0) : new Thickness(0, -30, 0, 0)
                : new Thickness(0, 0, 0, 0);
            Logger.Debug($"Platform={DeviceInfo.Platform}, Version={DeviceInfo.Version}, Margin.Top={Tv.Margin.Top}");

            BindingContext = new AccountViewModel();

            CellBasicInfo.Tapped += CellBasicInfo_Tapped;
            CellGroups.Tapped += (s, e) => {
                //Shell.Current.GoToAsync(Constants.ROUTE_HOME_GROUPCONNECTION);
                Navigation.PushAsync(new GroupConnectionPage());
            };
            CellMap.Tapped += (s, e) => {
                //Shell.Current.GoToAsync(Constants.ROUTE_HOME_MAP);
                Navigation.PushAsync(new MapPage());
            };
            CellAdmin.Tapped += (s, e) => {
                //Shell.Current.GoToAsync(Constants.ROUTE_HOME_ADMIN);
                Navigation.PushAsync(new AdminPage());
            };
            CellLogin.Tapped += async (s, e) =>
            {
                if (Device.RuntimePlatform != Device.iOS)
                {
                    await Navigation.PushAsync(new LoginPage());
                    return;
                }
                try
                {
                    UserDialogs.Instance.ShowLoading(res.Processing);
                    //Shell.Current.GoToAsync(Constants.ROUTE_HOME_LOGIN); //ERROR Shell.Current is null
                    var appleSignInConfig = await App.FriendStore.GetConfiguration("AppleSignInButtonVisible");
                    bool.TryParse(appleSignInConfig, out Constants.AppleSignInButtonVisible);
                    Logger.Debug($"AppleSignInButtonVisible={Constants.AppleSignInButtonVisible}");
                    await Navigation.PushAsync(new LoginPage());
                }
                finally
                {
                    UserDialogs.Instance.HideLoading();
                }
            };
            CellLogout.Tapped += async (s, e) =>
            {
                if (!await App.DisplayMsgQuestion(res.MsgLogoutConfirm)) return;
                await Logout();
            };
            CellAbout.Tapped += (s, e) => {
                //Shell.Current.GoToAsync(Constants.ROUTE_HOME_ABOUT);
                Navigation.PushAsync(new AboutPage());
            };
            //MessagingCenter.Subscribe<Application>(this, Constants.MsgLogout, obj => Logout());
        }

        private async void CellBasicInfo_Tapped(object sender, EventArgs e)
        {
            var page = new AccountBasicInfosPage();
            await page.Initialize(this, App.User);
            await Navigation.PushAsync(page);
        }

        private async Task Logout()
        {
            Logger.Debug("Logout.BEGIN");
            ((App) Application.Current).FaceBookManager.Logout();
            await App.FriendStore.SignalR.StopAsync();
            App.LocationService.Stop();
            App.NotificationService.CancelNotification();
            App.IsUserLoggedIn = false;
            App.User = null;
            App.MyGroups = null;
            Settings.IsUserLoggedIn = App.IsUserLoggedIn;
            App.ChatListVm.ChatListItems.Clear();
            MapOnlinePage.Instance?.Reset();
            Application.Current.MainPage = new NavigationPage(new AccountPage{ Title = AppInfo.Name})
                { BarBackgroundColor = (Color)Application.Current.Resources["ColorPrimary"], BarTextColor = (Color)Application.Current.Resources["ColorTitle"] };
            Logger.Debug("Logout.END");
        }

        protected override async void OnAppearing()
        {
            Extension.SendLogFile();

            if (_isInitializing)
            {
                //continue from Constructor
                UserDialogs.Instance.ShowLoading(res.Processing);
                ClearMenus();
                await App.TaskInitialization;
                Logger.Debug("ConstructorRefresh.BEGIN");
                UserDialogs.Instance.HideLoading();
                RefreshMenu();
                Logger.Debug("ConstructorRefresh.END");
                _isInitializing = false;
                return;
            }
            if (DateTime.Now < _lastOnAppearing.AddMinutes(Constants.AccountOnAppearingTimeout)) return;
            _lastOnAppearing = DateTime.Now;
            //If user logged in, but does not belong to any group
            if (App.IsUserLoggedIn && App.User != null && App.User.Active
                && (App.MyGroups == null || App.MyGroups.All(x => !x.GroupFriend.Active))) {
                Logger.Debug("OnAppearing.BEGIN");
                UserDialogs.Instance.ShowLoading(res.Processing);
                await App.RefreshMyGroups();
                UserDialogs.Instance.HideLoading();
                RefreshMenu();
                Logger.Debug("OnAppearing.END");
            }

            if (!await CrossLatestVersion.Current.IsUsingLatestVersion())
            {
                if (await App.DisplayMsgQuestion(res.MsgNewVersionAvailable))
                {
                    await CrossLatestVersion.Current.OpenAppInStore();
                }
            }
        }

        private void ClearMenus()
        {
            TsShells.Clear();
            if (App.IsUserLoggedIn && App.User != null)
            {
                TsShells.Add(CellLogout);
            }
            else
            {
                TsShells.Add(CellLogin);
            }
            TsShells.Add(CellAbout);
            (Shell.Current as AppShell)?.RefreshTabs();
        }

        public async void RefreshMenu()
        {
            Logger.Debug("RefreshMenu.BEGIN");
            ClearMenus();
            if (App.IsUserLoggedIn && App.User != null)
            {
                try
                {
                    UserDialogs.Instance.ShowLoading(res.Processing);
                    TsShells.Insert(TsShells.IndexOf(CellLogout), CellAvatar);
                    if (App.User.Active)
                    {
                        TsShells.Insert(TsShells.IndexOf(CellLogout), CellBasicInfo);
                    }

                    try
                    {
                        // Location may be lost when stored in local setting. Try to get from server
                        if (App.User.Active && App.User.Location == null)
                        {
                            var myProfile = await App.FriendStore.GetProfile();
                            //App.User.Location = myProfile.Location;
                            var backupToken = App.User.Token;
                            App.User = myProfile;
                            App.User.Token = backupToken;
                            Settings.LastUser = App.User;
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    TsShells.Insert(TsShells.IndexOf(CellLogout), CellGroups);

                    ImgAvatar.Source = App.User.GetImageUrl(); // normal 100 x 100
                    Logger.Debug($"ImgAvatar.Source = {ImgAvatar.Source}");
                    //ImgAvatar.Source = Extension.GetImageSourceFromFile("admin.png"); // normal 100 x 100
                    LblFullName.Text = App.User.Name;
                    LblMemberSince.Text = string.Format(res.MemberSince, App.User.CreatedDate?.ToShortDateString());
                    if (App.User.Active)
                    {
                        await App.TaskInitialization;
                        if (App.MyGroups != null && App.MyGroups.Any(x => x.GroupFriend.Active))
                        {
                            TsShells.Insert(TsShells.IndexOf(CellLogout), CellMap);
                        }
                        if (App.MyGroups != null && App.MyGroups.Any(x => x.Group.Public && x.GroupFriend.UserRight >= UserType.Admin))
                        {
                            TsShells.Insert(TsShells.IndexOf(CellLogout), CellAdmin);
                        }
                        //if (App.MyGroups != null && App.MyGroups.All(x => !x.GroupFriend.Active)) {
                        //    App.DisplayMsgInfo(res.MsgNoGroupWarning);
                        //    //await Shell.Current.GoToAsync(Constants.ROUTE_HOME_GROUPCONNECTION);
                        //    await Navigation.PushAsync(new GroupConnectionPage());
                        //}
                        //else if (App.User.Location == null && App.User.ShowLocation == true)
                        //{
                        //    //App.DisplayMsgInfo(res.MsgNoLocationSuggestion);
                        //    CellBasicInfo_Tapped(null, null);
                        //}
                        (Shell.Current as AppShell)?.RefreshTabs();
                    }
                    else
                    {
                        App.DisplayMsgInfo(res.MsgInactiveUserWarning);
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
            Logger.Debug("RefreshMenu.END");
        }
    }
}