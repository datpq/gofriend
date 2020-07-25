using System.Linq;
using System.Threading.Tasks;
using goFriend.Controls;
using goFriend.DataModel;
using goFriend.Services;
using goFriend.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatListPage : ContentPage
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public ChatListPage()
        {
            InitializeComponent();
            BindingContext = App.ChatListVm;
            App.ChatListPage = this;

            Task.Run(() =>
            {
                App.TaskInitialization.Wait();
                //Task.Delay(5000).Wait();
            }).ContinueWith(task =>
            {
                if (!App.MyGroups.Any())
                {
                    App.DisplayMsgInfo(res.MsgNoGroupWarning);
                    Navigation.PushAsync(new GroupConnectionPage());
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());

            Appearing += (sender, args) =>
            {
                DphListView.Refresh(true);
            };

            DphListView.Initialize(async (selectedItem) =>
            {
                await Navigation.PushAsync(new ChatPage((ChatListItemViewModel)selectedItem.SelectedObject));
            });
            DphListView.LoadItems(async () =>
            {
                var result = await Task.WhenAll(App.ChatListVm.ChatListItems.Select(async x => {
                    var item = new DphListViewItemModel
                    {
                        Id = x.Chat.Id,
                        SelectedObject = x,
                        IsHighlight = !x.IsLastMessageRead,
                        FormattedText = x.FormattedText
                    };
                    var overlapImageInfo = await x.Chat.GetOverlapImageInfo();
                    item.ImageUrl = overlapImageInfo.ImageUrl;
                    item.OverlappingImageUrl = overlapImageInfo.OverlappingImageUrl;
                    item.OverlapType = overlapImageInfo.OverlapType;
                    return item;
                }));
                return result;
            });
        }

        public void RefreshLastMessage(ChatListItemViewModel chatListItemVm)
        {
            //Logger.Debug($"RefreshLastMessage.BEGIN(ChatId={chatListItemVm.Chat.Id})");
            var item = ((DphListViewModel)DphListView.BindingContext).DphListItems.SingleOrDefault(x => x.Id == chatListItemVm.Chat.Id);
            if (item != null) {
                item.IsHighlight = !chatListItemVm.IsLastMessageRead;
                item.FormattedText = chatListItemVm.FormattedText;
                //Logger.Debug($"IsHighlight={item.IsHighlight}, LastMessage={chatListItemVm.LastMessage}, FormattedText={item.FormattedText}");
            }
            //Logger.Debug($"RefreshLastMessage.END");
        }

        private void MnuAddNew_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new ChatEdit());
        }
    }
}