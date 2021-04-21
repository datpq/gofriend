using System;
using System.Collections.Generic;
using goFriend.DataModel;
using goFriend.Helpers;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatIncomingViewCell : ViewCell
    {
        public ChatIncomingViewCell()
        {
            InitializeComponent();

            LongPressedEffect.SetCommand(ImgThumbsUp, new Command(() =>
            {
                var chatMessage = (ChatMessage)BindingContext;
                DisplayContextMenu(chatMessage, false);
            }));
            LongPressedEffect.SetCommand(FraMessage, new Command(() =>
            {
                var chatMessage = (ChatMessage)BindingContext;
                DisplayContextMenu(chatMessage, true);
            }));
        }

        public static async void DisplayContextMenu(ChatMessage chatMessage, bool withCopy)
        {
            var friend = await App.FriendStore.GetFriendInfo(chatMessage.OwnerId);
            var menuItems = new List<string>() { res.BasicInfos, Constants.ImgAccountInfo };
            var menuItemActions = new List<Action>() {
                new Action(async () =>
                {
                    if (chatMessage.Chat.Members.StartsWith("g")
                        && int.TryParse(chatMessage.Chat.Members.Substring(1), out var groupId))
                    {
                        await App.GotoAccountInfo(groupId, chatMessage.OwnerId);
                    }
                })};
            if (friend.Location != null)
            {
                menuItems.AddRange(new[] { res.MapOffline, Constants.ImgMap });
                menuItemActions.Add(
                    new Action(async () =>
                    {
                        if (chatMessage.Chat.Members.StartsWith("g")
                            && int.TryParse(chatMessage.Chat.Members.Substring(1), out var groupId))
                        {
                            await App.Current.MainPage.Navigation.PushAsync(new MapPage(chatMessage.Chat.Name, chatMessage.OwnerId));
                        }
                    }));
            }
            if (withCopy)
            {
                menuItems.AddRange(new[] { res.Copy, Constants.ImgCopy });
                menuItemActions.Add(
                    new Action(async () =>
                    {
                        await Clipboard.SetTextAsync(chatMessage.Message);
                    }));
            }
            App.DisplayContextMenu(menuItems.ToArray(), menuItemActions.ToArray());
        }
    }
}