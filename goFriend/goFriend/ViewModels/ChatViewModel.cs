﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using goFriend.DataModel;
using goFriend.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace goFriend.ViewModels
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());
        public ICommand CommandMembers { get; set; }
        public ICommand CommandMute { get; set; }
        public ICommand CommandEdit { get; set; }

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

        public bool IsEnabled => App.FriendStore.SignalR.IsConnected;
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
                OnPropertyChanged(nameof(SendIcon));
            }
        }

        public string SendIcon => string.IsNullOrWhiteSpace(Message) ? Constants.IconThumbsUp : Constants.IconSend;

        // Messages is in descending order
        // [0] is the last message with max MessageIndex
        // [count - 1] is the first message with MessageIndex = 0
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
                await App.FriendStore.SignalR.ConnectAsync();
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
                await App.FriendStore.SignalR.ConnectAsync();
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
                //Logger.Debug($"ReceiveMessage.BEGIN(ChatId={chatMessage.ChatId}, MessageIndex={chatMessage.MessageIndex})");
                chatMessage.Chat = ChatListItem.Chat;
                chatMessage.CreatedDate = chatMessage.CreatedDate.ToLocalTime();
                chatMessage.ModifiedDate = chatMessage.ModifiedDate.ToLocalTime();
                chatMessage.IsOwnMessage = chatMessage.OwnerId == App.User.Id;
                if (chatMessage.IsDeleted)
                {
                    chatMessage.Message =
                        $"{(chatMessage.IsOwnMessage ? res.You : chatMessage.OwnerFirstName)}: {res.ChatMessageIsDeleted}";
                }

                var lastDateTime = new DateTime(2000, 1, 1);
                var arrIdx = 0;
                // Messages[0] is the first message received.
                // Searching from 0 until the message that have MessageIndex greater than the one of current message
                // example Messages index = 1, 2, 3, ..., 10, 15, 16, ... 100...
                // Receive 14 --> arrIdx will return 10
                for (arrIdx = 0; arrIdx < Messages.Count; arrIdx++)
                {
                    if (Messages[arrIdx].MessageIndex > chatMessage.MessageIndex) continue;
                    lastDateTime = Messages[arrIdx].CreatedDate.Date;
                    break;
                }
                //Logger.Debug($"arrIdx={arrIdx}, Messages.Count={Messages.Count}");

                if (lastDateTime < chatMessage.CreatedDate.Date)
                {
                    // if the SysDate message on the same day exist already, just change the MessageIndex
                    if (arrIdx > 0 && Messages[arrIdx - 1].MessageType == ChatMessageType.SysDate
                                   && Messages[arrIdx - 1].CreatedDate.Date == chatMessage.CreatedDate.Date)
                    {
                        arrIdx--;
                        Messages[arrIdx].MessageIndex = chatMessage.MessageIndex;
                        //Logger.Debug($"arrIdx={arrIdx}");
                    }
                    else
                    {
                        //Logger.Debug($"SysDate message added for {chatMessage.CreatedDate.Date:yyyy MMMM dd}");
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

                if (Messages.Any(x => x.MessageIndex == chatMessage.MessageIndex && !x.IsSystemMessage)) // Modification, Deletion
                {
                    arrIdx = Messages.IndexOf(Messages.Single(x => x.MessageIndex == chatMessage.MessageIndex && !x.IsSystemMessage));
                    if (arrIdx != -1)
                    {
                        Messages[arrIdx] = chatMessage;
                    }
                }
                else // New message
                {
                    Messages.Insert(arrIdx, chatMessage);
                }
                if (arrIdx == 0 && ChatListItem != null && !chatMessage.IsSystemMessage)
                {
                    ChatListItem.IsLastMessageRead = LastMessageVisible && ChatListItem.IsAppearing; //when page is appearing, the last message is read
                    var msg = chatMessage.IsDeleted ? res.ChatMessageIsDeleted
                        : chatMessage.MessageType == ChatMessageType.Text ? (chatMessage.IsThumbsUp ? "👍" : chatMessage.Message)
                        : chatMessage.MessageType  == ChatMessageType.Attachment ? res.ChatMessageIsImage : null;
                    ChatListItem.LastMessage = $"{(chatMessage.IsOwnMessage ? res.You : chatMessage.OwnerFirstName)}: {msg}";
                    if (!ChatListItem.IsAppearing && chatMessage.MessageIndex >= LastReadMsgIdxWhenAppearing + Constants.ChatMinPendingMsg)
                    {
                        LastReadMsgIdx = LastReadMsgIdxWhenAppearing;
                        PendingMessageCount = (chatMessage.MessageIndex - LastReadMsgIdx).ToString();
                    }
                }

                //check with the last message retrieved, it's it's newer play a sound and vibration
                //always save the newer in setting for the next time retrieving messages
                var arrLastMsgIdxRetrievedByChatId = Settings.LastMsgIdxRetrievedByChatId;
                if (arrLastMsgIdxRetrievedByChatId == null)
                {
                    Logger.Debug($"Creating Settings.LastMsgIdxRetrievedByChatId");
                    arrLastMsgIdxRetrievedByChatId = new Dictionary<int, int>();
                    Settings.LastMsgIdxRetrievedByChatId = arrLastMsgIdxRetrievedByChatId;
                }
                if (!arrLastMsgIdxRetrievedByChatId.TryGetValue(chatMessage.ChatId, out int lastMsgIdxRetrieved))
                {
                    Logger.Debug($"No last msgidx ever saved for chat {chatMessage.ChatId}. Creating a new one.");
                    lastMsgIdxRetrieved = 0;
                }
                //Logger.Debug($"chatId={chatMessage.ChatId}, lastMsgIdxRetrieved={lastMsgIdxRetrieved}");
                if (lastMsgIdxRetrieved < chatMessage.MessageIndex)
                {
                    //Save new value in the setting
                    arrLastMsgIdxRetrievedByChatId[chatMessage.ChatId] = lastMsgIdxRetrieved = chatMessage.MessageIndex;
                    Settings.LastMsgIdxRetrievedByChatId = arrLastMsgIdxRetrievedByChatId;

                    if (ChatListItem != null && !ChatListItem.IsAppearing && !chatMessage.IsOwnMessage && !IsMute)
                    {
                        App.SapChatNewMessage.Play();
                        Vibration.Vibrate();

                        if (!chatMessage.IsSystemMessage) {
                            if (!App.NotificationChatInboxLinesById.TryGetValue(chatMessage.ChatId, out List<string[]> inputLines))
                            {
                                inputLines = new List<string[]>();
                                App.NotificationChatInboxLinesById.Add(chatMessage.ChatId, inputLines);
                            }
                            inputLines.Add(new[] { chatMessage.OwnerFirstName, chatMessage.IsThumbsUp ? "👍" : chatMessage.Message });

                            App.NotificationService.SendNotification(
                                new Models.ServiceNotification
                                {
                                    ContentTitle = chatMessage.Chat.Name,
                                    ContentText = null,
                                    SummaryText = null,
                                    LargeIconUrl = chatMessage.Chat.GetChatType() == ChatType.Individual ? chatMessage.LogoUrl : chatMessage.Chat.LogoUrl,
                                    NotificationType = Models.NotificationType.ChatReceiveMessage,
                                    ExtraId = chatMessage.ChatId,
                                    InboxLines = inputLines
                                });
                        }
                    }
                }
                //Logger.Debug($"Message {chatMessage.Id}, ChatId={chatMessage.ChatId}, MessageIndex={chatMessage.MessageIndex} received. Added at {arrIdx}.");
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                //Logger.Debug($"ReceiveMessage.END");
            }
        }

        public ChatMessage GetPreviousMessage(int msgListIdx)
        {
            ChatMessage result = null;
            var idx = msgListIdx + 1;
            while (idx < Messages.Count)
            {
                if (Messages[idx].MessageType.IsRealShowableMessage())
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
                if (Messages[idx].MessageType.IsRealShowableMessage())
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
            if (!message.MessageType.IsRealShowableMessage()) return;
            try
            {
                var listIdx = Messages.IndexOf(message);
                Logger.Debug($"OnMessageAppearing.BEGIN(listIdx={listIdx}, MessageIndex={message.MessageIndex})");

                //go up the list find the previous message
                var previousMessage = GetPreviousMessage(listIdx);
                if ((previousMessage == null && message.MessageIndex > 3) || // The first 2 messages are CreateChat messages
                    (previousMessage != null && previousMessage.MessageIndex + 1 != message.MessageIndex))
                {
                    Logger.Debug("There is some missing messages up the list");
                    //do not show Refreshing on iOS, that make ListView to scroll to Top unexpectedly
                    if (Device.RuntimePlatform == Device.Android) IsRefreshing = true;
                    var missingMessages = await App.FriendStore.ChatGetMessages(message.ChatId, message.MessageIndex,
                        previousMessage?.MessageIndex ?? 0, Constants.ChatMessagePageSize);
                    //do not show Refreshing on iOS, that make ListView to scroll to Top unexpectedly
                    if (Device.RuntimePlatform == Device.Android) IsRefreshing = false;
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
                    //do not show Refreshing on iOS, that make ListView to scroll to Top unexpectedly
                    if (Device.RuntimePlatform == Device.Android) IsRefreshing = true;
                    var missingMessages = await App.FriendStore.ChatGetMessages(message.ChatId, message.MessageIndex,
                        nextMessage?.MessageIndex ?? 0, Constants.ChatMessagePageSize);
                    //do not show Refreshing on iOS, that make ListView to scroll to Top unexpectedly
                    if (Device.RuntimePlatform == Device.Android) IsRefreshing = false;
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
                //do not show Refreshing on iOS, that make ListView to scroll to Top unexpectedly
                if (Device.RuntimePlatform == Device.Android) IsRefreshing = false;
                Logger.Debug($"OnMessageAppearing.END");
            }
        }

        void OnMessageDisappearing(ChatMessage message)
        {
            Logger.Debug("OnMessageDisappearing.BEGIN");
            if (!message.MessageType.IsRealShowableMessage()) return;
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
