using System;
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
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public AdminPage()
        {
            InitializeComponent();
            PickerGroups.Title = $"{res.Select} {res.Groups}";
            LblGroup.Text = $"{res.Groups}:";

            UserDialogs.Instance.ShowLoading(res.Processing);
            //can not await TaskGetMyGroups because we are in a constructor, and not in an async method.
            Task.Run(() =>
            {
                App.TaskGetMyGroups.Wait();
                //Task.Delay(5000).Wait();
            }).ContinueWith(task =>
            {
                PickerGroups.ItemsSource = App.MyGroups.Where(x => x.GroupFriend.UserRight >= UserType.Admin).OrderBy(x => x.Group.Name).ToList();
                UserDialogs.Instance.HideLoading();//must be called before setting SelectedIndex
                if (PickerGroups.Items.Count > 0)
                {
                    PickerGroups.SelectedIndex = 0;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async void PickerGroups_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Logger.Debug($"PickerGroups_OnSelectedIndexChanged.BEGIN(SelectedIndex={PickerGroups.SelectedIndex}, SelectedItem={(PickerGroups.SelectedItem as ApiGetGroupsModel)?.Group?.Name})");
                if (!(PickerGroups.SelectedItem is ApiGetGroupsModel selectedGroup)) return;

                UserDialogs.Instance.ShowLoading(res.Processing);
                var groupFixedCatValues = await App.FriendStore.GetGroupFixedCatValues(selectedGroup.Group.Id);
                var arrFixedCats = groupFixedCatValues.GetCatList().ToList();

                UserDialogs.Instance.HideLoading();
                DphListView.Initialize(selectedItem => Navigation.PushAsync(new AccountBasicInfosPage(selectedGroup.Group.Id, selectedItem.Id)),
                    selectedItem => App.DisplayMsgInfo("Denying"),
                    selectedItem => App.DisplayMsgInfo("Accepting"));
                Logger.Debug("Calling DphListView.LoadItems");
                DphListView.LoadItems(async () =>
                {
                    var groupFriends = await App.FriendStore.GetGroupFriends(selectedGroup.Group.Id, false);
                    var result = groupFriends?.Select(x => new DphListViewItemModel
                    {
                        Id = x.FriendId,
                        //ImageUrl = x.Friend.GetImageUrl(FacebookImageType.small), // small 50 x 50
                        ImageUrl = x.Friend.GetImageUrl(), // normal 100 x 100
                        FormattedText = new FormattedString
                        {
                            Spans =
                            {
                                new Span { Text = x.Friend.Name, FontAttributes = FontAttributes.Bold,
                                    FontSize = (double)Application.Current.Resources["LblDetailFontSize"]},
                                new Span {Text = arrFixedCats.Count == 0 ? res.AskedToJoinGroup : $", {x.GetCatValueDisplay(arrFixedCats.Count)}, {res.AskedToJoinGroup}"},
                                new Span {Text = Environment.NewLine},
                                new Span {Text = x.ModifiedDate.HasValue ? x.ModifiedDate.Value.GetSpentTime()
                                    : string.Empty, LineHeight = 1.5}
                            }
                        },
                        Button1ImageSource = "deny.png",
                        Button2ImageSource = "accept.png"
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
                Logger.Debug("PickerGroups_OnSelectedIndexChanged.END");
            }
        }
    }
}