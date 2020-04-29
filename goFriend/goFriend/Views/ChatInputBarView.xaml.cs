using System;
using goFriend.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatInputBarView : ContentView
    {
        public ChatInputBarView()
        {
            InitializeComponent();
        }

        private void ImgSend_OnTapped(object sender, EventArgs e)
        {
            (Parent.Parent.BindingContext as ChatViewModel)?.SendMessageCommand.Execute(null);
            ChatTextInput.Focus();
            //(Parent.Parent.BindingContext as ChatViewModel)?.RefreshScrollDown();
        }

        public void UnFocusEntry()
        {
            ChatTextInput?.Unfocus();
        }
    }
}