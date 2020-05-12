using System;
using System.Linq;
using goFriend.Controls;
using goFriend.DataModel;
using goFriend.Services;
using goFriend.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPage : ContentPage
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public ChatPage(ChatListItemViewModel chatListItem)
        {
            BindingContext = chatListItem.ChatViewModel;

            InitializeComponent();

            //hide Shell tab bar for this page
            Shell.SetTabBarIsVisible(this, false);

            //NavigationPage.TitleView in XAML not working, so the code below is for this purpose.
            Shell.SetTitleView(this, new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.Center,
                Children = {
                    new Label
                    {
                        HorizontalOptions = LayoutOptions.StartAndExpand,
                        Text = chatListItem.ChatViewModel.ChatName,
                        TextColor = (Color)Application.Current.Resources["ColorTitle"],
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        VerticalTextAlignment = TextAlignment.Center
                    },
                    new DphOverlapImage
                    {
                        HorizontalOptions = LayoutOptions.End,
                        WidthRequest = HeightRequest = 40,
                        Margin = 5,
                        Source1 = chatListItem.ChatViewModel.ChatLogoUrl
                    }
                }
            });

            Appearing += (sender, args) =>
            {
                chatListItem.IsLastMessageRead = chatListItem.IsAppearing = true;
            };
            Disappearing += (sender, args) => chatListItem.IsAppearing = false;
        }

        private void MnuItemClose_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        public void ScrollTapDown(object sender, System.EventArgs args)
        {
            if (!(BindingContext is ChatViewModel vm))
            {
                Logger.Error("Error when casting BindingContext");
                return;
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                vm.ShowScrollTapDown = false;
                LvMessages.ScrollToFirst();
            });
        }

        public async void ScrollTapUp(object sender, System.EventArgs args)
        {
            //lock (new object())
            //{
            //}
            try
            {
                Logger.Debug("ScrollTap.BEGIN");
                if (!(BindingContext is ChatViewModel vm))
                {
                    Logger.Error("Error when casting BindingContext");
                    return;
                }

                var fetchCount = 0;
                var idx = 0;
                while (idx < vm.Messages.Count)
                {
                    var message = vm.Messages[idx];
                    if (message.MessageType != ChatMessageType.Text)
                    {
                        idx++;
                        continue;
                    }

                    if (message.MessageIndex <= vm.LastReadMsgIdx ||
                        (message.MessageIndex == 1 && vm.LastReadMsgIdx == 0))
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            //while (vm.DelayedMessages.Count > 0)
                            //{
                            //    vm.Messages.Insert(0, vm.DelayedMessages.Dequeue());
                            //}
                            vm.ShowScrollTapUp = false;
                            vm.LastMessageVisible = false;
                            Logger.Debug($"Set LastMessageVisible={vm.LastMessageVisible}");
                            vm.PendingMessageCount = 0.ToString();
                            LvMessages.ScrollToLast(message);
                        });
                        break;
                    }

                    //go up the list find the previous message
                    var previousMessage = vm.GetPreviousMessage(idx);
                    if ((previousMessage == null && message.MessageIndex != 1) ||
                        (previousMessage != null && previousMessage.MessageIndex + 1 != message.MessageIndex))
                    {
                        if (fetchCount >= Constants.ChatMaxPagesFetched)
                        {
                            vm.LastReadMsgIdx = message.MessageIndex;
                            Logger.Debug($"Max number of fetching reached. Stop fetching. set LastReadMsgIdx = {vm.LastReadMsgIdx}");
                            continue;
                        }
                        Logger.Debug("There is some missing messages up the list");
                        vm.IsRefreshing = true;
                        var missingMessages = await App.FriendStore.ChatGetMessages(message.ChatId,
                            message.MessageIndex,
                            previousMessage?.MessageIndex ?? 0, Constants.ChatMessagePageSize);
                        vm.IsRefreshing = false;
                        foreach (var msg in missingMessages.OrderByDescending(x => x.MessageIndex))
                        {
                            vm.ReceiveMessage(msg);
                        }
                        fetchCount++;
                    }
                    idx++;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                Logger.Debug("ScrollTap.END");
            }
        }

        private void ImgSend_OnTapped(object sender, EventArgs e)
        {
            ((ChatViewModel) BindingContext).SendMessageCommand.Execute(null);
        }

        private void ListView_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            ChatInput.UnFocusEntry();

            //// don't do anything if we just de-selected the row.
            //if (e.Item == null) return;

            //// Optionally pause a bit to allow the preselect hint.
            //Task.Delay(500);

            //// Deselect the item.
            //if (sender is ListView lv) lv.SelectedItem = null;
        }
    }
}