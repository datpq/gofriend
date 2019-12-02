using System;
using System.Linq;
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

        public void LoadGroupConnectionInfo(string groupName, GroupFriend groupFriend, int startCatIdx)
        {
            GroupConnectionSection.Children.Add(new BoxView
            {
                HorizontalOptions = LayoutOptions.Fill,
                HeightRequest = 1,
                Color = Color.LightGray
            });
            GroupConnectionSection.Children.Add(new Label
            {
                VerticalOptions = LayoutOptions.Center,
                FontSize = (double)Application.Current.Resources["LblFontSize"],
                TextColor = (Color)Application.Current.Resources["ColorLabel"],
                Text = groupName
            });
            var gr = new Grid
            {
                ColumnSpacing = 30,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };
            GroupConnectionSection.Children.Add(gr);
            var selectedGroup = App.MyGroups.FirstOrDefault(x => x.Group.Id == groupFriend.GroupId);
            if (selectedGroup == null) return;
            var arrCats = selectedGroup.Group.GetCatDescList().ToList();
            for (var i = startCatIdx; i < arrCats.Count; i++)
            {
                var lblCat = new Label
                {
                    Text = $"{arrCats[i]}:",
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = (double)Application.Current.Resources["LblDetailFontSize"],
                    TextColor = (Color)Application.Current.Resources["ColorLabel"]
                };
                Grid.SetColumn(lblCat, 0);
                Grid.SetRow(lblCat, i - startCatIdx);
                gr.Children.Add(lblCat);
                var lblCatVal = new Label
                {
                    Text = groupFriend.GetCatByIdx(i + 1),
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = lblCat.FontSize,
                    TextColor = lblCat.TextColor
                };
                Grid.SetColumn(lblCatVal, 1);
                Grid.SetRow(lblCatVal, i - startCatIdx);
                gr.Children.Add(lblCatVal);
            }
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