using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using goFriend.DataModel;
using goFriend.Services;
using goFriend.Views;
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
            BindableProperty.CreateAttached(nameof(IsShowingCategories), typeof(bool), typeof(DphFriendSelection), false);
        public bool IsShowingCategories
        {
            get => (bool)GetValue(IsShowingCategoriesProperty);
            set => SetValue(IsShowingCategoriesProperty, value);
        }

        public static readonly BindableProperty IsExpandableCategoriesProperty =
            BindableProperty.CreateAttached(nameof(IsExpandableCategories), typeof(bool), typeof(DphFriendSelection), true);
        public bool IsExpandableCategories
        {
            get => (bool)GetValue(IsExpandableCategoriesProperty);
            set => SetValue(IsExpandableCategoriesProperty, value);
        }

        public string SelectedGroupName { get; set; }

        //search criteria
        private ApiGetGroupsModel _selectedGroup;
        private List<string> _arrFixedCats;
        private string[] _arrCatValues;

        public DphFriendSelection()
        {
            InitializeComponent();

            PickerGroups.Title = $"{res.Select} {res.Groups}";
            LblGroup.Text = $"{res.Groups}:";
            LblGroup.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => CmdExpandCategories_OnClicked(null, null))
            });
            EntryName.ReturnCommand = new Command(() =>
            {
                if (_selectedGroup == null) return;
                _onSelectionAction?.Invoke(_selectedGroup, EntryName.Text, _arrFixedCats, _arrCatValues);
            });

            Refresh();
        }

        private Action<ApiGetGroupsModel, string, List<string>, string[]> _onSelectionAction;
        public void Initialize(Action<ApiGetGroupsModel, string, List<string>, string[]> onSelectionAction)
        {
            _onSelectionAction = onSelectionAction;
        }

        public void Refresh()
        {
            Logger.Debug("Refresh.BEGIN");
            UserDialogs.Instance.ShowLoading(res.Processing);
            //can not await TaskInitialization because we are in a constructor, and not in an async method.
            Task.Run(() =>
            {
                App.TaskInitialization.Wait();
                //Task.Delay(5000).Wait();
            }).ContinueWith(task =>
            {
                var myGroups = App.MyGroups == null ? new List<ApiGetGroupsModel>() : App.MyGroups.Where(x => x.GroupFriend.Active).OrderBy(x => x.Group.Name).ToList();
                PickerGroups.ItemsSource = new ObservableCollection<ApiGetGroupsModel>(myGroups);
                UserDialogs.Instance.HideLoading();//must be called before setting SelectedIndex
                for (var i = 0; i < PickerGroups.Items.Count; i++)
                {
                    if (PickerGroups.Items[i].StartsWith($"{SelectedGroupName} ("))
                    {
                        PickerGroups.SelectedIndex = i;
                        break;
                    }
                }
                if (PickerGroups.SelectedIndex < 0 && PickerGroups.Items.Count > 0)
                {
                    PickerGroups.SelectedIndex = 0;
                }
                Logger.Debug("Refresh.END");
                if (myGroups.Count == 0)
                {
                    App.DisplayMsgInfo(res.MsgNoGroupWarning);
                    Navigation.PushAsync(new GroupConnectionPage());
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private const int DynamicRowStartIndex = 2;
        private async void PickerGroups_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Logger.Debug($"PickerGroups_OnSelectedIndexChanged.BEGIN(SelectedIndex={PickerGroups.SelectedIndex}, SelectedItem={(PickerGroups.SelectedItem as ApiGetGroupsModel)?.Group?.Name})");
                UserDialogs.Instance.ShowLoading(res.Processing);
                _selectedGroup = PickerGroups.SelectedItem as ApiGetGroupsModel;
                if (_selectedGroup == null) return;
                SelectedGroupName = _selectedGroup.Group.Name;
                var rowToRemove = Grid.Children.Where(x => Grid.GetRow(x) >= DynamicRowStartIndex).ToList();
                foreach (var child in rowToRemove)
                {
                    Grid.Children.Remove(child);
                }
                while (Grid.RowDefinitions.Count > DynamicRowStartIndex)
                {
                    Grid.RowDefinitions.RemoveAt(DynamicRowStartIndex);
                }
                var groupFixedCatValues = await App.FriendStore.GetGroupFixedCatValues(_selectedGroup.Group.Id);
                var arrCats = _selectedGroup.Group.GetCatDescList().ToList();
                _arrFixedCats = groupFixedCatValues?.GetCatList().ToList() ?? new List<string>();

                if (IsExpandableCategories || IsShowingCategories)
                {
                    var arrPickers = new Picker[arrCats.Count - _arrFixedCats.Count];
                    for (var i = 0; i < arrCats.Count - _arrFixedCats.Count; i++)
                    {
                        //Logger.Debug($"i={i}");
                        var lbl = new Label
                        {
                            FontSize = LblGroup.FontSize,
                            VerticalOptions = LblGroup.VerticalOptions,
                            TextColor = LblGroup.TextColor,
                            IsVisible = IsShowingCategories,
                            Text = $"{arrCats[i + _arrFixedCats.Count]}:"
                        };
                        Grid.SetColumn(lbl, 0);
                        Grid.SetRow(lbl, i + DynamicRowStartIndex);
                        Grid.Children.Add(lbl);
                        var picker = new Picker
                        {
                            FontSize = PickerGroups.FontSize,
                            HeightRequest = PickerGroups.HeightRequest,
                            //ItemDisplayBinding = new Binding("Display"),
                            ItemDisplayBinding = PickerGroups.ItemDisplayBinding,
                            IsVisible = IsShowingCategories,
                            Title = $"{res.Select} {arrCats[i + _arrFixedCats.Count]}"
                        };
                        arrPickers[i] = picker;
                        if (i == 0)
                        {
                            var groupCatValues = await App.FriendStore.GetGroupCatValues(_selectedGroup.Group.Id);
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
                            _arrCatValues = new string[arrCatValuesLen];
                            for (var k = 0; k < _arrCatValues.Length; k++)
                            {
                                //Logger.Debug($"arrPickers[{k}].SelectedItem={arrPickers[k].SelectedItem}");
                                _arrCatValues[k] = (arrPickers[k].SelectedItem as ApiGetGroupCatValuesModel)?.CatValue;
                            }
                            Logger.Debug($"arrCatValues={JsonConvert.SerializeObject(_arrCatValues)}");
                            for (var j = localI + 1; j < arrPickers.Length; j++)
                            {
                                arrPickers[j].ItemsSource = null;
                                //Logger.Debug($"j={j}");
                                if (j == localI + 1 && picker.SelectedIndex > 0)//don't get catvalues when first item selected. Let it's null
                                {
                                    var localJ = j;
                                    var groupCatValues = await App.FriendStore.GetGroupCatValues(_selectedGroup.Group.Id, true, _arrCatValues);
                                    var itemSourceList = new List<ApiGetGroupCatValuesModel> { new ApiGetGroupCatValuesModel { Display = res.ClearSelection } }
                                        .Concat(groupCatValues.ToList());
                                    arrPickers[localJ].ItemsSource = itemSourceList.ToList();
                                }
                            }
                            UserDialogs.Instance.HideLoading();
                            EntryName.ReturnCommand.Execute(null);
                            //_onSelectionAction?.Invoke(_selectedGroup, EntryName.Text, _arrFixedCats, _arrCatValues);
                        };
                        Grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        Grid.SetColumn(picker, 1);
                        Grid.SetRow(picker, i + DynamicRowStartIndex);
                        Grid.Children.Add(picker);
                    }
                }

                UserDialogs.Instance.HideLoading();
                _arrCatValues = new string[0];
                EntryName.ReturnCommand.Execute(null);
                //_onSelectionAction?.Invoke(_selectedGroup, EntryName.Text, _arrFixedCats, _arrCatValues);
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

        private void CmdExpandCategories_OnClicked(object sender, EventArgs e)
        {
            IsShowingCategories = !IsShowingCategories;
            CmdExpandCategories.ImageSource = IsShowingCategories ? "folder_open.png" : "folder_close.png";
            foreach (var child in Grid.Children.Where(x => Grid.GetRow(x) >= DynamicRowStartIndex))
            {
                child.IsVisible = IsShowingCategories;
            }
        }
    }
}