﻿using System;
using System.Collections.Generic;
using Xamarin.Forms;
using goFriend.Services;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using goFriend.DataModel;
using goFriend.ViewModels;
using goFriend.Views;
using PCLAppConfig;
using Xamarin.Essentials;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Plugin.SimpleAudioPlayer;
using Device = Xamarin.Forms.Device;
using goFriend.Models;
using Point = NetTopologySuite.Geometries.Point;
using Xamarin.Forms.GoogleMaps;

namespace goFriend
{
    public partial class App : Application
    {
        public static bool UseMockDataStore = true;
        public static bool IsUserLoggedIn { get; set; }
        public static Friend User { get; set; }
        public static Position LastPosition { get; set; }
        public readonly IFacebookManager FaceBookManager;
        public static ILocationService LocationService;
        public static Dictionary<int, List<string[]>> NotificationChatInboxLinesById = new Dictionary<int, List<string[]>>();
        public static bool IsInitializing = false;
        private static ILogger Logger;
        public static IFriendStore FriendStore;
        public static IStorageService StorageService;
        public static ChatListViewModel ChatListVm; // List of all Chats
        public static ChatListPage ChatListPage = null;

        public static Task TaskInitialization;
        public static IEnumerable<ApiGetGroupsModel> MyGroups;
        public static IEnumerable<ApiGetGroupsModel> AllGroups;
        public static ISimpleAudioPlayer SapChatNewMessage = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
        public static ISimpleAudioPlayer SapChatNewChat = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();

        public App()
        {
            //NinjectManager.Wire(new ApplicationModule());
            try
            {
                //ConfigurationManager initialization
                ConfigurationManager.Initialise(Extension.GetStreamFromFile("App.config"));
            }
            catch
            {
                // ignored
            }

            FaceBookManager = DependencyService.Get<IFacebookManager>();
            LocationService = DependencyService.Get<ILocationService>();
            Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());
            ChatListVm = new ChatListViewModel();

            SapChatNewMessage.Loop = false;
            SapChatNewMessage.Load(Extension.GetStreamFromFile("Audios.chat_newmsg.wav"));
            SapChatNewChat.Loop = false;
            SapChatNewChat.Load(Extension.GetStreamFromFile("Audios.chat_newchat.wav"));

            VersionTracking.Track();
            Logger.Info($"GoFriend {VersionTracking.CurrentVersion}({VersionTracking.CurrentBuild}) starting new instance...");
            Logger.Info($"AzureBackendUrl = {Constants.AzureBackendUrlDev}");
            Logger.Debug(Extension.GetDeviceInfo());
            res.Culture = new CultureInfo("vi-VN");
            //res.Culture = new CultureInfo("");
            //Thread.CurrentThread.CurrentCulture = res.Culture;
            //Thread.CurrentThread.CurrentUICulture = res.Culture;

            AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
                var ex = (Exception)args.ExceptionObject;
                Logger.Error("UnhandledException exception");
                AppCenter.SetUserId(User?.Id.ToString());
                Logger.TrackError(ex, new Dictionary<string, string> {
                    { "FriendId", User?.Id.ToString() },
                    { "Comment", Extension.GetDeviceInfo() }
                });
            };

            InitializeComponent();

            IsUserLoggedIn = Settings.IsUserLoggedIn;
            if (IsUserLoggedIn)
            {
                User = Settings.LastUser;
            }

            DependencyService.Register<FriendStore>();
            FriendStore = DependencyService.Get<IFriendStore>();
            //StorageService = NinjectManager.Resolve<IStorageService>();
            StorageService = new StorageService(Logger, DependencyService.Get<IMediaService>());
            if (UseMockDataStore)
                DependencyService.Register<MockDataStore>();
            else
                DependencyService.Register<AzureDataStore>();

            //MainPage = new NavigationPage(new TestPageUiChat { Title = AppInfo.Name })
            //{
            //    BarBackgroundColor = (Color)Resources["ColorPrimary"]
            //};
            //return;

            Initialize();

