﻿using goFriend.DataModel;
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
        }

        private void Attachments_Tapped(object sender, System.EventArgs e)
        {
            App.Current.MainPage.Navigation.PushAsync(new MediaPage(BindingContext as ChatMessage));
        }
    }
}