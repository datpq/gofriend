﻿using System;
using System.Linq;
using Acr.UserDialogs;
using goFriend.DataModel;
using goFriend.Services;
using goFriend.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AccountPage : ContentPage
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public AccountPage()
        {
            InitializeComponent();
            Tv.Margin = DeviceInfo.Platform == DevicePlatform.iOS ?
                DeviceInfo.Version <= new Version(10, 3, 4) ?
                    new Thickness(0, -62, 0, 0) : new Thickness(0, -30, 0, 0)
                : new Thickness(0, 0, 0, 0);
            Logger.Debug($"Platform={DeviceInfo.Platform}, Version={DeviceInfo.Version}, Margin.Top={Tv.Margin.Top}");

            RefreshMenu();

            BindingContext = new AccountViewModel();

            CellBasicInfo.Tapped += CellBasicInfo_Tapped;
            CellGroups.Tapped += (s, e) => { Navigation.PushAsync(new GroupConnectionPage()); };
            CellAdmin.Tapped += (s, e) => { Navigation.PushAsync(new AdminPage()); };
            CellLogin.Tapped += (s, e) =>
            {
                Navigation.PushAsync(new LoginPage());
            };
            CellLogout.Tapped += async (s, e) =>
            {
                if (!await App.DisplayMsgQuestion(res.MsgLogoutConfirm)) return;
                Logout();
            };
            CellAbout.Tapped += (s, e) => { Navigation.PushAsync(new AboutPage()); };
            //MessagingCenter.Subscribe<Application>(this, Constants.MsgLogout, obj => Logout());
        }

        private async void CellBasicInfo_Tapped(object sender, EventArgs e)
        {
            var page = new AccountBasicInfosPage();
            await page.Initialize(this, App.User);
            await Navigation.PushAsync(page);
        }

        private void Logout()
        {
            Logger.Debug("Logout.BEGIN");
            App.FaceBookManager.Logout();
            App.IsUserLoggedIn = false;
            App.User = null;
            Settings.IsUserLoggedIn = App.IsUserLoggedIn;
            Application.Current.MainPage = new NavigationPage(new AccountPage{ Title = AppInfo.Name})
                { BarBackgroundColor = (Color)Application.Current.Resources["ColorPrimary"], BarTextColor = (Color)Application.Current.Resources["ColorTitle"] };
            Logger.Debug("Logout.END");
        }

        public async void RefreshMenu()
        {
            TsShells.Clear();
            if (App.IsUserLoggedIn && App.User != null)
            {
                try
                {
                    UserDialogs.Instance.ShowLoading(res.Processing);
                    TsShells.Add(CellAvatar);
                    if (App.User.Active)
                    {
                        TsShells.Add(CellBasicInfo);
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

                    //if (App.User.Active && App.User.Location != null)
                    //{
                        TsShells.Add(CellGroups);
                    //}

                    TsShells.Add(CellLogout);
                    TsShells.Add(CellAbout);
                    ImgAvatar.Source = App.User.GetImageUrl(); // normal 100 x 100
                    //ImgAvatar.Source = Extension.GetImageSourceFromFile("admin.png"); // normal 100 x 100
                    LblFullName.Text = App.User.Name;
                    LblMemberSince.Text = string.Format(res.MemberSince, App.User.CreatedDate?.ToShortDateString());
                    if (App.User.Active)
                    {
                        //if (App.User.Location != null)
                        //{
                            await App.TaskInitialization;
                            if (App.MyGroups != null &&
                                App.MyGroups.Any(x => x.GroupFriend.UserRight >= UserType.Admin))
                            {
                                TsShells.Insert(TsShells.IndexOf(CellLogout), CellAdmin);
                            }
                        //}
                        //else
                        //{
                        if (App.User.Location == null && App.User.ShowLocation == true)
                        {
                            //App.DisplayMsgInfo(res.MsgNoLocationSuggestion);
                            CellBasicInfo_Tapped(null, null);
                        }
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
            else
            {
                TsShells.Add(CellLogin);
                TsShells.Add(CellAbout);
            }
        }
    }
}