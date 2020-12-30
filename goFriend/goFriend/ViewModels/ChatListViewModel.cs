using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using goFriend.DataModel;
using goFriend.Helpers;
using goFriend.Services;
using goFriend.Views;
using PCLAppConfig;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace goFriend.ViewModels
{
    public class ChatListViewModel : INotifyPropertyChanged
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        private ObservableCollection<ChatListItemViewModel> _chatListItems = new ObservableCollection<ChatListItemViewModel>();
        public ObservableCollection<ChatListItemViewModel> ChatListItems
        {
            get => _chatListItems;
            set
            {
                _chatListItems = value;
                OnPropertyChanged(nameof(ChatListItems));
            }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        public async Task ReceiveCreateChat(Chat chat)
        {
            Logger.Debug($"ReceiveCreateChat.BEGIN(Id={chat.Id}, GetChatType={chat.GetChatType()})");
            if (!chat.OwnerId.HasValue)
            {
                chat.OwnerId = 0;
            }
            if (chat.LogoUrl == null)
            {
                chat.LogoUrl = "/logos/group.png";
            }
            chat.LogoUrl = $"{ConfigurationManager.AppSettings["HomePageUrl"]}{chat.LogoUrl}";

            if (chat.GetChatType() == ChatType.MixedGroup || chat.GetChatType() == ChatType.Individual)
            {
                // user is kicked out from the Chat
                if (ChatListItems.Any(x => x.Chat.Id == chat.Id) && chat.GetMemberIds().All(x => x != App.User.Id))
                {
                    Logger.Debug($"User is kicked out from the chat {chat.Id}");
                    ChatListItems.Remove(ChatListItems.FirstOrDefault(x => x.Chat.Id == chat.Id));
                    App.ChatListPage?.Refresh();
                    return;
                }
            }
            // if private message to only one member --> make the member's name the chat's name
            if (chat.GetChatType() == ChatType.Individual)
            {
                foreach (var memberId in chat.GetMemberIds())
                {
                    if (memberId == App.User.Id) continue;
                    var friend = await App.FriendStore.GetFriendInfo(memberId);
                    chat.Name = friend.Name;
                    break;
                }
            }

            if (ChatListItems.Any(x => x.Chat.Id == chat.Id))
            {
                Logger.Debug($"Updating chat {chat.Name}(Id={chat.Id})");
                var item = ChatListItems.Single(x => x.Chat.Id == chat.Id);
                item.Chat.Name = chat.Name;
                item.ChatViewModel.ChatName = chat.Name;
                item.Chat.Members = chat.Members;
                //if there is any new messages following the CreateChat --> retrieve them
                await App.RetrieveNewMessages(item);
            }
            else
            {
                Logger.Debug($"Adding new chat Name={chat.Name}(Id={chat.Id})");
                var chatListItemVm = new ChatListItemViewModel { Chat = chat };
                if (chat.OwnerId != 0)
                {
                    chat.Owner = await App.FriendStore.GetFriendInfo(chat.OwnerId.Value);
                }
                ChatListItems.Add(chatListItemVm);

                var arrLastMsgIdxRetrievedByChatId = Settings.LastMsgIdxRetrievedByChatId;
                if (arrLastMsgIdxRetrievedByChatId == null)
                {
                    Logger.Debug($"Creating Settings.LastMsgIdxRetrievedByChatId");
                    arrLastMsgIdxRetrievedByChatId = new Dictionary<int, int>();
                    Settings.LastMsgIdxRetrievedByChatId = arrLastMsgIdxRetrievedByChatId;
                }
                if (!arrLastMsgIdxRetrievedByChatId.ContainsKey(chat.Id))
                {
                    arrLastMsgIdxRetrievedByChatId.Add(chat.Id, 0);
                    Settings.LastMsgIdxRetrievedByChatId = arrLastMsgIdxRetrievedByChatId;

                    App.SapChatNewChat.Play();
                    Vibration.Vibrate();
                    App.NotificationService.SendNotification(
                        new Models.ServiceNotification
                        {
                            ContentTitle = chat.Owner?.Name,
                            ContentText = $"{res.ChatNew}: {await chat.GetMemberNames()}",
                            SummaryText = null,
                            LargeIconUrl = chat.Owner?.GetImageUrl(),
                            ExtraId = chat.Id,
                            NotificationType = Models.NotificationType.ChatReceiveCreateChat,
                        });
                }

                //if user is added to an existing chat --> retrieve the history of the chat
                await App.RetrieveNewMessages(chatListItemVm);

                if (App.ChatListPage != null && !App.IsInitializing) {
                    var trigger = new TaskCompletionSource<object>();

                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await App.ChatListPage.Navigation.PushAsync(new ChatPage(chatListItemVm)).ConfigureAwait(false);
                        trigger.SetResult(null);
                    });

                    await trigger.Task.ConfigureAwait(false);
                }

                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
                _ = Task.Run(() =>
                {
                    Logger.Debug("Getting FriendInfo of all members...");
                    var memberIds = chatListItemVm.Chat.GetMemberIds();
                    var result = Task.WhenAll(memberIds.Select(x => App.FriendStore.GetFriendInfo(x)));
                    return result;
                }).ContinueWith(task => {
                    Logger.Debug("Finished getting FriendInfo of all members.");
                    foreach (var friend in task.Result)
                    {
                        chatListItemVm.ChatViewModel.Members.Add(new ChatFriendOnline
                        {
                            Friend = friend,
                            Time = DateTime.MinValue,
                            LogoUrl = friend.GetImageUrl(FacebookImageType.small)
                        });
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());

                //chatListItemVm.ChatViewModel.ReceiveMessage(
                //    new ChatMessage
                //    {
                //        OwnerId = 0,
                //        Chat = chat,
                //        ChatId = chat.Id,
                //        CreatedDate = chat.CreatedDate,
                //        Message = string.Format(res.ChatMessageCreateChat,
                //            chat.OwnerId == 0 ? res.System : chat.Owner?.FirstName, chat.CreatedDate.ToLocalTime()),
                //        MessageIndex = 0,
                //        MessageType = ChatMessageType.CreateChat
                //    });
                //var chatType = chat.GetChatType();
                //if (chatType == ChatType.Individual || chatType == ChatType.MixedGroup)
                //{
                //    chatListItemVm.ChatViewModel.ReceiveMessage(
                //        new ChatMessage
                //        {
                //            OwnerId = 0,
                //            Chat = chat,
                //            ChatId = chat.Id,
                //            CreatedDate = chat.CreatedDate,
                //            Message = await chat.GetMemberNames(),
                //            MessageIndex = 0,
                //            MessageType = ChatMessageType.CreateChat
                //        });
                //}
            }

            App.ChatListPage?.Refresh();

            Logger.Debug("ReceiveCreateChat.END");
        }

        public async Task RefreshCommandAsyncExec()
        {
            try
            {
                Logger.Debug("RefreshCommandAsyncExec.BEGIN");

                IsRefreshing = true;

                var myChats = await App.FriendStore.ChatGetChats();
                foreach (var chat in myChats)
                {
                    await ReceiveCreateChat(chat);
                }

                foreach (var item in ChatListItems.Where(x => myChats.All(y => y.Id != x.Chat.Id)).ToList())
                {
                    Logger.Warn($"Removing chat {item.Chat.Name}{item.Chat.Id})");
                    ChatListItems.Remove(item);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                IsRefreshing = false;
                Logger.Debug("RefreshCommandAsyncExec.END");
            }
        }

        //public ICommand RefreshCommand => new Command(RefreshCommandAsyncExec);
        public IAsyncCommand RefreshCommand => new AsyncCommand(RefreshCommandAsyncExec, () => !IsRefreshing);

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
