using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using goFriend.DataModel;
using goFriend.Services;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DphFriendSelection : ContentView
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public static readonly BindableProperty IsShowingCategoriesProperty =
            BindableProperty.CreateAttached(nameof(IsShowingCategories), typeof(bool), typeof(DphFriendSelection), true);
        public bool IsShowingCategories
        {
            get => (bool)GetValue(IsShowingCategoriesProperty);
            set => SetValue(IsShowingCategoriesProperty, value);
        }

        public DphFriendSelection()
        {
            InitializeComponent();

            PickerGroups.Title = $"{res.Select} {res.Groups}";
            LblGroup.Text = $"{res.Groups}:";

            Refresh();
        }

        private Action<ApiGetGroupsModel, List<string>, string[]> _onSelectionAction;
        public void Initialize(Action<ApiGetGroupsModel, List<string>, string[]> onSelectionAction)
        {
            _onSelectionAction = onSelectionAction;
        }

        public void Refresh()
        {
            Logger.Debug("Refresh.BEGIN");
            UserDialogs.Instance.ShowLoading(res.Processing);
            //can not await TaskGetMyGroups because we are in a constructor, and not in an async method.
            Task.Run(() =>
            {
                App.TaskGetMyGroups.Wait();
                //Task.Delay(5000).Wait();
            }).ContinueWith(task =>
            {
                var selectedIndex = PickerGroups.SelectedIndex;
                var myGroups = App.MyGroups == null ? new List<ApiGetGroupsModel>() : App.MyGroups.Where(x => x.GroupFriend.Active).OrderBy(x => x.Group.Name).ToList();
                PickerGroups.ItemsSource = new ObservableCollection<ApiGetGroupsModel>(myGroups);
                UserDialogs.Instance.HideLoading();//must be called before setting SelectedIndex
                if (selectedIndex < 0 && PickerGroups.Items.Count > 0)
                {
                    PickerGroups.SelectedIndex = 0;
                } else if (selectedIndex >= 0 && selectedIndex < PickerGroups.Items.Count)
                {
                    PickerGroups.SelectedIndex = selectedIndex;
                }
                Logger.Debug("Refresh.END");
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
                var arrFixedCats = groupFixedCatValues?.GetCatList().ToList() ?? new List<string>();

                if (IsShowingCategories)
                {
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
                            var itemSourceList = new List<ApiGetGroupCatValuesModel> { new ApiGetGroupCatValuesModel { Display = res.ClearSelection } }
                                .Concat(groupCatValues.ToList());
                            picker.ItemsSource = itemSourceList.ToList();
                        }
                        var localI = i;
                        picker.SelectedIndexChanged += async (o, args) =>
                        {
                            UserDialogs.Instance.ShowLoading(res.Processing);
                            //Logger.Debug($"localI={localI}");
                            var arrCatValuesLen = localI + 1;
                            if (picker.SelectedIndex <= 0) arrCatValuesLen = localI; //nothing or first element selected
                            var arrCatValues = new string[arrCatValuesLen];
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
                                if (j == localI + 1 && picker.SelectedIndex > 0)//don't get catvalues when first item selected. Let it's null
                                {
                                    var localJ = j;
                                    var groupCatValues = await App.FriendStore.GetGroupCatValues(selectedGroup.Group.Id, true, arrCatValues);
                                    var itemSourceList = new List<ApiGetGroupCatValuesModel> { new ApiGetGroupCatValuesModel { Display = res.ClearSelection } }
                                        .Concat(groupCatValues.ToList());
                                    arrPickers[localJ].ItemsSource = itemSourceList.ToList();
                                }
                            }
                            UserDialogs.Instance.HideLoading();
                            _onSelectionAction?.Invoke(selectedGroup, arrFixedCats, arrCatValues);
                        };
                        Grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        Grid.SetColumn(picker, 1);
                        Grid.SetRow(picker, i + 1);
                        Grid.Children.Add(picker);
                    }
                }

                UserDialogs.Instance.HideLoading();
                _onSelectionAction?.Invoke(selectedGroup, arrFixedCats, new string[0]);
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

        private void CmdRefresh_OnClicked(object sender, EventArgs e)
        {
            App.Initialize();

            Refresh();
        }
    }
}