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
                if (res.Copy == await App.DisplayContextMenu(res.Copy))
                {
                    await Clipboard.SetTextAsync((BindingContext as ChatMessage)?.Message);
                };
            }));
        }
    }
}