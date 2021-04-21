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
    public partial class ChatOutgoingViewCell : ViewCell
    {
        public ChatOutgoingViewCell()
        {
            InitializeComponent();
            LongPressedEffect.SetCommand(FraMessage, new Command(() =>
            {
                var chatMessage = (ChatMessage)BindingContext;
                if (!chatMessage.IsDeleted)
                {
                    DisplayContextMenu(chatMessage);
                }
            }));
        }

        public static void DisplayContextMenu(ChatMessage chatMessage)
        {
            var menuItems = new List<string>() { res.Copy, Constants.ImgCopy };
            var menuItemActions = new List<Action>() {
                new Action(async () =>
                {
                    await Clipboard.SetTextAsync(chatMessage.Message);
                })};

            menuItems.AddRange(new[] { res.Delete, Constants.ImgDelete });
            menuItemActions.Add(
                new Action(async () =>
                {
                    if (!await App.DisplayMsgQuestion(res.MsgDeleteConfirm)) return;
                    try
                    {
                        var deletedMsg = new ChatMessage
                        {
                            IsDeleted = true,
                            ChatId = chatMessage.ChatId,
                            MessageIndex = chatMessage.MessageIndex,
                            MessageType = chatMessage.MessageType,
                            OwnerId = chatMessage.OwnerId,
                            Token = App.User.Token.ToString(),
                            LogoUrl = chatMessage.LogoUrl
                        };
                        await App.FriendStore.SendText(deletedMsg);
                    }
                    catch
                    {
                        App.DisplayMsgError(res.MsgErrConnection);
                    }
                }));
            App.DisplayContextMenu(menuItems.ToArray(), menuItemActions.ToArray());
        }
    }
}