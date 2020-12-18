using goFriend.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapOnline : ContentPage
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        public MapOnline()
        {
            InitializeComponent();

            App.LocationService.CancelNotification(Models.NotificationType.AppearOnMap);

            App.LocationService.StateChanged += LocationService_StateChanged;
            App.LocationService.StateChanged += LocationService_StateChanged;
            RefreshComponentsVisibility();

            DphFriendSelection.SelectedGroupName = Settings.LastMapPageGroupName;
            DphFriendSelection.Initialize((selectedGroup, searchText, arrFixedCats, arrCatValues) =>
            {
                Settings.LastMapPageGroupName = selectedGroup.Group.Name;
                if (!Map.IsVisible) return;
            });
        }

        ~MapOnline()
        {
            App.LocationService.StateChanged -= LocationService_StateChanged;
        }

        private void LocationService_StateChanged(object sender, System.EventArgs e)
        {
            RefreshComponentsVisibility();
        }

        private void RefreshComponentsVisibility()
        {
            CmdPlay.IsEnabled = LabelLocationServiceRunning.IsVisible  = !App.LocationService.IsRunning();
            CmdPause.IsEnabled = Map.IsVisible  = App.LocationService.IsRunning();
        }

        private async void CmdPlay_Clicked(object sender, System.EventArgs e)
        {
            if (!App.LocationService.IsRunning())
            {
                await App.LocationService.Start();
            }
            RefreshComponentsVisibility();
        }

        private async void CmdPause_Clicked(object sender, System.EventArgs e)
        {
            if (App.LocationService.IsRunning())
            {
                await App.LocationService.Pause();
            }
            RefreshComponentsVisibility();
        }
    }
}