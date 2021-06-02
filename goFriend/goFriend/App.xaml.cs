using System;
using System.Collections.Generic;
using Xamarin.Forms;
using goFriend.Services;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using goFriend.DataModel;
using goFriend.ViewModels;
using goFriend.Views;
using Xamarin.Essentials;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Plugin.SimpleAudioPlayer;
using Device = Xamarin.Forms.Device;
using Point = NetTopologySuite.Geometries.Point;
using Xamarin.Forms.Internals;

namespace goFriend
{
    public partial class App : Application
    {
        public static bool UseMockDataStore = true;
        public static bool IsUserLoggedIn { get; set; }
        public static IEnumerable<MyGroupViewModel> MyGroups;
        public static Friend User { get; set; }
        public readonly IFacebookManager FaceBookManager;
        public static ILocationService LocationService;
        public static INotificationService NotificationService;
        public static Dictionary<int, List<string[]>> NotificationChatInboxLinesById = new Dictionary<int, List<string[]>>();
        public static bool IsInitializing;
        private static ILogger _logger;
        public static IFriendStore FriendStore;
        public static IStorageService StorageService;
        public static ChatListViewModel ChatListVm; // List of all Chats

        public static Task TaskInitialization;
        public static ISimpleAudioPlayer SapChatNewMessage = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
        public static ISimpleAudioPlayer SapChatNewChat = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();

        public App()
        {
            //NinjectManager.Wire(new ApplicationModule());
            var device = DependencyService.Get<IDevice>();
            Constants.DeviceId = device.GetIdentifier();
            FaceBookManager = DependencyService.Get<IFacebookManager>();
            LocationService = DependencyService.Get<ILocationService>();
            NotificationService = DependencyService.Get<INotificationService>();
            _logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());
            ChatListVm = new ChatListViewModel();

            SapChatNewMessage.Loop = false;
            SapChatNewMessage.Load(Extension.GetStreamFromFile("Audios.chat_newmsg.wav"));
            SapChatNewChat.Loop = false;
            SapChatNewChat.Load(Extension.GetStreamFromFile("Audios.chat_newchat.wav"));

            VersionTracking.Track();
            _logger.Info($"GoFriend {VersionTracking.CurrentVersion}({VersionTracking.CurrentBuild}) starting new instance...");
            _logger.Info($"DeviceId = {Constants.DeviceId}");
            _logger.Info($"BackendUrl = {Constants.BackendUrl}");
            _logger.Debug(Extension.GetDeviceInfo());
            //Thread.CurrentThread.CurrentCulture = res.Culture;
            //Thread.CurrentThread.CurrentUICulture = res.Culture;

            AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
                var ex = (Exception)args.ExceptionObject;
                _logger.Error("UnhandledException exception");
                AppCenter.SetUserId(User?.Id.ToString());
                _logger.TrackError(ex, new Dictionary<string, string> {
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

            DependencyService.Register<CacheService>();
            DependencyService.Register<FriendStore>();
            FriendStore = DependencyService.Get<IFriendStore>();
            //StorageService = NinjectManager.Resolve<IStorageService>();
            StorageService = new StorageService(_logger, DependencyService.Get<IMediaService>());
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
                _logger.Warn($"Friend {friendId} is not found in the Group {groupId}");
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
                _logger.Debug($"RetrieveNewMessages.BEGIN(Name={chatListItemVm.Name}, Id={chatListItemVm.Chat.Id})");
                const int startMsgIdx = 999999;
                chatListItemVm.ChatViewModel.LastReadMsgIdx = chatListItemVm.ChatViewModel.Messages.Count == 0
                    ? 0 : chatListItemVm.ChatViewModel.Messages[0].MessageIndex;
                var messages = await FriendStore.ChatGetMessages(chatListItemVm.Chat.Id,
                    startMsgIdx, chatListItemVm.ChatViewModel.LastReadMsgIdx, Constants.ChatMessagePageSize);
                messages = messages.OrderBy(x => x.MessageIndex);
                messages.ForEach(x => chatListItemVm.ChatViewModel.ReceiveMessage(x));
                if (messages.Count() == Constants.ChatMessagePageSize) //there might be some missing messages not fetched yet
                {
                    chatListItemVm.ChatViewModel.PendingMessageCount =
                        (messages.Last().MessageIndex - chatListItemVm.ChatViewModel.LastReadMsgIdx).ToString();
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
            }
            finally
            {
                _logger.Debug("RetrieveNewMessages.END");
            }
        }

        public static async Task RetrieveAllNewMessages()
        {
            try
            {
                _logger.Debug("RetrieveAllNewMessages.BEGIN");
                foreach (var chatListItemVm in ChatListVm.ChatListItems)
                {
                    await RetrieveNewMessages(chatListItemVm);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
            }
            finally
            {
                _logger.Debug("RetrieveAllNewMessages.END");
            }
        }

        public static void Initialize()
        {
            if (IsInitializing) return;
            //use Task.Run, never use new Task()
            TaskInitialization = Task.Run(async () =>
            {
                try
                {
                    _logger.Debug("TaskInitialization.BEGIN");
                    IsInitializing = true;
                    if (IsUserLoggedIn && User != null)
                    {
                        await Constants.InitializeConfiguration();
                        var newMyGroups = await FriendStore.GetMyGroups();
                        newMyGroups = newMyGroups.Where(x => x.GroupFriend.Active).OrderBy(
                            x => x.ChatOwnerId.HasValue && x.ChatOwnerId.Value == App.User.Id ? 0 : x.ChatOwnerId.HasValue ? 1 : 2)
                        .ThenBy(x => x.Group.Name).ToList();

                        if (VersionTracking.IsFirstLaunchForCurrentBuild)
                        {
                            _logger.Debug("IsFirstLaunchForCurrentBuild --> Update Info");
                            await FriendStore.SaveBasicInfo(new Friend {Id = User.Id, Info = Extension.GetVersionTrackingInfo()});
                        }

                        if ((MyGroups == null || MyGroups.All(x => !x.GroupFriend.Active))
                        && newMyGroups.Any(x => x.GroupFriend.Active)) {
                            _logger.Debug("First active group approved or first time connecting to Chat");
                            MyGroups = newMyGroups;
                            await ChatListVm.RefreshCommandAsyncExec();

                            try
                            {
                                await FriendStore.SignalR.ConnectAsync();
                            }
                            catch { }
                            //await RetrieveAllNewMessages(); //already done in ChatListVm.RefreshCommandAsyncExec() when receiving CreateChat
                        }
                        //new Active group aproved
                        else if (MyGroups != null && newMyGroups.Any(x => x.GroupFriend.Active
                        && !MyGroups.Any(y => y.GroupFriend.Active && y.Group.Id == x.Group.Id)))
                        {
                            _logger.Debug("New active group approved.");
                            MyGroups = newMyGroups;
                            await ChatListVm.RefreshCommandAsyncExec();

                            _ = FriendStore.SignalR.SendMessageAsync<string>(ChatMessageType.JoinChat.ToString());
                        }
                        else
                        {
                            MyGroups = newMyGroups;
                        }
                    }
                    else
                    {
                        _logger.Debug("User is null or not logged in");
                    }
                }
                catch (AggregateException e)
                {
                    e.Handle(x =>
                    {
                        if (x is GoException goe)
                        {
                            _logger.Error(goe.ToString());
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

                        _logger.Error(x.ToString());
                        return true;
                        //return false; // Let anything else stop the application.
                    });
                }
                catch (Exception e)
                {
                    _logger.Error(e.ToString());
                }
                finally
                {
                    IsInitializing = false;
                    _logger.Debug("TaskInitialization.END");
                }
            });
        }

        public static async Task ReceiveLocationUpdate(double latitude, double longitude)
        {
            try
            {
                //_logger.Debug($"ReceiveLocationUpdate.BEGIN");
                if (IsUserLoggedIn && User != null)
                {
                    //var newPosition = await ((Point)null).GetPosition();
                    if (MapOnlinePage.MyLocation == null || MapOnlinePage.MyLocation.IsRefreshNeeded()
                        || MapOnlinePage.MyLocation.SharingInfo != MapOnlinePage.GetSharingInfo())
                    {
                        _logger.Debug($"Current location={MapOnlinePage.MyLocation?.Location}. Sending new location...");
                        await FriendStore.SendLocation(latitude, longitude);
                    }
                    else
                    {
                        var distance = Location.CalculateDistance(MapOnlinePage.MyLocation.Location.Y,
                            MapOnlinePage.MyLocation.Location.X, latitude, longitude, DistanceUnits.Kilometers);
                        if (distance >= Constants.LOCATIONSERVICE_DISTANCE_THRESHOLD / 1000)
                        {
                            _logger.Debug($"New Location. Longitude={longitude}, Latitude={latitude}, distance={distance}");
                            await FriendStore.SendLocation(latitude, longitude);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
            }
            finally
            {
                //_logger.Debug("ReceiveLocationUpdate.END");
            }
        }

        public static async Task DoNotificationAction(string action, int extraId)
        {
            switch (action)
            {
                case Constants.ACTION_GOTO_CHAT:
                    var chatListItemVm = App.ChatListVm.ChatListItems.SingleOrDefault(x => x.Chat.Id == extraId);
                    if (chatListItemVm != null && ChatListPage.Instance != null)
                    {
                        var trigger = new TaskCompletionSource<object>();

                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            await ChatListPage.Instance.Navigation.PushAsync(new ChatPage(chatListItemVm)).ConfigureAwait(false);
                            trigger.SetResult(null);
                        });

                        await trigger.Task.ConfigureAwait(false);
                    }
                    break;
            }
        }
    }
}
