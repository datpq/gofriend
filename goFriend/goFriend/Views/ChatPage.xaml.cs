using System;
using System.Linq;
using Acr.UserDialogs;
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
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        private readonly DphOverlapImage _logo = new DphOverlapImage
        {
            HorizontalOptions = LayoutOptions.End,
            WidthRequest = 40,
            HeightRequest = 40,
            Margin = new Thickness(0, 5), //for iOS
        };

        public ChatPage(ChatListItemViewModel chatListItem)
        {
            chatListItem.ChatViewModel.CommandMembers = new Command(() => MnuMembers_OnClicked(null, null));
            chatListItem.ChatViewModel.CommandMute = new Command(() => MnuMute_OnClicked(null, null));
            chatListItem.ChatViewModel.CommandEdit = new Command(() => MnuEdit_OnClicked(null, null));
            BindingContext = chatListItem.ChatViewModel;

            InitializeComponent();

            //hide Shell tab bar for this page
            Shell.SetTabBarIsVisible(this, false);

            MnuMembers.Text = res.members.CapitalizeFirstLetter();
            //MnuMembers.IconImageSource = Constants.ImgGroup;
            MnuMute.Text = chatListItem.ChatViewModel.MuteText;
            //MnuMute.IconImageSource = chatListItem.ChatViewModel.MuteImage;

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
                    _logo,
                }
            });

            Appearing += async (sender, args) =>
            {
                chatListItem.IsLastMessageRead = chatListItem.IsAppearing = true;
                await chatListItem.RefreshOnlineStatus();
                var overlapImageInfo = await chatListItem.Chat.GetOverlapImageInfo();
                _logo.Source1 = overlapImageInfo.ImageUrl;
                _logo.Source2 = overlapImageInfo.OverlappingImageUrl;
                _logo.OverlapType = overlapImageInfo.OverlapType;
                Title = (BindingContext as ChatViewModel)?.ChatName;
            };
            Disappearing += (sender, args) => chatListItem.IsAppearing = false;
        }

        public void ScrollTapDown(object sender, System.EventArgs args)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                ((ChatViewModel)BindingContext).ShowScrollTapDown = false;
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
                var vm = (ChatViewModel)BindingContext;

                var fetchCount = 0;
                var idx = 0;
                while (idx < vm.Messages.Count)
                {
                    var message = vm.Messages[idx];
                    if (!message.MessageType.IsRealShowableMessage())
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
                    if ((previousMessage == null && message.MessageIndex > 3) || // The first 2 messages are CreateChat messages
                        (previousMessage != null && previousMessage.MessageIndex + 1 != message.MessageIndex))
                    {
                        if (fetchCount >= Constants.ChatMaxPagesFetched)
                        {
                            vm.LastReadMsgIdx = message.MessageIndex;
                            Logger.Debug($"Max number of fetching reached. Stop fetching. set LastReadMsgIdx = {vm.LastReadMsgIdx}");
                            continue;
                        }
                        Logger.Debug("There is some missing messages up the list");
                        //do not show Refreshing on iOS, that make ListView to scroll to Top unexpectedly
                        if (Device.RuntimePlatform == Device.Android) vm.IsRefreshing = true;
                        var missingMessages = await App.FriendStore.ChatGetMessages(message.ChatId,
                            message.MessageIndex,
                            previousMessage?.MessageIndex ?? 0, Constants.ChatMessagePageSize);
                        //do not show Refreshing on iOS, that make ListView to scroll to Top unexpectedly
                        if (Device.RuntimePlatform == Device.Android) vm.IsRefreshing = false;
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

        private void MnuMute_OnClicked(object sender, EventArgs e)
        {
            var vm = (ChatViewModel)BindingContext;
            if (vm.IsMute)
            {
                vm.MuteExpiryTime = null; //un-mute
                MnuMute.Text = vm.MuteText;
            }
            else
            {
                App.DisplayContextMenu(new[] {
                        res.MuteFor15Mins, Constants.ImgMute,
                        res.MuteFor1Hour, Constants.ImgMute,
                        res.MuteFor8Hours, Constants.ImgMute,
                        res.MuteFor24Hours, Constants.ImgMute,
                        res.MuteUntilTurnOn, Constants.ImgMute
                    },
                    new Action[]
                    {
                        () => {
                            vm.MuteExpiryTime = DateTime.Now.AddMinutes(15);
                            MnuMute.Text = vm.MuteText;
                        },
                        () => {
                            vm.MuteExpiryTime = DateTime.Now.AddHours(1);
                            MnuMute.Text = vm.MuteText;
                        },
                        () => {
                            vm.MuteExpiryTime = DateTime.Now.AddHours(8);
                            MnuMute.Text = vm.MuteText;
                        },
                        () => {
                            vm.MuteExpiryTime = DateTime.Now.AddHours(24);
                            MnuMute.Text = vm.MuteText;
                        },
                        () => {
                            vm.MuteExpiryTime = DateTime.Now.AddYears(1);
                            MnuMute.Text = vm.MuteText;
                        }
                    }, res.MsgMuteTitle);
            }
        }

        private void MnuMembers_OnClicked(object sender, EventArgs e)
        {
            var vm = (ChatViewModel)BindingContext;
            Navigation.PushAsync(new OnlineMembersPage(vm));
        }

        private async void MnuEdit_OnClicked(object sender, EventArgs e)
        {
            var vm = (ChatViewModel)BindingContext;
            if (vm.ChatListItem.Chat.OwnerId == App.User.Id)
            {
                UserDialogs.Instance.ShowLoading(res.Processing);
                if (vm.ChatListItem.Chat.GetChatType() == ChatType.StandardGroup)
                {
                    var friends = await GroupEdit.GetFriends(vm.ChatListItem.Chat.GetMemberGroupId());
                    await Navigation.PushAsync(new GroupEdit(vm.ChatListItem.Chat.GetMemberGroupId(), friends));
                }
                else
                {
                    var friends = await ChatEdit.GetFriends(vm.ChatListItem.Chat.GetMemberIds());
                    await Navigation.PushAsync(new ChatEdit(vm, friends));
                }
                UserDialogs.Instance.HideLoading();
            }
            else
            {
                App.DisplayMsgError(res.MsgNoPermissionChatEdit);
            }
        }
    }
}