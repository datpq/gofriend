using System;
using System.IO;
using goFriend.ViewModels;
using Plugin.Media;
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
            ChatTextInput_OnUnfocused(null, null);
        }

        private void ImgSend_OnTapped(object sender, EventArgs e)
        {
            var chatViewModel = BindingContext as ChatViewModel;
            if (!string.IsNullOrWhiteSpace(chatViewModel?.Message))
            {
                ChatTextInput.Focus();
            }
            chatViewModel?.SendMessageCommand.Execute(null);
        }

        public void UnFocusEntry()
        {
            ChatTextInput?.Unfocus();
        }

        private async void BtnCamera_OnClicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                App.DisplayMsgError(res.MsgNoCamera);
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "Sample",
                Name = "test.jpg"
            });

            if (file == null)
                return;

            var chatViewModel = (ChatViewModel)BindingContext;
            var remoteFilePath = $"{chatViewModel.ChatListItem.Chat.Id}/tmp_{App.User.Id}{Path.GetExtension(file.Path)}";
            if (App.StorageService.Upload(file.Path, remoteFilePath, 800, 600))
            {
                chatViewModel?.SendAttachmentCommand.Invoke(remoteFilePath);
            }

            //image.Source = ImageSource.FromStream(() =>
            //{
            //    var stream = file.GetStream();
            //    return stream;
            //});
        }

        private async void BtnPhoto_OnClicked(object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                App.DisplayMsgError(res.MsgNoPhotosPermission);
                return;
            }
            var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
            });

            if (file == null)
                return;

            var chatViewModel = (ChatViewModel)BindingContext;
            var remoteFilePath = $"{chatViewModel.ChatListItem.Chat.Id}/tmp_{App.User.Id}{Path.GetExtension(file.Path)}";
            if (App.StorageService.Upload(file.Path, remoteFilePath, 800, 600))
            {
                chatViewModel?.SendAttachmentCommand.Invoke(remoteFilePath);
            }
        }

        private void ChatTextInput_OnFocused(object sender, FocusEventArgs e)
        {
            BtnCamera.IsVisible = BtnPhoto.IsVisible = false;
            BtnShowAttachments.IsVisible = true;
        }

        private void ChatTextInput_OnUnfocused(object sender, FocusEventArgs e)
        {
            BtnShowAttachments_OnClicked(null, null);
        }

        private void BtnShowAttachments_OnClicked(object sender, EventArgs e)
        {
            BtnCamera.IsVisible = BtnPhoto.IsVisible = true;
            BtnShowAttachments.IsVisible = false;
        }

        private void ChatTextInput_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(ChatTextInput.Text) && BtnShowAttachments.IsVisible)
            {
                BtnShowAttachments_OnClicked(null, null);
            } else if (!string.IsNullOrEmpty(ChatTextInput.Text) && !BtnShowAttachments.IsVisible)
            {
                ChatTextInput_OnFocused(null, null);
            }
        }
    }
}