            if (IsUserLoggedIn && User != null)
            {
                MainPage = new AppShell();
            }
            else
            {
                MainPage = new NavigationPage(new AccountPage{ Title = AppInfo.Name })
                    { BarBackgroundColor = (Color)Resources["ColorPrimary"], BarTextColor = (Color)Resources["ColorTitle"] };
            }
        }

        protected override void OnStart()
        {
            AppCenter.Start($"android={Constants.AppCenterAppSecretAndroid};ios={Constants.AppCenterAppSecretiOS}",
                  typeof(Analytics), typeof(Crashes));
        }

        protected override void OnSleep()
        {
            foreach (var chatListItemViewModel in ChatListVm.ChatListItems)
            {
                chatListItemViewModel.Tag = chatListItemViewModel.IsAppearing;
                chatListItemViewModel.IsAppearing = false;
            }
        }

        protected override async void OnResume()
        {
            foreach (var chatListItemViewModel in ChatListVm.ChatListItems)
            {
                if (chatListItemViewModel.Tag != null)
                {
                    chatListItemViewModel.IsAppearing = (bool)chatListItemViewModel.Tag;
                    await chatListItemViewModel.RefreshOnlineStatus();
                }
            }
        }

        public static void DisplayMsgInfo(string message)
        {
            Device.BeginInvokeOnMainThread(async () => {
                await Current.MainPage.DisplayAlert(res.MsgTitleInfo, message, res.Accept);
            });
            //Current.MainPage.DisplayAlert(res.MsgTitleInfo, message, res.Accept);
        }

        public static void DisplayMsgError(string message)
        {
            Device.BeginInvokeOnMainThread(async () => {
                await Current.MainPage.DisplayAlert(res.MsgTitleError, message, res.Accept);
            });
            //Current.MainPage.DisplayAlert(res.MsgTitleError, message, res.Accept);
        }

        public static async Task<string> DisplayActionSheet(params string[] buttons)
        {
            return await Current.MainPage.DisplayActionSheet(AppInfo.Name, res.Cancel, null, buttons);
        }

        public static async Task GotoAccountInfo(int groupId, int friendId)
        {
            var groupFriend = await App.FriendStore.GetGroupFriend(groupId, friendId);
            if (groupFriend == null)
            {
                Logger.Warn($"Friend {friendId} is not found in the Group {groupId}");
                return;
            }
            var groupFixedCatValues =
                await App.FriendStore.GetGroupFixedCatValues(groupId);
            var arrFixedCats = groupFixedCatValues?.GetCatList().ToList() ?? new List<string>();
            var accountBasicInfoPage = new AccountBasicInfosPage();
            await accountBasicInfoPage.Initialize(groupFriend.Group, groupFriend, arrFixedCats.Count);
            await App.Current.MainPage.Navigation.PushAsync(accountBasicInfoPage);
        }

        public static void DisplayContextMenu(string[] buttons, Action[] actions, string title = null)
        {
            var cfg = new ActionSheetConfig()
                .SetTitle(title ?? AppInfo.Name)
                .SetCancel(res.Cancel)
                //.SetDestructive(res.Cancel)
                ;
            for(var i=0; i< actions.Length; i++)
            {
                cfg.Add(buttons[2*i], actions[i], buttons[2*i+1]);
            }
            UserDialogs.Instance.ActionSheet(cfg);
        }

        public static Task<bool> DisplayMsgQuestion(string message)
        {
            var tcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                var result = await Current.MainPage.DisplayAlert(res.MsgTitleInfo, message, res.Accept, res.Cancel);
                tcs.SetResult(result);
            });
            return tcs.Task;
        }

        public static async Task RetrieveNewMessages(ChatListItemViewModel chatListItemVm)
        {
            try
            {
                Logger.Debug($"RetrieveNewMessages.BEGIN(Name={chatListItemVm.Name}, Id={chatListItemVm.Chat.Id})");
                const int startMsgIdx = 999999;
                chatListItemVm.ChatViewModel.LastReadMsgIdx = chatListItemVm.ChatViewModel.Messages.Count == 0
                    ? 0 : chatListItemVm.ChatViewModel.Messages[0].MessageIndex;
                var messages = await FriendStore.ChatGetMessages(chatListItemVm.Chat.Id,
                    startMsgIdx, chatListItemVm.ChatViewModel.LastReadMsgIdx, Constants.ChatMessagePageSize);
                messages = messages.OrderBy(x => x.MessageIndex);
                foreach (var chatMessage in messages)
                {
                    chatListItemVm.ChatViewModel.ReceiveMessage(chatMessage);
                }
                if (messages.Count() == Constants.ChatMessagePageSize) //there might be some missing messages not fetched yet
                {
                    chatListItemVm.ChatViewModel.PendingMessageCount =
                        (messages.Last().MessageIndex - chatListItemVm.ChatViewModel.LastReadMsgIdx).ToString();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                Logger.Debug("RetrieveNewMessages.END");
            }
        }

        public static async Task RetrieveAllNewMessages()
        {
            try
            {
                Logger.Debug("RetrieveAllNewMessages.BEGIN");
                foreach (var chatListItemVm in ChatListVm.ChatListItems)
                {
                    await RetrieveNewMessages(chatListItemVm);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                Logger.Debug("RetrieveAllNewMessages.END");
            }
        }

        public static void Initialize()
        {
            TaskInitialization = new Task(async () =>
            {
                try
                {
                    Logger.Debug("TaskInitialization.BEGIN");
                    IsInitializing = true;
                    if (IsUserLoggedIn && User != null)
                    {
                        MyGroups = FriendStore.GetMyGroups().Result;
                        if (VersionTracking.IsFirstLaunchForCurrentBuild)
                        {
                            Logger.Debug("IsFirstLaunchForCurrentBuild --> Update Info");
                            await FriendStore.SaveBasicInfo(new Friend {Id = User.Id, Info = Extension.GetVersionTrackingInfo()});
                        }

                        await ChatListVm.RefreshCommandAsyncExec();

                        await FriendStore.SignalR.ConnectAsync();
                        //await RetrieveAllNewMessages(); //already done in ChatListVm.RefreshCommandAsyncExec() when receiving CreateChat
                    }
                    else
                    {
                        Logger.Debug("User is null or not logged in");
                    }
                }
                catch (AggregateException e)
                {
                    e.Handle(x =>
                    {
                        if (x is GoException goe)
                        {
                            Logger.Error(goe.ToString());
                            switch (goe.Msg.Code)
                            {
                                case MessageCode.UserTokenError:
                                    //TODO: Logout by code
                                    //Logger.Debug("Wrong token. Logout");
                                    //MessagingCenter.Send(App.Current, Constants.MsgLogout);
                                    DisplayMsgError(res.MsgErrWrongToken);
                                    break;
                                default:
                                    DisplayMsgError(goe.Msg.Msg);
                                    break;
                            }
                            return true;
                        }

                        Logger.Error(x.ToString());
                        return true;
                        //return false; // Let anything else stop the application.
                    });
                }
                catch (Exception e)
                {
                    Logger.Error(e.ToString());
                }
                finally
                {
                    IsInitializing = false;
                    Logger.Debug("TaskInitialization.END");
                }
            });
            TaskInitialization.Start();
        }

        public static async Task ReceiveLocationUpdate(double latitude, double longitude)
        {
            try
            {
                Logger.Debug($"ReceiveLocationUpdate.BEGIN");
                if (IsUserLoggedIn && User != null)
                {
                    //var newPosition = await ((Point)null).GetPosition();
                    ServiceNotification result = null;
                    if (LastPosition == default)
                    {
                        LastPosition = new Position(latitude, longitude);
                        Logger.Debug($"First time getting location: Longitude={LastPosition.Longitude}, Latitude={LastPosition.Latitude}");
                        await FriendStore.SaveLocation(new FriendLocation()
                        {
                            FriendId = App.User.Id,
                            Location = new Point(LastPosition.Longitude, LastPosition.Latitude)
                        });
                        result = new ServiceNotification
                        {
                            ContentTitle = "Hanoi9194",
                            ContentText = null,
                            SummaryText = null,
                            LargeIconUrl = $"{ConfigurationManager.AppSettings["HomePageUrl"]}/logos/g12.png",
                            NotificationType = Models.NotificationType.AppearOnMap,
                            InboxLines = new List<string[]>(
                                new[] {
                                        new [] {"Bảo Anh Bảo Linh", "xuất hiện" },
                                        new [] {"Catherine Pham", "xuất hiện" },
                                        new [] {"Thang Pham", "xuất hiện" } })
                        };
                    }
                    else
                    {
                        var distance = Location.CalculateDistance(LastPosition.Latitude, LastPosition.Longitude,
                            latitude, longitude, DistanceUnits.Kilometers);
                        if (distance >= Constants.MOVING_DISTANCE_THRESHOLD)
                        {
                            LastPosition = new Position(latitude, longitude);
                            Logger.Debug($"New Location. Longitude={LastPosition.Longitude}, Latitude={LastPosition.Latitude}, distance={distance}");
                            await FriendStore.SaveLocation(new FriendLocation()
                            {
                                FriendId = App.User.Id,
                                Location = new Point(LastPosition.Longitude, LastPosition.Latitude)
                            });
                            result = new ServiceNotification
                            {
                                ContentTitle = "Hanoi9194",
                                ContentText = null,
                                SummaryText = null,
                                LargeIconUrl = $"{ConfigurationManager.AppSettings["HomePageUrl"]}/logos/g12.png",
                                NotificationType = Models.NotificationType.AppearOnMap,
                                InboxLines = new List<string[]>(
                                    new[] {
                                        new [] {"Bảo Anh Bảo Linh", "xuất hiện" },
                                        new [] {"Catherine Pham", "xuất hiện" },
                                        new [] {"Thang Pham", "xuất hiện" } })
                            };
                        }
                    }
                    if (result != null)
                    {
                        LocationService.SendNotification(result);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                Logger.Debug("ReceiveLocationUpdate.END");
            }
        }

        public static async Task DoNotificationAction(string action, int extraId)
        {
            switch (action)
            {
                case Constants.ACTION_GOTO_CHAT:
                    var chatListItemVm = App.ChatListVm.ChatListItems.SingleOrDefault(x => x.Chat.Id == extraId);
                    if (chatListItemVm != null && ChatListPage != null)
                    {
                        var trigger = new TaskCompletionSource<object>();

                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            await App.ChatListPage.Navigation.PushAsync(new ChatPage(chatListItemVm)).ConfigureAwait(false);
                            trigger.SetResult(null);
                        });

                        await trigger.Task.ConfigureAwait(false);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
