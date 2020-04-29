using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using goFriend.DataModel;
using goFriend.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Xamarin.Forms;

namespace goFriend.ViewModels
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        private bool _showScrollTab = false;
        public bool ShowScrollTap
        {
            get => _showScrollTab;
            set
            {
                _showScrollTab = value;
                OnPropertyChanged(nameof(ShowScrollTap));
            }
        }//Show the jump icon
        private bool _lastMessageVisible = true;
        public bool LastMessageVisible
        {
            get => _lastMessageVisible;
            set
            {
                _lastMessageVisible = value;
                OnPropertyChanged(nameof(LastMessageVisible));
            }
        }
        private int _pendingMessageCount = 0;
        public int PendingMessageCount
        {
            get => _pendingMessageCount;
            set
            {
                _pendingMessageCount = value;
                OnPropertyChanged(nameof(PendingMessageCount));
                OnPropertyChanged(nameof(PendingMessageCountVisible));
            }
        }
        public bool PendingMessageCountVisible => PendingMessageCount > 0;
        public Queue<ChatMessage> DelayedMessages { get; set; } = new Queue<ChatMessage>();
        public ICommand MessageAppearingCommand { get; set; }
        public ICommand MessageDisappearingCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public System.Action RefreshScrollDown { get; set; }

        private ChatListItemViewModel _chatListItem;
        public ChatListItemViewModel ChatListItem
        {
            get => _chatListItem;
            set
            {
                _chatListItem = value;
                OnPropertyChanged(nameof(ChatListItem));

                ChatName = _chatListItem.Name;
                ChatLogoUrl = _chatListItem.LogoUrl;
            }
        }

        private string _chatName;
        public string ChatName
        {
            get => _chatName;
            set
            {
                _chatName = value;
                OnPropertyChanged(nameof(ChatName));
            }
        }

        private string _chatLogoUrl;
        public string ChatLogoUrl
        {
            get => _chatLogoUrl;
            set
            {
                _chatLogoUrl = value;
                OnPropertyChanged(nameof(ChatLogoUrl));
            }
        }

        public bool IsEnabled => App.FriendStore.ChatHubConnection.State == HubConnectionState.Connected;
        public string Name => App.User.Name;
        public string LogoUrl => App.User.GetImageUrl(FacebookImageType.small);

        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        private ObservableCollection<ChatMessage> _messages = new ObservableCollection<ChatMessage>();
        public ObservableCollection<ChatMessage> Messages
        {
            get => _messages;
            set
            {
                _messages = value;
                OnPropertyChanged(nameof(Messages));
            }
        }

        public Command SendMessageCommand { get; set; }

        public ChatViewModel()
        {
            SendMessageCommand = new Command(async () => {
                if (string.IsNullOrEmpty(Message)) return;
                if (!IsEnabled)
                {
                    App.DisplayMsgError(res.MsgErrConnection);
                    return;
                }
                await App.FriendStore.SendMessage(new ChatMessage
                {
                    ChatId = ChatListItem.Chat.Id,
                    Message = Message,
                    MessageType = ChatMessageType.SendMessage,
                    OwnerId = App.User.Id,
                    LogoUrl = App.User.GetImageUrl(),
                    OwnerName = App.User.Name
                });
                Message = string.Empty;
            });
            MessageAppearingCommand = new Command<ChatMessage>(OnMessageAppearing);
            MessageDisappearingCommand = new Command<ChatMessage>(OnMessageDisappearing);
        }

        public void ReceiveMessage(ChatMessage chatMessage)
        {
            Messages.Insert(0, chatMessage);
        }

        void OnMessageAppearing(ChatMessage message)
        {
            var idx = Messages.IndexOf(message);
            if (idx <= 6)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    while (DelayedMessages.Count > 0)
                    {
                        Messages.Insert(0, DelayedMessages.Dequeue());
                    }
                    ShowScrollTap = false;
                    LastMessageVisible = true;
                    PendingMessageCount = 0;
                });
            }
        }

        void OnMessageDisappearing(ChatMessage message)
        {
            var idx = Messages.IndexOf(message);
            if (idx >= 6)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ShowScrollTap = true;
                    LastMessageVisible = false;
                });
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
