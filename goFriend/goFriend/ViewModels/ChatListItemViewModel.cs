using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using goFriend.DataModel;
using goFriend.Services;
using Xamarin.Forms;

namespace goFriend.ViewModels
{
    public class ChatListItemViewModel : INotifyPropertyChanged
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        private Chat _chat;
        public Chat Chat
        {
            get => _chat;
            set
            {
                _chat = value;
                if (ChatViewModel == null)
                {
                    ChatViewModel = new ChatViewModel
                    {
                        ChatListItem = this
                    };
                }
                OnPropertyChanged(nameof(Chat));
                OnPropertyChanged(nameof(FormattedText));
            }
        }

        public string Name => Chat.Name;
        public string LogoUrl => Chat.LogoUrl;

        public bool IsAppearing { get; set; }
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
                    new Span {Text = LastMessage, LineHeight = 1.2,
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
