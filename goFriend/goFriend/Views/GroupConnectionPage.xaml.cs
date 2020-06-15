using System;
using System.Collections.Generic;
using System.Linq;
using Acr.UserDialogs;
using goFriend.DataModel;
using goFriend.Services;
using goFriend.ViewModels;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GroupConnectionPage : ContentPage
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();
        private readonly IList<StackLayout> _lstCatEntries = new List<StackLayout>();
        private readonly IList<string> _lstCatEntriesText = new List<string>();
        private IList<string> _arrFixedCatValues;
        private Group _selectedGroup;
        private bool resetWhenAppearing = true;

        public GroupConnectionPage()
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.Android)
            {
                Appearing += (sender, args) =>
                {
                    //for (var i = 0; i < _lstCatEntries.Count; i++)
                    //{
                    //    var entry = (Entry)_lstCatEntries[i].Children[0];
                    //    entry.Text = _lstCatEntriesText[i];
                    //}
                    if (resetWhenAppearing)
                    {
                        DphGroupSearch.Reset();
                    }
                    else
                    {
                        resetWhenAppearing = true;
                    }
                };
            }

            //Disappearing += (sender, args) =>
            //{
            //Reset the form because of an unexpected behavior
            //(all categories got the value of the last category, don't know why?)
            //Ex: Chuyen DHTH, Toan A --> Toan A, Toan A
            //DphGroupSearch.Reset();
            //};

            DphGroupSearch.Initialize(async (searchText) =>
            {
                Logger.Debug($"Search.Group.BEGIN(searchText={searchText})");
                App.TaskInitialization.Wait();
                App.AllGroups = await App.FriendStore.GetGroups(searchText);
                var searchResult = App.MyGroups.Where(
                    x => x.Group.Name.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) >= 0).Union(
                    App.AllGroups.Where(x => App.MyGroups.All(y => y.Group.Id != x.Group.Id))).Select(
                    x => new SearchItemModel
                    {
                        Text = x.Group.Name,
                        Description = x.Group.Desc,
                        Id = x.Group.Id,
                        ItemType = (int)x.UserRight,
                        SubItemCount = x.MemberCount,
                        ImageSource = x.UserRight == UserType.Admin ? Constants.ImgGroupAdmin : Constants.ImgGroup,
                        ImageForeground = x.UserRight == UserType.Admin ? Color.BlueViolet :
                        x.UserRight == UserType.Normal ? (Color)Application.Current.Resources["ColorPrimary"] :
                        x.UserRight == UserType.Pending ? (Color)Application.Current.Resources["ColorPrimaryLight"] : Color.LightGray
                    });
                //Logger.Debug($"groupList={JsonConvert.SerializeObject(groupList)}");
                Logger.Debug("Search.Group.END");
                return searchResult;
            }, SelectItemAction, () => SlGroupDetail.IsVisible = false);
        }

        private async void SelectItemAction(string groupName)
        {
            try
            {
                UserDialogs.Instance.ShowLoading(res.Processing);
                Logger.Debug($"BEGIN({groupName})");
                SlGroupDetail.IsVisible = true;

                //remove all the grid rows
                Grid.Children.Clear();

                //Remove all the search bar after the grid
                //while (SlMain.Children[SlMain.Children.Count - 1] is SearchBar)
                //{
                //    SlMain.Children.RemoveAt(SlMain.Children.Count - 1);
                //}
                _lstCatEntries.ForEach(x => SlMain.Children.Remove(x));
                LblSubscriptionMsg.Text = string.Empty;
                _lstCatEntries.Clear();
                _lstCatEntriesText.Clear();
                LblInfoCats.Text = res.Information;
                _arrFixedCatValues = null;

                CmdSubscribe.IsVisible = CmdModify.IsVisible = CmdCancel.IsVisible = false;

                if (string.IsNullOrEmpty(groupName)) return;

                //Logger.Debug("Before Wait");
                //App.TaskInitialization.Wait(); // Do not use await here. that will block this thread but return the control to the parent thread
                await App.TaskInitialization; // await will block this thread but return the control to the parent thread, so the processing dialog can be seen.
                //Logger.Debug($"MyGroups={JsonConvert.SerializeObject(App.MyGroups)}");
                //Logger.Debug($"After Wait: {App.MyGroups?.Count()}");

                var selectedApiGroup = App.MyGroups?.SingleOrDefault(x => x.Group.Name == groupName);
                if (selectedApiGroup == null)
                {
                    if (App.AllGroups == null)
                    {
                        App.AllGroups = await App.FriendStore.GetGroups(groupName);
                    }
                    selectedApiGroup = App.AllGroups?.SingleOrDefault(x => x.Group.Name == groupName);
                }

                Logger.Debug($"selectedApiGroup={JsonConvert.SerializeObject(selectedApiGroup)}");
                CmdModify.IsVisible = selectedApiGroup?.UserRight == UserType.Pending; // Modify button visible when subscription is in pending
                CmdSubscribe.IsVisible = selectedApiGroup?.UserRight == UserType.NotMember;

                _selectedGroup = selectedApiGroup?.Group;

                if (_selectedGroup == null)
                {
                    App.DisplayMsgError(Message.MsgGroupNotFound.Msg);
                    return;
                }

                var groupFixedCatValues = await App.FriendStore.GetGroupFixedCatValues(_selectedGroup.Id);
                _arrFixedCatValues = groupFixedCatValues == null ? new List<string>() : groupFixedCatValues.GetCatList().ToList();

                Logger.Debug($"groupFixedCatValues.GetCatList.Count = {_arrFixedCatValues.Count}");
                CommonConnectionInfoLine.IsVisible = CommonConnectionInfoLayout.IsVisible = _arrFixedCatValues.Count > 0;

                var arrCatDesc = _selectedGroup.GetCatDescList().ToList();
                LblInfoCats.Text = $"{LblInfoCats.Text} {string.Join(", ", arrCatDesc.ToArray(), _arrFixedCatValues.Count, arrCatDesc.Count - _arrFixedCatValues.Count).ToUpper()}";

                for (var i = 0; i < arrCatDesc.Count; i++)
                {
                    if (i < _arrFixedCatValues.Count)
                    {
                        var lbl = new Label
                        {
                            VerticalOptions = LayoutOptions.Center,
                            //FontSize = (double)Application.Current.Resources["LblDetailFontSize"],
                            //FontSize = (double)Application.Current.Resources["LblFontSize"],
                            FontSize = (double)Application.Current.Resources["LblSectionFontSize"],
                            //TextColor = (Color)Application.Current.Resources["ColorLabelDetail"],
                            TextColor = (Color)Application.Current.Resources["ColorLabel"],
                            Text = arrCatDesc[i] + ":"
                        };
                        Grid.SetColumn(lbl, 0);
                        Grid.SetRow(lbl, i);
                        Grid.Children.Add(lbl);
                        var lblVal = new Label
                        {
                            VerticalOptions = LayoutOptions.Center,
                            //FontSize = (double)Application.Current.Resources["LblFontSize"],
                            FontSize = (double)Application.Current.Resources["LblSectionFontSize"],
                            TextColor = (Color)Application.Current.Resources["ColorLabel"],
                            //FontAttributes = FontAttributes.Bold,
                            //HorizontalTextAlignment = TextAlignment.End,
                            Text = _arrFixedCatValues[i]
                        };
                        Grid.SetColumn(lblVal, 1);
                        Grid.SetRow(lblVal, i);
                        Grid.Children.Add(lblVal);
                    }
                    else
                    {
                        var idx = i;
                        var entry = new Entry
                        {
                            Placeholder = arrCatDesc[i],
                            Text = selectedApiGroup?.GroupFriend?.GetCatByIdx(i + 1),
                            PlaceholderColor = (Color)Application.Current.Resources["ColorLabel"],
                            TextColor = (Color)Application.Current.Resources["ColorPrimary"],
                            ClearButtonVisibility = ClearButtonVisibility.WhileEditing,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                        };
                        entry.IsEnabled = string.IsNullOrEmpty(entry.Text);
                        var button = new ImageButton
                        {
                            Source = Constants.ImgSearch,
                            BackgroundColor = BackgroundColor,
                            Aspect = Aspect.AspectFit,
                            WidthRequest = 32,
                            HeightRequest = 32,
                            IsEnabled = entry.IsEnabled,
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.End,
                        };
                        // IsEnabled doesn't work with Command, but work properly with Clicked event.
                        button.Clicked += (s, e) =>
                        {
                            var arrCatValues = new string[idx - _arrFixedCatValues.Count];
                            for (var j = 0; j < arrCatValues.Length; j++)
                            {
                                arrCatValues[j] = ((Entry)_lstCatEntries[j].Children[0]).Text;
                            }
                            Navigation.PushAsync(
                                new SearchPage(arrCatDesc[idx], false, async (searchText) =>
                                {
                                    Logger.Debug($"Search.{arrCatDesc[idx]}.BEGIN(searchText={searchText})");

                                    var catValueList = await App.FriendStore.GetGroupCatValues(_selectedGroup.Id, true, arrCatValues);
                                    var searchResult = catValueList.Where(x => x.CatValue.IndexOf(searchText ?? string.Empty, StringComparison.CurrentCultureIgnoreCase) >= 0)
                                        .Select(x => new SearchItemModel
                                        {
                                            Text = x.CatValue,
                                            SubItemCount = x.MemberCount
                                        }).OrderBy(x => x.Text);
                                        //Logger.Debug($"searchResult={JsonConvert.SerializeObject(searchResult)}");
                                        Logger.Debug($"Search.{arrCatDesc[idx]}.END");
                                    return searchResult;
                                }, selectedValue =>
                                {
                                    resetWhenAppearing = false;
                                    entry.Text = selectedValue;
                                    CmdSubscribe.Focus();
                                }/*, entry.Text*/));
                        };
                        var sl = new StackLayout
                        {
                            Orientation = StackOrientation.Horizontal,
                            Margin = new Thickness(5,0),
                            Children = { entry, button }
                        };
                        SlMain.Children.Insert(SlMain.Children.Count - 1, sl);
                        _lstCatEntries.Add(sl);
                        _lstCatEntriesText.Add(entry.Text);
                    }
                }

                if (selectedApiGroup != null)
                {
                    switch (selectedApiGroup.UserRight)
                    {
                        case UserType.NotMember:
                            LblSubscriptionMsg.Text = res.MsgGroupSubscriptionNotMember;
                            break;
                        case UserType.Pending:
                            LblSubscriptionMsg.Text = res.MsgGroupSubscriptionPending;
                            break;
                        case UserType.Normal:
                            LblSubscriptionMsg.Text = res.MsgGroupSubscriptionNormal;
                            break;
                        case UserType.Admin:
                            LblSubscriptionMsg.Text = res.MsgGroupSubscriptionAdmin;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                App.DisplayMsgError(ex.Message);
                Logger.Error(ex.ToString());
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
                Logger.Debug($"END");
            }
        }

        private async void CmdSubscribe_OnClicked(object sender, EventArgs e)
        {
            try
            {
                UserDialogs.Instance.ShowLoading(res.Processing);
                Logger.Debug("BEGIN");

                var groupFriend = new GroupFriend
                {
                    GroupId = _selectedGroup.Id,
                    FriendId = App.User.Id
                };
                for (var i = 0; i < _arrFixedCatValues.Count; i++)
                {
                    groupFriend.SetCatByIdx(i + 1, _arrFixedCatValues[i]);
                }

                for (var i = 0; i < _lstCatEntries.Count; i++)
                {
                    var entry = (Entry)_lstCatEntries[i].Children[0];
                    if (string.IsNullOrWhiteSpace(entry.Text))
                    {
                        App.DisplayMsgError(string.Format(res.MsgDataRequired, entry.Placeholder));
                        return;
                    }
                    groupFriend.SetCatByIdx(i + 1 + _arrFixedCatValues.Count, entry.Text);
                }

                var result = await App.FriendStore.SubscribeGroup(groupFriend);
                if (result)
                {
                    App.DisplayMsgInfo(res.MsgSubscriptionSuccessful);
                    App.Initialize();

                    SelectItemAction(_selectedGroup.Name);
                }
            }
            catch (Exception ex)
            {
                App.DisplayMsgError(ex.Message);
                Logger.Error(ex.ToString());
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
                Logger.Debug("END");
            }
        }

        private async void CmdCancel_OnClicked(object sender, EventArgs e)
        {
            if (!await App.DisplayMsgQuestion(res.MsgCancelConfirmation)) return;
            for (var i=0; i < _lstCatEntries.Count; i++)
            {
                _lstCatEntries[i].Children.ForEach(x => x.IsEnabled = false);
                ((Entry)_lstCatEntries[i].Children[0]).Text = _lstCatEntriesText[i];
            }
            CmdCancel.IsVisible = false;
            CmdSubscribe.IsVisible = false;
            CmdModify.IsVisible = true;
        }

        private async void CmdModify_OnClicked(object sender, EventArgs e)
        {
            if (!await App.DisplayMsgQuestion(res.MsgModifyConfirmation)) return;
            _lstCatEntries.ForEach(x => x.Children.ForEach(y => y.IsEnabled = true));
            CmdCancel.IsVisible = true;
            CmdSubscribe.IsVisible = true;
            CmdSubscribe.IsEnabled = true;
            CmdModify.IsVisible = false;
        }
    }
}