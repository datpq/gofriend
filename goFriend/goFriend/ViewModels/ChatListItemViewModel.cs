using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using goFriend.DataModel;
using goFriend.Services;
using Xamarin.Forms;

namespace goFriend.ViewModels
{
    public class ChatListItemViewModel : INotifyPropertyChanged
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        private Chat _chat;
        public Chat Chat
        {
            get => _chat;
            set
            {
                _chat = value;
                ChatViewModel = new ChatViewModel
                {
                    ChatListItem = this
                };
                OnPropertyChanged(nameof(Chat));
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(FormattedText));
            }
        }

        public string Name => Chat.Name;
        public string LogoUrl => Chat.LogoUrl;
        public object Tag { get; set; }

        public DateTime LastPingTime { get; set; }

        private bool _isAppearing;
        public bool IsAppearing
        {
            get => _isAppearing;
            set
            {
                //Logger.Debug($"_isAppearing={_isAppearing}, value={value}, LastMessageVisible={ChatViewModel.LastMessageVisible}");
                if (_isAppearing && !value && ChatViewModel.LastMessageVisible) //Disappearing && LastMessageVisible
                {
                    ChatViewModel.LastReadMsgIdxWhenAppearing = ChatViewModel.Messages.Any() ? ChatViewModel.Messages[0].MessageIndex : 0;
                    Logger.Debug($"Chat={ChatViewModel.Name}, LastReadMsgIdxWhenAppearing={ChatViewModel.LastReadMsgIdxWhenAppearing}");
                }
                _isAppearing = value;
                if (_isAppearing) //app go sleep and resume
                {
                    IsLastMessageRead = ChatViewModel.LastMessageVisible && IsAppearing; //when page is appearing, the last message is read
                }
            }
        }

        public async Task RefreshOnlineStatus()
        {
            if (IsAppearing)
            {
                await App.FriendStore.SignalR.ConnectAsync();
                //ping to Chat server to make the user Active
                if (LastPingTime.AddMinutes(Constants.ChatPingFrequence) < DateTime.Now)
                {
                    Logger.Debug("Last ping expired. Sending another ping...");
                    LastPingTime = DateTime.Now;
                    await App.FriendStore.SendPing(Chat.Id);
                }
            }
        }

        private bool _isLastMessageRead; 
        public bool IsLastMessageRead
        {
            get => _isLastMessageRead;
            set
            {
                _isLastMessageRead = value;
                OnPropertyChanged(nameof(IsLastMessageRead));
                OnPropertyChanged(nameof(FormattedText));
            }
        }
        private string _lastMessage;
        public string LastMessage
        {
            get => _lastMessage;
            set
            {
                _lastMessage = value;
                OnPropertyChanged(nameof(LastMessage));
                OnPropertyChanged(nameof(FormattedText));
                App.ChatListPage?.RefreshLastMessage(this);
            }
        }

        public ChatViewModel ChatViewModel { get; private set; }

        public FormattedString FormattedText =>
            new FormattedString
            {
                Spans =
                {
                    new Span
                    {
                        Text = Chat.Name, FontAttributes = FontAttributes.Bold,
                        FontSize = (double) Application.Current.Resources["LblFontSize"], LineHeight = 1.2
                    },
                    new Span { Text = Environment.NewLine },
                    new Span {Text = LastMessage.TruncateAtWord(70), LineHeight = 1.2,
                        FontSize = (double) Application.Current.Resources["LblDetailFontSize"],
                        FontAttributes = IsLastMessageRead ? FontAttributes.None : FontAttributes.Bold}
                }
            };

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
