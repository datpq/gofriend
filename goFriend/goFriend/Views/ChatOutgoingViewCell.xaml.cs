using System;
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
            LongPressedEffect.SetCommand(FraMessage, new Command(async () =>
            {
                //if (res.Copy == await App.DisplayActionSheet(res.Copy))
                //{
                //    await Clipboard.SetTextAsync((BindingContext as ChatMessage)?.Message);
                //};
                var msg = (ChatMessage)BindingContext;
                if (msg.IsDeleted)
                {
                    App.DisplayContextMenu(new[] { res.Copy, Constants.ImgCopy },
                        new[]
                        {
                            new Action(async () =>
                            {
                                await Clipboard.SetTextAsync(msg?.Message);
                            })
                        });
                }
                else
                {
                    App.DisplayContextMenu(new[] { res.Copy, Constants.ImgCopy, res.Delete, Constants.ImgDelete },
                        new[]
                        {
                            new Action(async () =>
                            {
                                await Clipboard.SetTextAsync(msg?.Message);
                            }),
                            async () =>
                            {
                                if (!await App.DisplayMsgQuestion(res.MsgDeleteConfirm)) return;
                                try
                                {
                                    var chatMessage = new ChatMessage
                                    {
                                        IsDeleted = true,
                                        ChatId = msg.ChatId,
                                        MessageIndex = msg.MessageIndex,
                                        MessageType = msg.MessageType,
                                        OwnerId = msg.OwnerId,
                                        Token = App.User.Token.ToString(),
                                        LogoUrl = msg.LogoUrl
                                    };
                                    await App.FriendStore.SendText(chatMessage);
                                }
                                catch
                                {
                                    App.DisplayMsgError(res.MsgErrConnection);
                                }
                            }
                        });
                }
            }));
        }
    }
}