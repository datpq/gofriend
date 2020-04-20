using System;
using goFriend.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPageUiChat : ContentPage
    {
        private readonly TestModel _viewModel;
        public TestPageUiChat()
        {
            BindingContext = _viewModel = new TestModel();
            InitializeComponent();
            _viewModel.RefreshScrollDown = () => {
                if (_viewModel.Messages.Count > 0)
                {
                    Device.BeginInvokeOnMainThread(() => {
                        LvMessages.ScrollTo(_viewModel.Messages[_viewModel.Messages.Count - 1], ScrollToPosition.End, true);
                    });
                }
            };
        }

        public void ScrollTap(object sender, System.EventArgs e)
        {
            lock (new object())
            {
                if (BindingContext != null)
                {
                    var vm = BindingContext as ChatViewModel;
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        while (vm.DelayedMessages.Count > 0)
                        {
                            vm.Messages.Insert(0, vm.DelayedMessages.Dequeue());
                        }
                        vm.ShowScrollTap = false;
                        vm.LastMessageVisible = true;
                        vm.PendingMessageCount = 0;
                        LvMessages.ScrollToFirst();
                    });
                }
            }
        }

        private void ListView_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            ChatInput.UnFocusEntry();
        }

        private void ImgSend_OnTapped(object sender, EventArgs e)
        {
            _viewModel.SendMessageCommand.Execute(null);
        }

        private void MnuItemClose_OnClicked(object sender, EventArgs e)
        {
            //Navigation.PopModalAsync();
        }
    }
}