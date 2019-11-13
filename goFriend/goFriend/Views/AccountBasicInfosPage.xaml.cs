using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using goFriend.DataModel;
using goFriend.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AccountBasicInfosPage : ContentPage
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public AccountBasicInfosPage(Friend friend)
        {
            InitializeComponent();
            Initialize(friend);
        }

        public AccountBasicInfosPage(int groupId, int otherFriendId)
        {
            InitializeComponent();
            UserDialogs.Instance.ShowLoading(res.Processing);
            Task.Run(() => App.FriendStore.GetFriend(groupId, otherFriendId)).ContinueWith(friendTask =>
            {
                UserDialogs.Instance.HideLoading();
                var otherFriend = friendTask.Result;
                Initialize(otherFriend);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void Initialize(Friend friend)
        {
            BindingContext = friend;
            LblGender.Text = friend.Gender == "male" ? res.Male : res.Female;
            ImgAvatar.Source = friend.GetImageUrl();
        }

        private async void CmdSave_Click(object sender, EventArgs e)
        {
            try
            {
                Logger.Debug("CmdSave_Click.BEGIN");
                var oldName = App.User.Name;
                var oldEmail = App.User.Email;
                var oldBirthDay = App.User.Birthday;
                var oldGender = App.User.Gender;
                Logger.Debug("Calling SaveBasicInfo...");
                var result = await App.FriendStore.SaveBasicInfo(App.User);
                if (result)
                {
                    Settings.LastUser = App.User;
                    App.DisplayMsgInfo(res.SaveSuccess);
                }
                else
                {
                    Logger.Error("Saving failed.");
                    App.User.Name = oldName;
                    App.User.Email = oldEmail;
                    App.User.Birthday = oldBirthDay;
                    App.User.Gender = oldGender;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
            finally
            {
                Logger.Debug("CmdSave_Click.END");
            }
        }
    }
}