using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using goFriend.DataModel;
using goFriend.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace goFriend.ViewModels
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

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
        private bool _showScrollTabUp = false;
        public bool ShowScrollTapUp
        {
            get => _showScrollTabUp;
            set
            {
                _showScrollTabUp = value;
                OnPropertyChanged(nameof(ShowScrollTapUp));
            }
        }//Show the jump up icon
        private bool _showScrollTabDown = false;
        public bool ShowScrollTapDown
        {
            get => _showScrollTabDown;
            set
            {
                _showScrollTabDown = value;
                OnPropertyChanged(nameof(ShowScrollTapDown));
            }
        }//Show the jump down icon

        public int LastReadMsgIdxWhenAppearing { get; set; }
        public int LastReadMsgIdx { get; set; }
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
        public string PendingMessageCount
        {
            get => _pendingMessageCount > Constants.ChatMaxPendingMsg ? $"+{Constants.ChatMaxPendingMsg}" : _pendingMessageCount.ToString();
            set
            {
                _pendingMessageCount = int.Parse(value);
                OnPropertyChanged(nameof(PendingMessageCount));
                ShowScrollTapUp = _pendingMessageCount > Constants.ChatMinPendingMsg;
            }
        }
        public ICommand MessageAppearingCommand { get; set; }
        public ICommand MessageDisappearingCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

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

        public IList<ChatFriendOnline> Members { get; } = new List<ChatFriendOnline>();
        public void UpdateMembers(IEnumerable<ChatFriendOnline> members)
        {
            Logger.Debug($"UpdateMembers.BEGIN(Count={members.Count()})");
            members.ForEach(x =>
            {
                x.Time = x.Time.ToLocalTime();
                var friendOnline = Members.SingleOrDefault(y => y.Friend.Id == x.Friend.Id);
                if (friendOnline != null)
                {
                    friendOnline.Time = x.Time;
                }
                else
                {
                    Members.Add(x);
                    Logger.Debug($"new user online. {x.Friend.Name}");
                }
            });
            Logger.Debug($"UpdateMembers.END");
        }

        public bool IsMute => _muteExpiryTime.HasValue && _muteExpiryTime.Value > DateTime.Now;
        public ImageSource MuteImage => IsMute ? Constants.ImgMute : Constants.ImgUnMute;
        public string MuteText=> IsMute ? res.Unmute : res.Mute;
        private DateTime? _muteExpiryTime;
        public DateTime? MuteExpiryTime
        {
            get => _muteExpiryTime;
            set
            {
                _muteExpiryTime = value;
                OnPropertyChanged(nameof(MuteExpiryTime));
                OnPropertyChanged(nameof(IsMute));
                OnPropertyChanged(nameof(MuteImage));
                OnPropertyChanged(nameof(MuteText));
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
                OnPropertyChanged(nameof(SendImage));
            }
        }

        public ImageSource SendImage => string.IsNullOrWhiteSpace(Message) ? Constants.ImgThumbsUp : Constants.ImgSend;

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
        public Action<string> SendAttachmentCommand { get; set; }

        public ChatViewModel()
        {
            SendMessageCommand = new Command(async () => {
                if (App.FriendStore.ChatHubConnection.State == HubConnectionState.Disconnected)
                {
                    await App.JoinChats();
                }
                if (!IsEnabled)
                {
                    App.DisplayMsgError(res.MsgErrConnection);
                    return;
                }
                var chatMessage = new ChatMessage
                {
                    ChatId = ChatListItem.Chat.Id,
                    Message = string.IsNullOrWhiteSpace(Message) ? null : Message.Trim(),
                    MessageType = ChatMessageType.Text,
                    OwnerId = App.User.Id,
                    Token = App.User.Token.ToString(),
                    LogoUrl = App.User.GetImageUrl()
                };
                await App.FriendStore.SendText(chatMessage);
                Message = string.Empty;
            });
            SendAttachmentCommand = (async uploadedFilePath =>
            {
                if (App.FriendStore.ChatHubConnection.State == HubConnectionState.Disconnected)
                {
                    await App.JoinChats();
                }
                if (!IsEnabled)
                {
                    App.DisplayMsgError(res.MsgErrConnection);
                    return;
                }
                var chatMessage = new ChatMessage
                {
                    ChatId = ChatListItem.Chat.Id,
                    Message = string.IsNullOrWhiteSpace(Message) ? null : Message.Trim(),
                    MessageType = ChatMessageType.Attachment,
                    Attachments = uploadedFilePath,
                    OwnerId = App.User.Id,
                    Token = App.User.Token.ToString(),
                    LogoUrl = App.User.GetImageUrl()
                };
                await App.FriendStore.SendAttachment(chatMessage);
                Message = string.Empty;
            });
            MessageAppearingCommand = new Command<ChatMessage>(OnMessageAppearing);
            MessageDisappearingCommand = new Command<ChatMessage>(OnMessageDisappearing);
        }

        public void ReceiveMessage(ChatMessage chatMessage)
        {
            try
            {
                Logger.Debug($"ReceiveMessage.BEGIN(ChatId={chatMessage.ChatId}, MessageIndex={chatMessage.MessageIndex})");
                chatMessage.CreatedDate = chatMessage.CreatedDate.ToLocalTime();
                var lastDateTime = new DateTime(2000, 1, 1);
                var arrIdx = 0;
                for (arrIdx = 0; arrIdx < Messages.Count; arrIdx++)
                {
                    if (Messages[arrIdx].MessageIndex >= chatMessage.MessageIndex) continue;
                    lastDateTime = Messages[arrIdx].CreatedDate.Date;
                    break;
                }

                if (lastDateTime < chatMessage.CreatedDate.Date)
                {
                    // if the SysDate message on the same day exist already, just change the MessageIndex
                    if (arrIdx > 0 && Messages[arrIdx - 1].MessageType == ChatMessageType.SysDate
                                   && Messages[arrIdx - 1].CreatedDate.Date == chatMessage.CreatedDate.Date)
                    {
                        arrIdx--;
                        Messages[arrIdx].MessageIndex = chatMessage.MessageIndex;
                    }
                    else
                    {
                        Logger.Debug($"SysDate message added for {chatMessage.CreatedDate.Date:yyyy MMMM dd}");
                        Messages.Insert(arrIdx, new ChatMessage
                        {
                            CreatedDate = chatMessage.CreatedDate.Date,
                            Message = chatMessage.CreatedDate.Date.ToString("dddd, d MMMM", res.Culture),
                            OwnerId = 0,
                            MessageIndex = chatMessage.MessageIndex,
                            MessageType = ChatMessageType.SysDate
                        });
                    }
                }

                chatMessage.IsOwnMessage = chatMessage.OwnerId == App.User.Id;
                Messages.Insert(arrIdx, chatMessage);
                if (arrIdx == 0 && ChatListItem != null)
                {
                    ChatListItem.IsLastMessageRead = LastMessageVisible && ChatListItem.IsAppearing; //when page is appearing, the last message is read
                    var msg = chatMessage.MessageType == ChatMessageType.Text ? (chatMessage.IsThumbsUp ? "👍" : chatMessage.Message)
                        : chatMessage.MessageType  == ChatMessageType.Attachment ? res.LastMessageIsImage : null;
                    ChatListItem.LastMessage = $"{(chatMessage.IsOwnMessage ? res.You : chatMessage.OwnerFirstName)}: {msg}";
                    if (!ChatListItem.IsAppearing && chatMessage.MessageIndex >= LastReadMsgIdxWhenAppearing + Constants.ChatMinPendingMsg)
                    {
                        LastReadMsgIdx = LastReadMsgIdxWhenAppearing;
                        PendingMessageCount = (chatMessage.MessageIndex - LastReadMsgIdx).ToString();
                    }
                }

                if (ChatListItem != null && !ChatListItem.IsAppearing && !chatMessage.IsOwnMessage && !IsMute)
                {
                    Vibration.Vibrate();
                }
                Logger.Debug($"Message {chatMessage.Id}, ChatId={chatMessage.ChatId}, MessageIndex={chatMessage.MessageIndex} received. Added at {arrIdx}.");
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                Logger.Debug($"ReceiveMessage.END");
            }
        }

        public ChatMessage GetPreviousMessage(int msgListIdx)
        {
            ChatMessage result = null;
            var idx = msgListIdx + 1;
            while (idx < Messages.Count)
            {
                if (Messages[idx].MessageType.IsShowableMessage())
                {
                    result = Messages[idx];
                    //Logger.Debug($"GetPreviousMessage.result(msgListIdx={idx}, MessageIndex={result.MessageIndex})");
                    break;
                }
                idx++;
            }
            return result;
        }

        public ChatMessage GetNextMessage(int msgListIdx)
        {
            ChatMessage result = null;
            var idx = msgListIdx - 1;
            while (idx >= 0)
            {
                if (Messages[idx].MessageType.IsShowableMessage())
                {
                    result = Messages[idx];
                    //Logger.Debug($"GetNextMessage.result(msgListIdx={idx}, MessageIndex={result.MessageIndex})");
                    break;
                }
                idx--;
            }
            return result;
        }

        async void OnMessageAppearing(ChatMessage message)
        {
            if (!message.MessageType.IsShowableMessage()) return;
            try
            {
                var listIdx = Messages.IndexOf(message);
                Logger.Debug($"OnMessageAppearing.BEGIN(listIdx={listIdx}, MessageIndex={message.MessageIndex})");

                //go up the list find the previous message
                var previousMessage = GetPreviousMessage(listIdx);
                if ((previousMessage == null && message.MessageIndex != 1) ||
                    (previousMessage != null && previousMessage.MessageIndex + 1 != message.MessageIndex))
                {
                    Logger.Debug("There is some missing messages up the list");
                    IsRefreshing = true;
                    var missingMessages = await App.FriendStore.ChatGetMessages(message.ChatId, message.MessageIndex,
                        previousMessage?.MessageIndex ?? 0, Constants.ChatMessagePageSize);
                    IsRefreshing = false;
                    foreach (var msg in missingMessages.OrderByDescending(x => x.MessageIndex))
                    {
                        ReceiveMessage(msg);
                    }
                }

                //go down the list find the next message
                var nextMessage = GetNextMessage(listIdx); ;
                if (nextMessage != null && nextMessage.MessageIndex - 1 != message.MessageIndex)
                {
                    Logger.Debug("There is some missing messages down the list");
                    IsRefreshing = true;
                    var missingMessages = await App.FriendStore.ChatGetMessages(message.ChatId, message.MessageIndex,
                        nextMessage?.MessageIndex ?? 0, Constants.ChatMessagePageSize);
                    IsRefreshing = false;
                    foreach (var msg in missingMessages.OrderBy(x => x.MessageIndex))
                    {
                        ReceiveMessage(msg);
                    }
                }

                if (listIdx >= Constants.ChatStartIdxToHideScrollUp)
                {
                    //Device.BeginInvokeOnMainThread(() =>
                    //{
                    //while (DelayedMessages.Count > 0)
                    //{
                    //    Messages.Insert(0, DelayedMessages.Dequeue());
                    //}
                    ShowScrollTapUp = false;
                    PendingMessageCount = 0.ToString();
                    //});
                }
                else if (listIdx <= 6)
                {
                    LastMessageVisible = true;
                    Logger.Debug($"listIdx={listIdx}, LastMessageVisible={LastMessageVisible}");
                }
                ShowScrollTapDown = listIdx > Constants.ChatStartIdxToShowScrollDown;
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                IsRefreshing = false;
                Logger.Debug($"OnMessageAppearing.END");
            }
        }

        void OnMessageDisappearing(ChatMessage message)
        {
            Logger.Debug("OnMessageDisappearing.BEGIN");
            if (!message.MessageType.IsShowableMessage()) return;
            var listIdx = Messages.IndexOf(message);
            if (listIdx <= 6)
            {
                LastMessageVisible = false;
                Logger.Debug($"listIdx={listIdx}, LastMessageVisible={LastMessageVisible}");
            }
            Logger.Debug("OnMessageDisappearing.END");
        }

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
