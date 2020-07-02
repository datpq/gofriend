using goFriend.DataModel;
using goFriend.ViewModels;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OnlineMembersPage : ContentPage
    {
        public OnlineMembersPage(ChatViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;

            Title = res.members.CapitalizeFirstLetter();
            if (Device.RuntimePlatform == Device.Android)
            {
                MnuMembers.IconImageSource = Constants.ImgGroup;
                //MnuMembers.IconImageSource = vm.LogoUrl;
            }

            DphListView.Initialize(async (selectedItem) =>
            {
                if (vm.ChatListItem.Chat.Members.StartsWith("g")
                    && int.TryParse(vm.ChatListItem.Chat.Members.Substring(1), out var groupId))
                {
                    await App.GotoAccountInfo(groupId, selectedItem.Id);
                }
            });
            DphListView.LoadItems(async () =>
            {
                var now = DateTime.Now;
                var result = vm.Members.OrderByDescending(
                    x => x.Time.AddMinutes(Constants.ChatPingFrequence) >= now ? now : x.Time).ThenBy(
                    x => x.Friend.Name).Select(x => new DphListViewItemModel
                    {
                        Id = x.Friend.Id,
                        SelectedObject = x,
                        ImageUrl = x.LogoUrl,
                        IsHighlight = x.Time.AddMinutes(Constants.ChatPingFrequence) >= now,
                        //ImageUrl = x.Friend.GetImageUrl(), // normal 100 x 100
                        FormattedText = new FormattedString
                        {
                            Spans =
                            {
                                new Span {Text = x.Friend.Name, FontAttributes = FontAttributes.Bold,
                                    FontSize = (double)Application.Current.Resources["LblFontSize"], LineHeight = 1.2},
                                new Span {Text = Environment.NewLine},
                                new Span {Text = x.Time.AddMinutes(
                                    Constants.ChatPingFrequence) >= now ? res.Online :
                                    string.Format(res.OnlineEarlier, x.Time.GetSpentTime()), LineHeight = 1.2}
                            }
                        }
                    });
                return result;
            });
        }
    }
}