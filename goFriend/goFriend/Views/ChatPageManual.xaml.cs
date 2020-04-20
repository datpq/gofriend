using System;
using System.Threading.Tasks;
using goFriend.Services;
using goFriend.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPageManual : ContentPage
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public ChatPageManual(ChatListItemViewModel chatListItem)
        {
            if (!App.MapChatViewModels.ContainsKey(chatListItem.Chat.Id))
            {
                Logger.Debug($"Joined chat: {chatListItem.Chat.Name}");
                App.MapChatViewModels.Add(chatListItem.Chat.Id, new ChatViewModel
                {
                    ChatListItem = chatListItem
                });
            }
            var chatViewModel = App.MapChatViewModels[chatListItem.Chat.Id];
            BindingContext = chatViewModel;
            chatViewModel.RefreshScrollDown = () => {
                if (chatViewModel.Messages.Count > 0)
                {
                    Device.BeginInvokeOnMainThread(() => {
                        LvMessages.ScrollTo(chatViewModel.Messages[chatViewModel.Messages.Count - 1], ScrollToPosition.End, true);
                    });
                }
            };

            InitializeComponent();
        }

        private void MnuItemClose_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void ImgSend_OnTapped(object sender, EventArgs e)
        {
            ((ChatViewModel) BindingContext).SendMessageCommand.Execute(null);
        }

        private void ListView_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            // don't do anything if we just de-selected the row.
            if (e.Item == null) return;

            // Optionally pause a bit to allow the preselect hint.
            Task.Delay(500);

            // Deselect the item.
            if (sender is ListView lv) lv.SelectedItem = null;
        }
    }
}