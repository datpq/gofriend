using Xamarin.Forms;
using goFriend.Services;
using System.Globalization;
using System.Threading.Tasks;
using goFriend.DataModel;

namespace goFriend
{
    public partial class App : Application
    {
        public static string AzureBackendUrl = "http://gofriend.azurewebsites.net";
        public static bool UseMockDataStore = true;
        public static bool IsUserLoggedIn { get; set; }
        public static Friend User { get; set; }
        public static IFacebookManager FaceBookManager = DependencyService.Get<IFacebookManager>();
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();
        public static IFriendStore FriendStore;

        public App()
        {
            Logger.Info("GoFriend starting new instance...");
            res.Culture = new CultureInfo("vi-VN");
            //res.Culture = new CultureInfo("");
            //Thread.CurrentThread.CurrentCulture = res.Culture;
            //Thread.CurrentThread.CurrentUICulture = res.Culture;

            System.AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
                System.Exception ex = (System.Exception)args.ExceptionObject;
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
            var appShell = new AppShell();
            appShell.RefreshTabs();
            MainPage = appShell;
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
            Current.MainPage.DisplayAlert(res.MsgTitleInfo, message, res.Accept);
        }

        public static void DisplayMsgError(string message)
        {
            Current.MainPage.DisplayAlert(res.MsgTitleError, message, res.Accept);
        }

        public static Task<bool> DisplayMsgQuestion(string message)
        {
            return Current.MainPage.DisplayAlert(res.MsgTitleInfo, message, res.Accept, res.Cancel);
        }
    }
}
