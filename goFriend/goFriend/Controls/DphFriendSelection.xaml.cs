using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Acr.UserDialogs;
using goFriend.DataModel;
using goFriend.Services;
using goFriend.ViewModels;
using goFriend.Views;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DphFriendSelection : ContentView
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        public static readonly BindableProperty IsShowingCategoriesProperty =
            BindableProperty.CreateAttached(nameof(IsShowingCategories), typeof(bool), typeof(DphFriendSelection), false);
        public bool IsShowingCategories
        {
            get => (bool)GetValue(IsShowingCategoriesProperty);
            set => SetValue(IsShowingCategoriesProperty, value);
        }

        public static readonly BindableProperty IsShowingEditGroupProperty =
            BindableProperty.CreateAttached(nameof(IsShowingEditGroup), typeof(bool), typeof(DphFriendSelection), false);
        public bool IsShowingEditGroup
        {
            get => (bool)GetValue(IsShowingEditGroupProperty);
            set => SetValue(IsShowingEditGroupProperty, value);
        }

        public static readonly BindableProperty IsShowingAllGroupProperty =
            BindableProperty.CreateAttached(nameof(IsShowingAllGroup), typeof(bool), typeof(DphFriendSelection), false);
        public bool IsShowingAllGroup
        {
            get => (bool)GetValue(IsShowingAllGroupProperty);
            set => SetValue(IsShowingAllGroupProperty, value);
        }

        public static readonly BindableProperty IsExpandableCategoriesProperty =
            BindableProperty.CreateAttached(nameof(IsExpandableCategories), typeof(bool), typeof(DphFriendSelection), true);
        public bool IsExpandableCategories
        {
            get => (bool)GetValue(IsExpandableCategoriesProperty);
            set => SetValue(IsExpandableCategoriesProperty, value);
        }

        public static readonly BindableProperty IsShowingRefreshProperty =
            BindableProperty.CreateAttached(nameof(IsShowingRefresh), typeof(bool), typeof(DphFriendSelection), true);
        public bool IsShowingRefresh
        {
            get => (bool)GetValue(IsShowingRefreshProperty);
            set => SetValue(IsShowingRefreshProperty, value);
        }

        public string SelectedGroupName { get; set; }

        //search criteria
        public MyGroupViewModel SelectedGroup { get; private set; }
        public List<string> ArrFixedCats { get; private set; }
        private string[] _arrCatValues;
        private bool _isSelectionEventEnabled = true;

        public DphFriendSelection()
        {
            InitializeComponent();

            BindingContext = this;

            PickerGroups.Title = $"{res.Select} {res.Groups}";
            LblGroup.Text = $"{res.Groups}:";
            LblGroup.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => CmdCategories_OnClicked(null, null))
            });
            EntryName.ReturnCommand = new Command(() =>
            {
                if (SelectedGroup == null) return;
                _onSelectionAction?.Invoke(SelectedGroup, EntryName.Text, ArrFixedCats, _arrCatValues);
            });
        }

        private Action<MyGroupViewModel, string, List<string>, string[]> _onSelectionAction;
        public void Initialize(Action<MyGroupViewModel, string, List<string>, string[]> onSelectionAction)
        {
            _onSelectionAction = onSelectionAction;

            Refresh();
        }

        public void Refresh()
        {
            Logger.Debug("Refresh.BEGIN");
            UserDialogs.Instance.ShowLoading(res.Processing);
            //can not await TaskInitialization because we are in a constructor, and not in an async method.
            App.TaskInitialization.Wait();
                var myGroups = App.MyGroups == null ? new List<MyGroupViewModel>() : App.MyGroups.Where(x => x.GroupFriend.Active).ToList();
            var oc = new ObservableCollection<MyGroupViewModel>(myGroups);
            if (IsShowingAllGroup)
            {
                oc.Insert(0, new MyGroupViewModel
                {
                    Group = null,
                    GroupFriend = null,
                    ChatOwnerId = 0
                });
                PickerGroups.ItemsSource = oc;
                UserDialogs.Instance.HideLoading();//must be called before setting SelectedIndex
                PickerGroups.SelectedIndex = 0;
                return;
            }
            PickerGroups.ItemsSource = oc;
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
                    Shell.Current.GoToAsync(Constants.ROUTE_HOME_GROUPCONNECTION);
                    //Navigation.PushAsync(new GroupConnectionPage());
                }
        }

        private const int DynamicRowStartIndex = 2;
        private async void PickerGroups_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Logger.Debug($"PickerGroups_OnSelectedIndexChanged.BEGIN(SelectedIndex={PickerGroups.SelectedIndex}, SelectedItem={(PickerGroups.SelectedItem as MyGroupViewModel)?.Group?.Name})");
                UserDialogs.Instance.ShowLoading(res.Processing);
                SelectedGroup = PickerGroups.SelectedItem as MyGroupViewModel;
                CmdEditGroup.IsVisible = IsShowingEditGroup && SelectedGroup != null && SelectedGroup.ChatOwnerId.HasValue && SelectedGroup.ChatOwnerId.Value == App.User.Id;
                if (SelectedGroup == null) return;
                var rowToRemove = Grid.Children.Where(x => Grid.GetRow(x) >= DynamicRowStartIndex).ToList();
                foreach (var child in rowToRemove)
                {
                    Grid.Children.Remove(child);
                }
                while (Grid.RowDefinitions.Count > DynamicRowStartIndex)
                {
                    Grid.RowDefinitions.RemoveAt(DynamicRowStartIndex);
                }

                if (SelectedGroup.Group == null) // All Groups
                {
                    UserDialogs.Instance.HideLoading();
                    EntryName.ReturnCommand.Execute(null);
                    return;
                }

                SelectedGroupName = SelectedGroup.Group.Name;
                var groupFixedCatValues = await App.FriendStore.GetGroupFixedCatValues(SelectedGroup.Group.Id);
                var arrCats = SelectedGroup.Group.GetCatDescList().ToList();
                ArrFixedCats = groupFixedCatValues == null ? new List<string>() : groupFixedCatValues.GetCatList().ToList();

                if (IsExpandableCategories || IsShowingCategories)
                {
                    var arrPickers = new Picker[arrCats.Count - ArrFixedCats.Count];
                    for (var i = 0; i < arrCats.Count - ArrFixedCats.Count; i++)
                    {
                        //Logger.Debug($"i={i}");
                        var lbl = new Label
                        {
                            //FontSize = LblGroup.FontSize,
                            FontSize = Device.GetNamedSize(NamedSize.Caption, typeof(Label)),
                            VerticalOptions = LblGroup.VerticalOptions,
                            TextColor = LblName.TextColor,
                            IsVisible = IsShowingCategories,
                            Text = $"{arrCats[i + ArrFixedCats.Count]}:"
                        };
                        Grid.SetColumn(lbl, 0);
                        Grid.SetRow(lbl, i + DynamicRowStartIndex);
                        Grid.Children.Add(lbl);
                        var picker = new Picker
                        {
                            //FontSize = PickerGroups.FontSize,
                            //HeightRequest = PickerGroups.HeightRequest,
                            FontSize = Device.GetNamedSize(NamedSize.Caption, typeof(Picker)),
                            //ItemDisplayBinding = new Binding("Display"),
                            ItemDisplayBinding = PickerGroups.ItemDisplayBinding,
                            IsVisible = IsShowingCategories,
                            Title = $"{res.Select} {arrCats[i + ArrFixedCats.Count]}"
                        };
                        arrPickers[i] = picker;
                        if (i == 0)
                        {
                            var groupCatValues = await App.FriendStore.GetGroupCatValues(SelectedGroup.Group.Id);
                            var itemSourceList = new List<ApiGetGroupCatValuesModel> { new ApiGetGroupCatValuesModel { Display = res.ClearSelection } }
                                .Concat(groupCatValues.ToList());
                            picker.ItemsSource = itemSourceList.ToList();
                        }
                        var localI = i;
                        picker.SelectedIndexChanged += async (o, args) =>
                        {
                            if (!_isSelectionEventEnabled) return;;
                            try
                            {
                                _isSelectionEventEnabled = false; // Do not fire selection event of the picker children
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
                                    //Logger.Debug($"j={j}");
                                    if (j == localI + 1 && picker.SelectedIndex > 0)//don't get catvalues when first item selected. Let it's null
                                    {
                                        var localJ = j;
                                        var groupCatValues = await App.FriendStore.GetGroupCatValues(SelectedGroup.Group.Id, true, true, _arrCatValues);
                                        var itemSourceList = new List<ApiGetGroupCatValuesModel> { new ApiGetGroupCatValuesModel { Display = res.ClearSelection } }
                                            .Concat(groupCatValues.ToList());
                                        arrPickers[localJ].ItemsSource = itemSourceList.ToList();
                                    }
                                    else
                                    {
                                        arrPickers[j].ItemsSource = null;
                                    }
                                }
                                UserDialogs.Instance.HideLoading();
                                EntryName.ReturnCommand.Execute(null);
                                _isSelectionEventEnabled = true; // Restore the initial value
                                //_onSelectionAction?.Invoke(_selectedGroup, EntryName.Text, _arrFixedCats, _arrCatValues);
                            }
                            catch(Exception ex)
                            {
                                Logger.Error(ex.ToString());
                            }
                            finally
                            {
                                UserDialogs.Instance.HideLoading();
                            }
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
                UserDialogs.Instance.HideLoading();
                Logger.Debug("PickerGroups_OnSelectedIndexChanged.END");
            }
        }

        private void CmdEditGroup_OnClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new GroupEdit(SelectedGroup.Group.Id));
        }

        private void CmdRefresh_OnClicked(object sender, EventArgs e)
        {
            App.Initialize();

            Refresh();
        }

        private void CmdCategories_OnClicked(object sender, EventArgs e)
        {
            if (!IsExpandableCategories) return;
            IsShowingCategories = !IsShowingCategories;
            foreach (var child in Grid.Children.Where(x => Grid.GetRow(x) >= DynamicRowStartIndex))
            {
                child.IsVisible = IsShowingCategories;
            }
            CmdCategoriesOpen.IsVisible = IsShowingCategories;
            CmdCategoriesClose.IsVisible = !IsShowingCategories;
        }
    }
}