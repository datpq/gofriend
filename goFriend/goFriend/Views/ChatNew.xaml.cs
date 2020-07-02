using System;
using System.Linq;
using goFriend.Controls;
using goFriend.DataModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatNew : ContentPage
    {
        public ChatNew()
        {
            InitializeComponent();

            //LblMembers.Text = $"{res.Select} {res.members}:";
            LblMembers.Text = $"{res.members.CapitalizeFirstLetter()}:";
            DphFriendList.Initialize(async (selectedItem) =>
            {
                var selectedGroupFriend = (GroupFriend)selectedItem.SelectedObject;
                if (ChipContainer.Children.All(x => (int) ((Chip) x).Tag != selectedGroupFriend.Friend.Id))
                {
                    var chip = new Chip
                    {
                        Text = selectedGroupFriend.Friend.FirstName,
                        FontSize = (double)Application.Current.Resources["LblDetailFontSize"],
                        Tag = selectedGroupFriend.Friend.Id,
                        BackgroundColor = (Color)Application.Current.Resources["ColorChatGray"],
                        CloseImage = Constants.ImgDeny,
                        Image = selectedGroupFriend.Friend.GetImageUrl(FacebookImageType.small),
                    };
                    ChipContainer.Children.Add(chip);
                    chip.CloseCommand = new Command(() =>
                    {
                        ChipContainer.Children.Remove(chip);
                    });
                }
            });
        }

        //private bool _isShowingAllChips;
        //public bool IsShowingAllChips
        //{
        //    get => _isShowingAllChips;
        //    set
        //    {
        //        _isShowingAllChips = value;
        //        Grid.RowDefinitions[0].Height = _isShowingAllChips ? GridLength.Auto : 100;
        //    }
        //}

        //private void CmdExpandAllChips_OnClicked(object sender, EventArgs e)
        //{
        //    _isShowingAllChips = !_isShowingAllChips;
        //    CmdExpandAllChips.ImageSource = _isShowingAllChips ? Constants.ImgFolderOpen : Constants.ImgFolderClose;
        //}

        private void CmdOk_OnClicked(object sender, EventArgs e)
        {
            var members = string.Join(",", ChipContainer.Children.Select(x => $"u{((Chip) x).Tag}"));
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                TxtName.Text = App.User.FirstName;
                var memberCount = 0;
                foreach (var memberChip in ChipContainer.Children)
                {
                    var chip = (Chip) memberChip;
                    if ((int) chip.Tag != App.User.Id)
                    {
                        memberCount++;
                        TxtName.Text = $"{TxtName.Text}-{chip.Text}";
                        if (memberCount == 2) break;
                    }
                }
            }
            var chat = new Chat
            {
                Members = members,
                Name = TxtName.Text,
            };
            App.FriendStore.SendCreateChat(chat);
            Navigation.PopAsync();
        }

        private void CmdClear_OnClicked(object sender, EventArgs e)
        {
            ChipContainer.Children.Clear();
        }
    }
}