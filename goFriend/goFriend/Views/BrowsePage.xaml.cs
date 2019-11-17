using System;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using goFriend.DataModel;
using goFriend.Services;
using goFriend.ViewModels;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BrowsePage : ContentPage
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public BrowsePage()
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
                PickerGroups.ItemsSource = App.MyGroups.Where(x => x.GroupFriend.Active).OrderBy(x => x.Group.Name).ToList();
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
                UserDialogs.Instance.ShowLoading(res.Processing);
                if (!(PickerGroups.SelectedItem is ApiGetGroupsModel selectedGroup)) return;
                var rowToRemove = Grid.Children.Where(x => Grid.GetRow(x) > 0).ToList();
                foreach (var child in rowToRemove)
                {
                    Grid.Children.Remove(child);
                }
                while (Grid.RowDefinitions.Count > 1)
                {
                    Grid.RowDefinitions.RemoveAt(1);
                }
                var groupFixedCatValues = await App.FriendStore.GetGroupFixedCatValues(selectedGroup.Group.Id);
                var arrCats = selectedGroup.Group.GetCatDescList().ToList();
                var arrFixedCats = groupFixedCatValues.GetCatList().ToList();
                var arrPickers = new Picker[arrCats.Count - arrFixedCats.Count];
                for (var i = 0; i < arrCats.Count - arrFixedCats.Count; i++)
                {
                    //Logger.Debug($"i={i}");
                    var lbl = new Label
                    {
                        FontSize = LblGroup.FontSize,
                        VerticalOptions = LblGroup.VerticalOptions,
                        TextColor = LblGroup.TextColor,
                        Text = $"{arrCats[i + arrFixedCats.Count]}:"
                    };
                    Grid.SetColumn(lbl, 0);
                    Grid.SetRow(lbl, i + 1);
                    Grid.Children.Add(lbl);
                    var picker = new Picker
                    {
                        FontSize = PickerGroups.FontSize,
                        HeightRequest = PickerGroups.HeightRequest,
                        //ItemDisplayBinding = new Binding("Display"),
                        ItemDisplayBinding = PickerGroups.ItemDisplayBinding,
                        Title = $"{res.Select} {arrCats[i + arrFixedCats.Count]}"
                    };
                    arrPickers[i] = picker;
                    if (i == 0)
                    {
                        var groupCatValues = await App.FriendStore.GetGroupCatValues(selectedGroup.Group.Id);
                        picker.ItemsSource = groupCatValues.ToList();
                        //await App.FriendStore.GetGroupCatValues(selectedGroup.Group.Id).ContinueWith(task =>
                        //    {
                        //        picker.ItemsSource = task.Result.ToList();
                        //    }, TaskScheduler.FromCurrentSynchronizationContext());
                    }
                    var localI = i;
                    picker.SelectedIndexChanged += async (o, args) =>
                    {
                        UserDialogs.Instance.ShowLoading(res.Processing);
                        //Logger.Debug($"localI={localI}");
                        var arrCatValues = new string[localI+1];
                        for (var k = 0; k < arrCatValues.Length; k++)
                        {
                            //Logger.Debug($"arrPickers[{k}].SelectedItem={arrPickers[k].SelectedItem}");
                            arrCatValues[k] = (arrPickers[k].SelectedItem as ApiGetGroupCatValuesModel)?.CatValue;
                        }
                        Logger.Debug($"arrCatValues={JsonConvert.SerializeObject(arrCatValues)}");
                        for (var j = localI + 1; j < arrPickers.Length; j++)
                        {
                            arrPickers[j].ItemsSource = null;
                            //Logger.Debug($"j={j}");
                            if (j == localI + 1)
                            {
                                var localJ = j;
                                var groupCatValues = await App.FriendStore.GetGroupCatValues(selectedGroup.Group.Id, true, arrCatValues);
                                arrPickers[localJ].ItemsSource = groupCatValues.ToList();
                                //await App.FriendStore.GetGroupCatValues(selectedGroup.Group.Id, true, arrCatValues).ContinueWith(task =>
                                //{
                                //    arrPickers[localJ].ItemsSource = task.Result.ToList();
                                //}, TaskScheduler.FromCurrentSynchronizationContext());
                            }
                        }
                        UserDialogs.Instance.HideLoading();
                        DphListView.Initialize(selectedItem => Navigation.PushAsync(new AccountBasicInfosPage(selectedGroup.Group.Id, selectedItem.Id)));
                        Logger.Debug("Calling DphListView.LoadItems");
                        DphListView.LoadItems(async () =>
                        {
                            var catGroupFriends = await App.FriendStore.GetGroupFriends(selectedGroup.Group.Id, true, true, arrCatValues);
                            var result = catGroupFriends.Select(x => new DphListViewItemModel
                            {
                                Id = x.FriendId,
                                //ImageUrl = x.Friend.GetImageUrl(FacebookImageType.small) // small 50 x 50
                                ImageUrl = x.Friend.GetImageUrl(), // normal 100 x 100
                                FormattedText = new FormattedString
                                {
                                    Spans =
                                    {
                                        new Span {Text = x.Friend.Name, FontAttributes = FontAttributes.Bold,
                                            FontSize = (double)Application.Current.Resources["LblDetailFontSize"], LineHeight = 1.2},
                                        new Span {Text = Environment.NewLine},
                                        new Span {Text = x.GetCatValueDisplay(arrFixedCats.Count), LineHeight = 1.2},
                                        new Span {Text = Environment.NewLine},
                                        new Span {Text = "Vương Quốc Anh", LineHeight = 1.2}
                                    }
                                }
                            });
                            return result;
                        });
                    };
                    Grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto});
                    Grid.SetColumn(picker, 1);
                    Grid.SetRow(picker, i + 1);
                    Grid.Children.Add(picker);
                }

                UserDialogs.Instance.HideLoading();
                DphListView.Initialize(selectedItem => Navigation.PushAsync(new AccountBasicInfosPage(selectedGroup.Group.Id, selectedItem.Id)));
                Logger.Debug("Calling DphListView.LoadItems");
                DphListView.LoadItems(async () =>
                {
                    var groupFriends = await App.FriendStore.GetGroupFriends(selectedGroup.Group.Id);
                    var result = groupFriends.Select(x => new DphListViewItemModel
                    {
                        Id = x.FriendId,
                        //ImageUrl = x.Friend.GetImageUrl(FacebookImageType.small), // small 50 x 50
                        ImageUrl = x.Friend.GetImageUrl(), // normal 100 x 100
                        FormattedText = new FormattedString
                        {
                            Spans =
                            {
                                new Span {Text = x.Friend.Name, FontAttributes = FontAttributes.Bold,
                                    FontSize = (double)Application.Current.Resources["LblDetailFontSize"], LineHeight = 1.2},
                                new Span {Text = Environment.NewLine},
                                new Span {Text = x.GetCatValueDisplay(arrFixedCats.Count), LineHeight = 1.2},
                                new Span {Text = Environment.NewLine},
                                new Span {Text = "Vương Quốc Anh", LineHeight = 1.2}
                            }
                        }
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