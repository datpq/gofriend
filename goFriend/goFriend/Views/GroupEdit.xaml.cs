using Acr.UserDialogs;
using goFriend.Controls;
using goFriend.DataModel;
using goFriend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class GroupEdit : ContentPage
	{
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        private int? _chatOwnerId;
        private int _groupId;
        private List<Friend> _friends;

		public GroupEdit (int groupId, List<Friend> friends)
		{
			InitializeComponent ();

            _groupId = groupId;
			Title = res.ChatEdit;
            var apiGroup = App.MyGroups.SingleOrDefault(x => x.Group.Id == groupId);
            LblMembers.Text = $"{res.members.CapitalizeFirstLetter()} {res.Groups.ToLower()}: {apiGroup.Group.Name}";
            _chatOwnerId = apiGroup.ChatOwnerId;
            TxtName.Text = apiGroup.Group.Name;
            CmdReset.IsEnabled = CmdSave.IsEnabled = false;
            _friends = friends;
            foreach(var friend in _friends) ChipContainer.Children.Add(CreateChipFromFriend(friend));

            DphFriendList.Initialize(async (selectedItem) =>
            {
                bool changed = false;
                var friend = selectedItem.SelectedObject as Friend ?? (selectedItem.SelectedObject as GroupFriend).Friend;
                if (ChipContainer.Children.All(x => (int)((Chip)x).Tag != friend.Id))
                {
                    ChipContainer.Children.Add(CreateChipFromFriend(friend));
                    changed = true;
                }
                if (changed)
                {
                    CmdReset.IsEnabled = CmdSave.IsEnabled = true;
                }
            });
        }

        public static async Task<List<Friend>> GetFriends(int groupId)
        {
            var result = new List<Friend>();
            var groupFriends = await App.FriendStore.GetGroupFriends(groupId);
            foreach (var friendId in groupFriends.Select(x => x.FriendId))
            {
                var friend = await App.FriendStore.GetFriendInfo(friendId);
                result.Add(friend);
            }
            return result;
        }

        public Chip CreateChipFromFriend(Friend friend)
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
                var chipId = (int)chip.Tag;
                if (chipId != _chatOwnerId)
                {
                    ChipContainer.Children.Remove(chip);
                    CmdReset.IsEnabled = CmdSave.IsEnabled = true;
                }
            });
            return chip;
        }

        private async void CmdSave_Click(object sender, EventArgs e)
        {
            if (ChipContainer.Children.Count <= 1) return;
            try
            {
                UserDialogs.Instance.ShowLoading(res.Processing);
                var lstFriends = new List<Friend>();
                foreach(var x in ChipContainer.Children)
                {
                    var friend = await App.FriendStore.GetFriendInfo((int)((Chip)x).Tag);
                    lstFriends.Add(friend);
                }
                var result = await App.FriendStore.SubscribeGroupMultiple(_groupId, lstFriends.ToArray());
                if (result)
                {
                    await App.RefreshMyGroups();
                    App.DisplayMsgInfo(res.SaveSuccess);
                    CmdReset.IsEnabled = CmdSave.IsEnabled = false;
                }
            }
            catch (Exception)
            {
                App.DisplayMsgError(res.MsgErrConnection);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        private void CmdReset_Click(object sender, EventArgs e)
        {
            ChipContainer.Children.Clear();
            _friends.ForEach(x =>
            {
                ChipContainer.Children.Add(CreateChipFromFriend(x));
            });
            CmdReset.IsEnabled = CmdSave.IsEnabled = false;
        }
    }
}