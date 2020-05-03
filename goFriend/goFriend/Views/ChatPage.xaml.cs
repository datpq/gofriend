using System;
using goFriend.Services;
using goFriend.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPage : ContentPage
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public ChatPage(ChatListItemViewModel chatListItem)
        {
            BindingContext = chatListItem.ChatViewModel;
            //chatViewModel.RefreshScrollDown = () => {
            //    if (chatViewModel.Messages.Count > 0)
            //    {
            //        Device.BeginInvokeOnMainThread(() => {
            //            LvMessages.ScrollTo(chatViewModel.Messages[chatViewModel.Messages.Count - 1], ScrollToPosition.End, true);
            //        });
            //    }
            //};

            InitializeComponent();

            Appearing += (sender, args) =>
            {
                chatListItem.IsLastMessageRead = chatListItem.IsAppearing = true;
            };
            Disappearing += (sender, args) => chatListItem.IsAppearing = false;
        }

        private void MnuItemClose_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        public void ScrollTap(object sender, System.EventArgs e)
        {
            lock (new object())
            {
                if (BindingContext != null)
                {
                    var vm = BindingContext as ChatViewModel;
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        while (vm.DelayedMessages.Count > 0)
                        {
                            vm.Messages.Insert(0, vm.DelayedMessages.Dequeue());
                        }
                        vm.ShowScrollTap = false;
                        vm.LastMessageVisible = true;
                        vm.PendingMessageCount = 0;
                        LvMessages.ScrollToFirst();
                    });
                }
            }
        }

        private void ImgSend_OnTapped(object sender, EventArgs e)
        {
            ((ChatViewModel) BindingContext).SendMessageCommand.Execute(null);
        }

        private void ListView_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            ChatInput.UnFocusEntry();

            //// don't do anything if we just de-selected the row.
            //if (e.Item == null) return;

            //// Optionally pause a bit to allow the preselect hint.
            //Task.Delay(500);

            //// Deselect the item.
            //if (sender is ListView lv) lv.SelectedItem = null;
        }
    }
}