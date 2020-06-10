using System;
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
            LongPressedEffect.SetCommand(FraMessage, new Command(() =>
            {
                //if (res.Copy == await App.DisplayContextMenu(res.Copy))
                //{
                //    await Clipboard.SetTextAsync((BindingContext as ChatMessage)?.Message);
                //};
                var chatMessage = (ChatMessage) BindingContext;
                App.DisplayContextMenu(new[] { res.BasicInfos, Constants.ImgAccountInfo, res.Copy, Constants.ImgCopy},
                    new[]
                    {
                        async () =>
                        {
                            if (chatMessage.Chat.Members.StartsWith("g")
                                && int.TryParse(chatMessage.Chat.Members.Substring(1), out var groupId))
                            {
                                await App.GotoAccountInfo(groupId, chatMessage.OwnerId);
                            }
                        },
                        new Action(async () =>
                        {
                            await Clipboard.SetTextAsync(chatMessage.Message);
                        }),
                    });
            }));
        }
    }
}