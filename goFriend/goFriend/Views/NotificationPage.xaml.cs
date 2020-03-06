using System.Linq;
using goFriend.DataModel;
using goFriend.Services;
using goFriend.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotificationPage : ContentPage
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public NotificationPage()
        {
            InitializeComponent();

            Appearing += (sender, args) => DphListView.Refresh();

            DphListView.Initialize(selectedItem =>
            {
                //Nothing happens on selection
            });
            DphListView.LoadItems(async () =>
            {
                var listViewModel = (DphListViewModel)DphListView.BindingContext;
                var arrNotifications = await App.FriendStore.GetNotifications(
                    listViewModel.PageSize, listViewModel.PageSize * listViewModel.CurrentPage);
                var result = arrNotifications?.Select(x => new DphListViewItemModel
                {
                    Id = x.Id,
                    ImageUrl = Extension.GetImageUrlByFacebookId(
                        (x.NotificationObject as GroupSubscriptionNotifBase)?.FacebookId, FacebookImageType.small), // normal 100 x 100
                    OverlappingImageUrl = Extension.GetImageUrl((x.NotificationObject as GroupSubscriptionNotifBase)?.ImageFile),
                    FormattedText = x.GetNotificationMessage(App.User.Id)
                });
                return result;
            });
        }
    }
}