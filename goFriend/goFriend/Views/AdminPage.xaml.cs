using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using goFriend.DataModel;
using goFriend.Services;
using goFriend.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AdminPage : ContentPage
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        public AdminPage()
        {
            InitializeComponent();

            PickerGroups.Title = $"{res.Select} {res.Groups}";
            LblGroup.Text = $"{res.Groups}:";

            UserDialogs.Instance.ShowLoading(res.Processing);
            //can not await TaskInitialization because we are in a constructor, and not in an async method.
            App.TaskInitialization.Wait();
                PickerGroups.ItemsSource = App.MyGroups.Where(x => x.GroupFriend.UserRight >= UserType.Admin).OrderBy(x => x.Group.Name).ToList();
                UserDialogs.Instance.HideLoading();//must be called before setting SelectedIndex
                for (var i = 0; i < PickerGroups.Items.Count; i++)
                {
                    if (PickerGroups.Items[i] == Settings.LastAdminPageGroupNme)
                    {
                        PickerGroups.SelectedIndex = i;
                        break;
                    }
                }
                if (PickerGroups.SelectedIndex < 0 && PickerGroups.Items.Count > 0)
                {
                    PickerGroups.SelectedIndex = 0;
                }
                Appearing += (sender, args) => DphListView.Refresh();
        }

        private async void PickerGroups_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Logger.Debug($"PickerGroups_OnSelectedIndexChanged.BEGIN(SelectedIndex={PickerGroups.SelectedIndex}, SelectedItem={(PickerGroups.SelectedItem as ApiGetGroupsModel)?.Group?.Name})");
                if (!(PickerGroups.SelectedItem is ApiGetGroupsModel selectedGroup)) return;
                Settings.LastAdminPageGroupNme = selectedGroup.Group.Name;

                UserDialogs.Instance.ShowLoading(res.Processing);
                var groupFixedCatValues = await App.FriendStore.GetGroupFixedCatValues(selectedGroup.Group.Id);
                var arrFixedCats = groupFixedCatValues?.GetCatList().ToList() ?? new List<string>();

                DphListView.Initialize(async(selectedItem) =>
                    {
                        var selectedGroupFriend = (GroupFriend) selectedItem.SelectedObject;
                        var accountBasicInfoPage = new AccountBasicInfosPage();
                        await accountBasicInfoPage.Initialize(selectedGroup.Group, selectedGroupFriend, arrFixedCats.Count);
                        await Navigation.PushAsync(accountBasicInfoPage);
                    },
                    async selectedItem =>
                    {
                        Logger.Debug($"selectedItem.Id={selectedItem.Id}");
                        if (await App.DisplayMsgQuestion(res.MsgSubscriptionRejectConfirm))
                        {
                            var result = await App.FriendStore.GroupSubscriptionReact(selectedItem.Id, UserType.NotMember);
                            if (result)
                            {
                                DphListView.Refresh(true);
                            }
                        }
                    },
                    async selectedItem =>
                    {
                        Logger.Debug($"selectedItem.Id={selectedItem.Id}");
                        if (await App.DisplayMsgQuestion(res.MsgSubscriptionApproveConfirm))
                        {
                            var result = await App.FriendStore.GroupSubscriptionReact(selectedItem.Id, UserType.Normal);
                            if (result)
                            {
                                DphListView.Refresh(true);
                            }
                        }
                    });
                DphListView.LoadItems(async () =>
                {
                    var groupFriends = await App.FriendStore.GetGroupFriends(selectedGroup.Group.Id, false);
                    var result = groupFriends?.Select(x => new DphListViewItemModel
                    {
                        Id = x.Id,
                        SelectedObject = x,
                        //ImageUrl = x.Friend.GetImageUrl(FacebookImageType.small), // small 50 x 50
                        ImageUrl = x.Friend.GetImageUrl(), // normal 100 x 100
                        FormattedText = new FormattedString
                        {
                            Spans =
                            {
                                new Span { Text = x.Friend.Name, FontAttributes = FontAttributes.Bold,
                                    FontSize = (double)Application.Current.Resources["LblFontSize"]},
                                new Span {Text = arrFixedCats.Count == x.GetCatList().Count() ? $" {res.AskedToJoinGroup}" : $", {x.GetCatValueDisplay(arrFixedCats.Count)}, {res.AskedToJoinGroup}"},
                                new Span {Text = Environment.NewLine},
                                new Span {Text = x.ModifiedDate.HasValue ? x.ModifiedDate.Value.GetSpentTime()
                                    : string.Empty, LineHeight = 1.3}
                            }
                        },
                        Button1ImageSource = Constants.ImgDeny,
                        Button2ImageSource = Constants.ImgAccept,
                    });
                    return result;
                });
            }
            catch (Exception ex)
            {
                App.DisplayMsgError(ex.Message);
                Logger.Error(ex.ToString());
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
                Logger.Debug("PickerGroups_OnSelectedIndexChanged.END");
            }
        }
    }
}