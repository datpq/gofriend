using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using goFriend.Controls;
using goFriend.DataModel;
using goFriend.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatEdit : ContentPage
    {
        private List<Friend> _friends;

        public ChatEdit(ChatViewModel chat = null, List<Friend> friends = null)
        {
            InitializeComponent();
            BindingContext = chat;

            Title = chat == null ? res.ChatNew : res.ChatEdit;
            LblMembers.Text = $"{res.members.CapitalizeFirstLetter()}:";
            TxtName.Text = chat?.ChatName;
            _friends = friends ?? new List<Friend>();

            if (chat != null && friends != null)
            {
                foreach (var friend in _friends) ChipContainer.Children.Add(CreateChipFromFriend(friend));
            }
            else // if new Chat --> Add current user as owner
            {
                ChipContainer.Children.Add(CreateChipFromFriend(App.User));
            }
            RefreshName();
            CmdReset.IsEnabled = CmdSave.IsEnabled = false;

            DphFriendList.Initialize(async (selectedItem) =>
            {
                bool changed = false;
                var friend = selectedItem.SelectedObject as Friend ?? (selectedItem.SelectedObject as GroupFriend).Friend;
                if (ChipContainer.Children.All(x => (int) ((Chip) x).Tag != friend.Id))
                {
                    ChipContainer.Children.Add(CreateChipFromFriend(friend));
                    RefreshName();
                    changed = true;
                }
                if (changed)
                {
                    CmdReset.IsEnabled = CmdSave.IsEnabled = true;
                }
            });
        }

        public static async Task<List<Friend>> GetFriends(int[] friendIds)
        {
            var result = new List<Friend>();
            foreach (var friendId in friendIds)
            {
                var friend = await App.FriendStore.GetFriendInfo(friendId);
                result.Add(friend);
            }
            return result;
        }

        private Chip CreateChipFromFriend(Friend friend)
        {
            var chip = new Chip
            {
                Text = friend.FirstName,
                FontSize = (double)Application.Current.Resources["LblDetailFontSize"],
                Tag = friend.Id,
                BackgroundColor = (Color)Application.Current.Resources["ColorChatGray"],
                CloseImage = Constants.ImgDeny,
                Image = friend.GetImageUrl(FacebookImageType.small),
            };
            chip.CloseCommand = new Command(() =>
            {
                var chipId = (int) chip.Tag;
                var vm = (ChatViewModel)BindingContext;
                if (vm != null && chipId != vm.ChatListItem.Chat.OwnerId)
                {
                    ChipContainer.Children.Remove(chip);
                    RefreshName();
                    CmdReset.IsEnabled = CmdSave.IsEnabled = true;
                }
            });
            return chip;
        }

        private void RefreshName()
        {
            TxtName.IsEnabled = ChipContainer.Children.Count > 2;
            switch (ChipContainer.Children.Count)
            {
                case 1:
                    TxtName.Text = string.Empty;
                    break;
                case 2:
                    TxtName.Text = (ChipContainer.Children[1] as Chip)?.Text;
                    break;
                default:
                    if (TxtName.Text == (ChipContainer.Children[1] as Chip)?.Text)
                    {
                        TxtName.Text =
                            $"{(ChipContainer.Children[0] as Chip)?.Text}, {(ChipContainer.Children[1] as Chip)?.Text}, {(ChipContainer.Children[2] as Chip)?.Text}";
                    }
                    break;
            }
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

        private async void CmdSave_Click(object sender, EventArgs e)
        {
            if (ChipContainer.Children.Count <= 1) return;
            try
            {
                var chatVm = (ChatViewModel)BindingContext;
                var newChat = new Chat
                {
                    Id = chatVm?.ChatListItem?.Chat?.Id ?? 0,
                    Members = string.Join(",", ChipContainer.Children.Select(x => $"u{((Chip)x).Tag}")),
                    Name = ChipContainer.Children.Count == 2 ? string.Empty : TxtName.Text,
                };
                await App.FriendStore.SendCreateChat(newChat);
                chatVm.ChatName = newChat.Name;
                CmdReset.IsEnabled = CmdSave.IsEnabled = false;
                await Navigation.PopAsync();
            }
            catch (Exception)
            {
                App.DisplayMsgError(res.MsgErrConnection);
            }
        }

        private void CmdReset_Click(object sender, EventArgs e)
        {
            ChipContainer.Children.Clear();
            _friends.ForEach(x =>
            {
                ChipContainer.Children.Add(CreateChipFromFriend(x));
            });
            TxtName.Text = (BindingContext as ChatViewModel)?.ChatName;
            CmdReset.IsEnabled = CmdSave.IsEnabled = false;
        }

        private void TxtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            CmdReset.IsEnabled = CmdSave.IsEnabled = true;
        }
    }
}