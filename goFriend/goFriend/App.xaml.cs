using System;
using System.Collections.Generic;
using Xamarin.Forms;
using goFriend.Services;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using goFriend.DataModel;
using goFriend.Views;
using PCLAppConfig;
using Xamarin.Essentials;

namespace goFriend
{
    public partial class App : Application
    {
        public static bool UseMockDataStore = true;
        public static bool IsUserLoggedIn { get; set; }
        public static Friend User { get; set; }
        public static IFacebookManager FaceBookManager = DependencyService.Get<IFacebookManager>();
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();
        public static IFriendStore FriendStore;

        public static Task TaskGetMyGroups;
        public static IEnumerable<ApiGetGroupsModel> MyGroups;
        public static IEnumerable<ApiGetGroupsModel> AllGroups;

        public App()
        {
            try
            {
                //ConfigurationManager initialization
                var assembly = typeof(App).GetTypeInfo().Assembly;
                ConfigurationManager.Initialise(assembly.GetManifestResourceStream("goFriend.App.config"));
            }
            catch
            {
                // ignored
            }

            VersionTracking.Track();
            Logger.Info($"GoFriend {VersionTracking.CurrentVersion}({VersionTracking.CurrentBuild}) starting new instance...");
            Logger.Info($"AzureBackendUrl = {ConfigurationManager.AppSettings["AzureBackendUrl"]}");
            var deviceInfo = $"Name={DeviceInfo.Name}|Type={DeviceInfo.DeviceType}|Model={DeviceInfo.Model}|Manufacturer={DeviceInfo.Manufacturer}|Platform={DeviceInfo.Platform}|Version={DeviceInfo.Version}";
            Logger.Debug(deviceInfo);
            res.Culture = new CultureInfo("vi-VN");
            //res.Culture = new CultureInfo("");
            //Thread.CurrentThread.CurrentCulture = res.Culture;
            //Thread.CurrentThread.CurrentUICulture = res.Culture;

            AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
                var ex = (Exception)args.ExceptionObject;
                Logger.Error("UnhandledException exception");
                Logger.Error(ex.ToString());
            };

            InitializeComponent();

            DependencyService.Register<FriendStore>();
            FriendStore = DependencyService.Get<IFriendStore>();
            if (UseMockDataStore)
                DependencyService.Register<MockDataStore>();
            else
                DependencyService.Register<AzureDataStore>();

            IsUserLoggedIn = Settings.IsUserLoggedIn;
            if (IsUserLoggedIn)
            {
                User = Settings.LastUser;
            }

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
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
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

        public static void Initialize()
        {
            TaskGetMyGroups = new Task(() =>
            {
                try
                {
                    Logger.Debug("TaskGetMyGroups.BEGIN");
                    if (IsUserLoggedIn && User != null)
                    {
                        MyGroups = FriendStore.GetMyGroups().Result;
                        if (VersionTracking.IsFirstLaunchForCurrentBuild)
                        {
                            Logger.Debug("IsFirstLaunchForCurrentBuild --> Update Info");
                            FriendStore.SaveBasicInfo(new Friend {Id = User.Id, Info = Extension.GetVersionTrackingInfo()});
                        }
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
                    Logger.Debug("TaskGetMyGroups.END");
                }
            });
            TaskGetMyGroups.Start();
        }
    }
}
