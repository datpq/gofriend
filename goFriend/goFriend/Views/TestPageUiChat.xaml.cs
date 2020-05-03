using System;
using goFriend.DataModel;
using goFriend.Services;
using goFriend.ViewModels;
using PCLAppConfig;
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
            //_viewModel.RefreshScrollDown = () => {
            //    if (_viewModel.Messages.Count > 0)
            //    {
            //        Device.BeginInvokeOnMainThread(() => {
            //            LvMessages.ScrollTo(_viewModel.Messages[_viewModel.Messages.Count - 1], ScrollToPosition.End, true);
            //        });
            //    }
            //};
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

    public class TestModel : ChatViewModel
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public TestModel()
        {
            SendMessageCommand = new Command(async () => {
                Message = string.Empty;
            });

            var msgIndex = 0;
            ChatMessage[] messages =
            {
                new ChatMessage
                {
                    MessageType = ChatMessageType.JoinChat,
                    Message = "Bảo Anh Bảo Linh Bảo Châu tham gia hội thoại",
                    OwnerId = 0,
                    MessageIndex = ++msgIndex,
                    Time = new DateTime(2020, 04, 12, 13, 06, 00)
                },
                new ChatMessage
                {
                    MessageType = ChatMessageType.Text,
                    Message = "Xin chào, có ai ở đây không?",
                    OwnerName = "Bảo Anh Bảo Linh Bảo Châu",
                    OwnerId = 1,
                    MessageIndex = ++msgIndex,
                    IsOwnMessage = true,
                    LogoUrl = "https://graph.facebook.com/10217327271725297/picture?type=small",
                    Time = new DateTime(2020, 04, 12, 14, 06, 00)
                },
                new ChatMessage
                {
                    MessageType = ChatMessageType.JoinChat,
                    Message = "Bảo Thoa tham gia hội thoại",
                    OwnerId = 0
                },
                new ChatMessage
                {
                    MessageType = ChatMessageType.Text,
                    Message = "Có tôi đây?",
                    OwnerName = "Thắng Phạm",
                    Time = new DateTime(2020, 04, 12, 15, 06, 00),
                    OwnerId = 2,
                    MessageIndex = ++msgIndex,
                    IsOwnMessage = false,
                    LogoUrl = "https://graph.facebook.com/2678476115565775/picture?type=small"
                },
                new ChatMessage
                {
                    MessageType = ChatMessageType.Text,
                    Message = "Còn ai nữa không?",
                    OwnerName = "Bảo Anh Bảo Linh Bảo Châu",
                    OwnerId = 1,
                    MessageIndex = ++msgIndex,
                    Time = new DateTime(2020, 04, 13, 15, 07, 00),
                    IsOwnMessage = true,
                    LogoUrl = "https://graph.facebook.com/10217327271725297/picture?type=small"
                },
                new ChatMessage
                {
                    MessageType = ChatMessageType.Text,
                    Message = "Có tớ nữa",
                    OwnerName = "Duong Vu",
                    OwnerId = 3,
                    MessageIndex = ++msgIndex,
                    Time = new DateTime(2020, 04, 13, 15, 07, 30),
                    IsOwnMessage = false,
                    LogoUrl = "https://graph.facebook.com/2959609917385257/picture?type=small"
                },
                new ChatMessage
                {
                    MessageType = ChatMessageType.Text,
                    Message = "Vậy là đủ thành 1 nhóm rồi. Chúng ta sẽ nói chuyện về chủ đề gì ngày hôm nay?:-)",
                    OwnerName = "Thắng Phạm",
                    OwnerId = 2,
                    MessageIndex = ++msgIndex,
                    Time = new DateTime(2020, 04, 13, 15, 23, 00),
                    IsOwnMessage = false,
                    LogoUrl = "https://graph.facebook.com/2678476115565775/picture?type=small"
                },
                new ChatMessage
                {
                    MessageType = ChatMessageType.Text,
                    Message = "máy tớ dùng chrome, để tự động login google, mỗi vấn đề đó thôi",
                    OwnerName = "Bảo Thoa Anh Linh Châu",
                    OwnerId = 4,
                    MessageIndex = ++msgIndex,
                    Time = new DateTime(2020, 04, 15, 16, 25, 00),
                    IsOwnMessage = false,
                    LogoUrl = "https://graph.facebook.com/2351327781814388/picture?type=small"
                },
                new ChatMessage
                {
                    MessageType = ChatMessageType.Text,
                    Message = "- Vậy khi nào chúng ta sẽ có một CEO thực sự?" +
                              "Cảm xúc của Ben khi đó? Câm nín! Trong một cuộc họp với những nhân vật có ảnh hưởng, Ben được coi là 'fake CEO' ngay trước mặt chính nhân viên của mình và sẽ đến lúc công ty cần thuê một người khác. Đơn giản chỉ vì anh là Engineering founder, và không được coi là người phù hợp dẫn dắt công ty.",
                    OwnerName = "Duong Vu",
                    Time = new DateTime(2020, 04, 15, 17, 06, 00),
                    OwnerId = 3,
                    MessageIndex = ++msgIndex,
                    IsOwnMessage = false,
                    LogoUrl = "https://graph.facebook.com/2959609917385257/picture?type=small"
                },
                new ChatMessage
                {
                    MessageType = ChatMessageType.Text,
                    Message = "- Vậy khi nào chúng ta sẽ có một CEO thực sự?" +
                              "Cảm xúc của Ben khi đó? Câm nín! Trong một cuộc họp với những nhân vật có ảnh hưởng, Ben được coi là 'fake CEO' ngay trước mặt chính nhân viên của mình và sẽ đến lúc công ty cần thuê một người khác. Đơn giản chỉ vì anh là Engineering founder, và không được coi là người phù hợp dẫn dắt công ty.",
                    OwnerName = "Duong Vu",
                    Time = new DateTime(2020, 04, 16, 17, 06, 10),
                    OwnerId = 3,
                    MessageIndex = ++msgIndex,
                    IsOwnMessage = true,
                    LogoUrl = "https://graph.facebook.com/2959609917385257/picture?type=small"
                },
                new ChatMessage
                {
                    MessageType = ChatMessageType.Text,
                    Message = "WHEN WILL WE HAVE A REAL CEO?" +
                              "Năm 1999, Ben Horowitz trong vị trí CEO đến gặp một số quỹ đầu tư hàng đầu ở Sillion Valley để gọi vốn cho LoudCloud. Sau một buổi họp tích cực và vui vẻ, người đại diện quỹ đầu tư hỏi Ben?",
                    OwnerName = "Bảo Anh Bảo Linh Bảo Châu",
                    Time = new DateTime(2020, 04, 16, 17, 07, 00),
                    OwnerId = 1,
                    MessageIndex = ++msgIndex,
                    IsOwnMessage = true,
                    LogoUrl = "https://graph.facebook.com/10217327271725297/picture?type=small"
                },
                new ChatMessage
                {
                    MessageType = ChatMessageType.Text,
                    Message =
                        "Nhiều nhà máy ở nước ngoài cũng phải ngừng sản xuất khi có công nhân +. Hậu quả mà ngành y tế phải gánh còn nặng nề hơn. Chủ động buộc đóng cửa cũng là 1 cách hay",
                    OwnerName = "Minh",
                    Time = new DateTime(2020, 04, 17, 17, 09, 00),
                    OwnerId = 5,
                    MessageIndex = ++msgIndex,
                    IsOwnMessage = false,
                    LogoUrl = "https://graph.facebook.com/2719282418189321/picture?type=small"
                },
                new ChatMessage
                {
                    MessageType = ChatMessageType.Text,
                    Message = "Hello",
                    OwnerName = "Bảo Thoa Anh Linh Châu",
                    OwnerId = 4,
                    MessageIndex = ++msgIndex,
                    Time = new DateTime(2020, 04, 17, 16, 25, 00),
                    IsOwnMessage = false,
                    LogoUrl = "https://graph.facebook.com/2351327781814388/picture?type=small"
                },
                new ChatMessage
                {
                    MessageType = ChatMessageType.Text,
                    Message = "- Vậy khi nào chúng ta sẽ có một CEO thực sự?" +
                              "Cảm xúc của Ben khi đó? Câm nín! Trong một cuộc họp với những nhân vật có ảnh hưởng, Ben được coi là 'fake CEO' ngay trước mặt chính nhân viên của mình và sẽ đến lúc công ty cần thuê một người khác. Đơn giản chỉ vì anh là Engineering founder, và không được coi là người phù hợp dẫn dắt công ty.",
                    OwnerName = "Duong Vu",
                    Time = new DateTime(2020, 04, 17, 17, 10, 00),
                    OwnerId = 3,
                    MessageIndex = ++msgIndex,
                    IsOwnMessage = false,
                    LogoUrl = "https://graph.facebook.com/2959609917385257/picture?type=small"
                },
                new ChatMessage
                {
                    MessageType = ChatMessageType.Text,
                    Message = "- Vậy khi nào chúng ta sẽ có một CEO thực sự?" +
                              "Cảm xúc của Ben khi đó? Câm nín! Trong một cuộc họp với những nhân vật có ảnh hưởng, Ben được coi là 'fake CEO' ngay trước mặt chính nhân viên của mình và sẽ đến lúc công ty cần thuê một người khác. Đơn giản chỉ vì anh là Engineering founder, và không được coi là người phù hợp dẫn dắt công ty.",
                    OwnerName = "Duong Vu",
                    Time = new DateTime(2020, 04, 20, 19, 06, 00),
                    OwnerId = 3,
                    MessageIndex = ++msgIndex,
                    IsOwnMessage = false,
                    LogoUrl = "https://graph.facebook.com/2959609917385257/picture?type=small"
                }
            };
            foreach (var chatMessage in messages)
            {
                ReceiveMessage(chatMessage);
            }

            ChatName = "A0-K26DHTH";
            ChatLogoUrl = $"{ConfigurationManager.AppSettings["HomePageUrl"]}/logos/g12.png";

            //Code to simulate reveing a new message procces
            var random = new Random();
            Device.StartTimer(TimeSpan.FromSeconds(5), () =>
            {
                var randomNum = random.Next(14);
                var msgIdx = Messages.Count - randomNum - 1; // 0-14
                Logger.Debug($"randomNum={randomNum}, msgIdx={msgIdx}");
                var newMessage = new ChatMessage
                {
                    Message = Messages[msgIdx].Message,
                    OwnerName = Messages[msgIdx].OwnerName,
                    OwnerFirstName = Messages[msgIdx].OwnerFirstName,
                    OwnerId = Messages[msgIdx].OwnerId,
                    MessageIndex = ++msgIndex,
                    Time = Messages[msgIdx].Time,
                    IsOwnMessage = Messages[msgIdx].IsOwnMessage,
                    LogoUrl = Messages[msgIdx].LogoUrl
                };
                if (LastMessageVisible)
                {
                    ReceiveMessage(newMessage);
                }
                else
                {
                    DelayedMessages.Enqueue(newMessage);
                    PendingMessageCount++;
                }
                return true;
            });
        }
    }
}