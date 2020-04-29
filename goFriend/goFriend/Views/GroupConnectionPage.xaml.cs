using System;
using System.Collections.Generic;
using System.Linq;
using Acr.UserDialogs;
using goFriend.Controls;
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
        private readonly IList<SearchBar> _lstCatSearchBars = new List<SearchBar>();
        private readonly IList<string> _lstCatSearchBarsText = new List<string>();
        private IList<string> _arrFixedCatValues;
        private Group _selectedGroup;

        public GroupConnectionPage()
        {
            InitializeComponent();

            _textChangedCorrect = true;
            SbGroup.Text = Settings.LastGroupName;

            SbGroup.Focused += (s, e) =>
            {
                Navigation.PushAsync(
                    new SearchPage(res.Groups, false, async (searchText) =>
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
                            ImageSource = x.UserRight == UserType.Admin ? "group_admin.png" : "group.png",
                            ImageForeground = x.UserRight == UserType.Admin ? Color.BlueViolet :
                                x.UserRight == UserType.Normal ? (Color)Application.Current.Resources["ColorPrimary"] :
                                x.UserRight == UserType.Pending ? (Color)Application.Current.Resources["ColorPrimaryLight"] : Color.LightGray
                        });
                        //Logger.Debug($"groupList={JsonConvert.SerializeObject(groupList)}");
                        Logger.Debug("Search.Group.END");
                        return searchResult;
                    }, groupName =>
                    {
                        _textChangedCorrect = true;
                        Settings.LastGroupName = SbGroup.Text = groupName;
                    }, SbGroup.Text));
            };
        }

        private bool _textChangedCorrect; //happened with Android only. It's called when user go to another shell tab and come back to the Account tab
        private async void SbGroup_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_textChangedCorrect)
            {
                var correctTextVal = Settings.LastGroupName;
                if (e.NewTextValue != correctTextVal && e.OldTextValue == correctTextVal)
                {
                    SbGroup.Text = correctTextVal;
                    Logger.Warn("TextChanged happened unexpectedly. Group Picker's text is reset");
                    return;
                }
                else
                {
                    Logger.Warn("TextChanged happened unexpectedly. Group Picker Fixed.");
                    return;
                }
            }
            _textChangedCorrect = false; //once consumed --> set to false for not being consumed unexpectedly
            try
            {
                UserDialogs.Instance.ShowLoading(res.Processing);
                var groupName = SbGroup.Text;
                Logger.Debug($"BEGIN({groupName})");

                //remove all the grid rows
                Grid.Children.Clear();

                //Remove all the search bar after the grid
                //while (SlMain.Children[SlMain.Children.Count - 1] is SearchBar)
                //{
                //    SlMain.Children.RemoveAt(SlMain.Children.Count - 1);
                //}
                _lstCatSearchBars.ForEach(x => SlMain.Children.Remove(x));
                LblSubscriptionMsg.Text = string.Empty;
                _lstCatSearchBars.Clear();
                _lstCatSearchBarsText.Clear();
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
                CmdSubscribe.IsEnabled = false;

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
                            FontSize = (double)Application.Current.Resources["LblFontSize"],
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
                            FontSize = (double)Application.Current.Resources["LblFontSize"],
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
                        var sb = new DphSearchBar
                        {
                            Placeholder = arrCatDesc[i],
                            FontAttributes = FontAttributes.Bold,
                            Text = selectedApiGroup?.GroupFriend?.GetCatByIdx(i + 1),
                            //BackgroundColor = Color.Aqua,
                            BackgroundColor = SbGroup.BackgroundColor,
                            IsEnabled = i == _arrFixedCatValues.Count // only first SearchBar is enabled
                        };
                        sb.LastText = sb.Text;
                        if (!string.IsNullOrEmpty(sb.Text)) sb.IsEnabled = false; // search bar is disable when user is a member of Group
                        var sbIdx = i;
                        sb.TextChanged += (o, args) =>
                        {
                            if (!sb.TextChangedCorrect)
                            {
                                var correctTextVal = sb.LastText;
                                if (args.NewTextValue != correctTextVal && args.OldTextValue == correctTextVal)
                                {
                                    sb.Text = correctTextVal;
                                    Logger.Warn("TextChanged happened unexpectedly. Category Picker's text is reset");
                                    return;
                                }
                                else
                                {
                                    Logger.Warn("TextChanged happened unexpectedly. Category Picker Fixed.");
                                    return;
                                }
                            }
                            sb.TextChangedCorrect = false;
                            if (sbIdx == arrCatDesc.Count - 1)
                            {
                                CmdSubscribe.IsEnabled = sb.Text != string.Empty;
                            }
                            else
                            {
                                _lstCatSearchBars[sbIdx - _arrFixedCatValues.Count + 1].Text = string.Empty;
                                _lstCatSearchBars[sbIdx - _arrFixedCatValues.Count + 1].IsEnabled = sb.Text != string.Empty;
                            }
                        };
                        sb.Focused += (s, args) =>
                        {
                            var arrCatValues = new string[sbIdx - _arrFixedCatValues.Count];
                            for (var j = 0; j < arrCatValues.Length; j++)
                            {
                                arrCatValues[j] = _lstCatSearchBars[j].Text;
                            }
                            Navigation.PushAsync(
                                new SearchPage(arrCatDesc[sbIdx], true, async (searchText) =>
                                {
                                    Logger.Debug($"Search.{arrCatDesc[sbIdx]}.BEGIN(searchText={searchText})");

                                    var catValueList = await App.FriendStore.GetGroupCatValues(_selectedGroup.Id, true, arrCatValues);
                                    var searchResult = catValueList.Where(x => x.CatValue.IndexOf(searchText ?? string.Empty, StringComparison.CurrentCultureIgnoreCase) >= 0)
                                        .Select(x => new SearchItemModel
                                        {
                                            Text = x.CatValue,
                                            SubItemCount = x.MemberCount
                                        }).OrderBy(x => x.Text);
                                    //Logger.Debug($"searchResult={JsonConvert.SerializeObject(searchResult)}");
                                    Logger.Debug($"Search.{arrCatDesc[sbIdx]}.END");
                                    return searchResult;
                                }, selectedValue =>
                                {
                                    sb.TextChangedCorrect = true;
                                    sb.LastText = sb.Text = selectedValue;
                                }, sb.Text));
                        };
                        SlMain.Children.Insert(SlMain.Children.Count - 1, sb);
                        _lstCatSearchBars.Add(sb);
                        _lstCatSearchBarsText.Add(sb.Text);
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

                for (var i = 0; i < _lstCatSearchBars.Count; i++)
                {
                    groupFriend.SetCatByIdx(i + 1 + _arrFixedCatValues.Count, _lstCatSearchBars[i].Text);
                }

                var result = await App.FriendStore.SubscribeGroup(groupFriend);
                if (result)
                {
                    App.DisplayMsgInfo(res.MsgSubscriptionSuccessful);
                    App.Initialize();

                    _textChangedCorrect = true;
                    SbGroup_OnTextChanged(null, null);
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
            for (var i=0; i < _lstCatSearchBars.Count; i++)
            {
                _lstCatSearchBars[i].IsEnabled = false;
                _lstCatSearchBars[i].Text = _lstCatSearchBarsText[i];
            }
            CmdCancel.IsVisible = false;
            CmdSubscribe.IsVisible = false;
            CmdModify.IsVisible = true;
        }

        private async void CmdModify_OnClicked(object sender, EventArgs e)
        {
            if (!await App.DisplayMsgQuestion(res.MsgModifyConfirmation)) return;
            foreach (var sb in _lstCatSearchBars)
            {
                sb.IsEnabled = true;
            }
            CmdCancel.IsVisible = true;
            CmdSubscribe.IsVisible = true;
            CmdSubscribe.IsEnabled = true;
            CmdModify.IsVisible = false;
        }
    }
}