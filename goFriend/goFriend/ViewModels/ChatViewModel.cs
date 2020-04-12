using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using goFriend.DataModel;
using goFriend.Services;
using Xamarin.Forms;

namespace goFriend.ViewModels
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();
        public event PropertyChangedEventHandler PropertyChanged;

        public ChatListItemViewModel ChatListItem { get; set; }
        public string ChatName => ChatListItem.Name;
        public string ChatLogoUrl => ChatListItem.LogoUrl;

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

        public Command SendMessageCommand { get; }

        public ChatViewModel()
        {
            SendMessageCommand = new Command(async () => {
                await App.FriendStore.SendMessage(new ChatMessage
                {
                    ChatId = ChatListItem.Chat.Id,
                    Message = Message,
                    MessageType = ChatMessageType.SendMessage,
                    OwnerId = App.User.Id,
                    OwnerName =  App.User.Name
                });
                Message = string.Empty;
            });
        }

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
