using System;
using goFriend.DataModel;
using goFriend.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatOutgoingAttachmentViewCell : ViewCell
    {
        public ChatOutgoingAttachmentViewCell()
        {
            InitializeComponent();
            LongPressedEffect.SetCommand(FraMessage, new Command(async () =>
            {
                var msg = (ChatMessage)BindingContext;
                if (!msg.IsDeleted)
                {
                    App.DisplayContextMenu(new[] { res.Delete, Constants.ImgDelete },
                        new[]
                        {
                            new Action(async () =>
                            {
                                if (!await App.DisplayMsgQuestion(res.MsgDeleteConfirm)) return;
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
                            })
                        });
                }
            }));
        }

        private void Attachments_Tapped(object sender, System.EventArgs e)
        {
            App.Current.MainPage.Navigation.PushAsync(new MediaPage(BindingContext as ChatMessage));
        }
    }
}