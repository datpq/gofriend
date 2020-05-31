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
                App.DisplayContextMenu(new[] {res.Copy, Constants.ImgCopy},
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