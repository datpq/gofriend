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
                App.DisplayContextMenu(new[] { res.Copy, Constants.ImgCopy/*, res.Delete, Constants.ImgDelete */},
                    new[]
                    {
                        new Action(async () =>
                        {
                            await Clipboard.SetTextAsync((BindingContext as ChatMessage)?.Message);
                        })
                    });
            }));
        }
    }
}