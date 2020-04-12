using System;
using goFriend.DataModel;
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
            if (!App.MapChatViewModels.ContainsKey(chatListItem.Chat.Id))
            {
                Logger.Debug($"Joined chat: {chatListItem.Chat.Name}");
                App.MapChatViewModels.Add(chatListItem.Chat.Id, new ChatViewModel
                {
                    ChatListItem = chatListItem
                });
            }
            BindingContext = App.MapChatViewModels[chatListItem.Chat.Id];

            InitializeComponent();
        }

        private void MnuItemClose_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            ((ChatViewModel) BindingContext).SendMessageCommand.Execute(null);
        }
    }
}