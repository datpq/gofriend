using System.Linq;
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
                App.Initialize();
                var result = App.ChatListVm.ChatListItems.Select(x => new DphListViewItemModel
                {
                    Id = x.Chat.Id,
                    SelectedObject = x,
                    ImageSize = 50,
                    ImageUrl = x.LogoUrl,
                    IsHighlight = !x.IsLastMessageRead,
                    FormattedText = x.FormattedText
                });
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
    }
}