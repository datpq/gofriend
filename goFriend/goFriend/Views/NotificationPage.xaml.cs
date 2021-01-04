using System;
using System.Collections.Generic;
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
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());
        private List<Notification> _allNotifications = new List<Notification>();

        public NotificationPage()
        {
            InitializeComponent();

            Appearing += (sender, args) => DphListView.Refresh();

            DphListView.Initialize(async selectedItem =>
            {
                try
                {
                    Logger.Debug("NotificationClicked.BEGIN");
                    await App.TaskInitialization;
                    var selectedNotification = _allNotifications.Single(x => x.Id == selectedItem.Id);
                    switch (selectedNotification.Type)
                    {
                        case NotificationType.NewSubscriptionRequest:
                        case NotificationType.UpdateSubscriptionRequest:
                        case NotificationType.SubscriptionApproved:
                        case NotificationType.SubscriptionRejected:
                            var subscription = (GroupSubscriptionNotifBase) selectedNotification.NotificationObject;
                            await App.GotoAccountInfo(subscription.GroupId, subscription.FriendId);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
                finally
                {
                    Logger.Debug("NotificationClicked.END");
                }
            });
            DphListView.LoadItems(async () =>
            {
                var listViewModel = (DphListViewModel)DphListView.BindingContext;
                var arrNotifications = await App.FriendStore.GetNotifications(
                    listViewModel.PageSize, listViewModel.PageSize * listViewModel.CurrentPage);
                if (listViewModel.CurrentPage == 0)
                {
                    _allNotifications.Clear();
                }
                _allNotifications.AddRange(arrNotifications);
                var result = arrNotifications?.Select(x => new DphListViewItemModel
                {
                    Id = x.Id,
                    ImageUrl = DataModel.Extension.GetImageUrlByFacebookId(
                        (x.NotificationObject as GroupSubscriptionNotifBase)?.FacebookId, FacebookImageType.small), // normal 100 x 100
                    //OverlappingImageUrl = DataModel.Extension.GetImageUrl((x.NotificationObject as GroupSubscriptionNotifBase)?.ImageFile),
                    OverlappingImageUrl = (x.NotificationObject as GroupSubscriptionNotifBase).GetOverlapImageFromSubscription(),
                    FormattedText = x.GetNotificationMessage(App.User.Id)
                });
                return result;
            });
        }
    }
}