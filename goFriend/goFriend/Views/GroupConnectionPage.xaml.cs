using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
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
        private readonly IList<SearchBar> _lstCatSearchBars = new List<SearchBar>();
        private IList<string> _arrFixedCatValues;
        private Group _selectedGroup;

        public GroupConnectionPage()
        {
            InitializeComponent();

            SbGroup.Text = Settings.LastGroupName;

            SbGroup.Focused += (s, e) =>
            {
                Navigation.PushAsync(
                    new SearchPage(res.Groups, false, async (searchText) =>
                    {
                        Logger.Debug($"Search.Group.BEGIN(searchText={searchText})");
                        await App.TaskGetMyGroups;
                        Logger.Debug("Get all groups");
                        App.AllGroups = await App.FriendStore.GetGroups(searchText);
                        var searchResult = App.MyGroups.Where(x => x.Group.Name.Contains(searchText)).Union(App.AllGroups.Where(
                            x => App.MyGroups.All(y => y.Group.Id != x.Group.Id))).Select(x => new SearchItemModel
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
                    }, groupName => { SbGroup.Text = groupName; }, SbGroup.Text));
            };
        }

        private async void SbGroup_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                UserDialogs.Instance.ShowLoading(res.Processing);
                var groupName = SbGroup.Text;
                Logger.Debug($"BEGIN({groupName})");

                //remove all the grid rows
                Grid.Children.Clear();
                //var rowToRemove = Grid.Children.Where(x => Grid.GetRow(x) >= 0).ToList();
                //foreach (var child in rowToRemove)
                //{
                //    Grid.Children.Remove(child);
                //}

                //Remove all the search bar after the grid
                //while (SlMain.Children[SlMain.Children.Count - 1] is SearchBar)
                //{
                //    SlMain.Children.RemoveAt(SlMain.Children.Count - 1);
                //}
                _lstCatSearchBars.ForEach(x => SlMain.Children.Remove(x));
                LblSubscriptionMsg.Text = string.Empty;
                _lstCatSearchBars.Clear();
                LblInfoCats.Text = res.Information;
                _arrFixedCatValues = null;

                CmdSave.IsVisible = !string.IsNullOrEmpty(groupName);
                if (string.IsNullOrEmpty(groupName)) return;

                Settings.LastGroupName = groupName;

                Logger.Debug("Before Wait");
                await App.TaskGetMyGroups; //Make sure everything is initialized. (MyGroups)
                Logger.Debug($"After Wait: {App.MyGroups?.Count()}");

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
                CmdSave.IsEnabled = selectedApiGroup?.UserRight != UserType.NotMember;

                _selectedGroup = selectedApiGroup?.Group;

                if (_selectedGroup == null)
                {
                    App.DisplayMsgError(Message.MsgGroupNotFound.Msg);
                    return;
                }

                var groupFixedCatValues = await App.FriendStore.GetGroupFixedCatValues(_selectedGroup.Id);

                _arrFixedCatValues = groupFixedCatValues.GetCatList().ToList();
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
                        var sb = new SearchBar
                        {
                            Placeholder = arrCatDesc[i],
                            FontAttributes = FontAttributes.Bold,
                            Text = selectedApiGroup?.GroupFriend?.GetCatByIdx(i + 1),
                            //BackgroundColor = Color.Aqua,
                            BackgroundColor = SbGroup.BackgroundColor,
                            IsEnabled = i == _arrFixedCatValues.Count // only first SearchBar is enabled
                        };
                        if (sb.Text != string.Empty) sb.IsEnabled = false; // search bar is disable the user's Group
                        var sbIdx = i;
                        sb.TextChanged += (o, args) =>
                        {
                            if (sbIdx == arrCatDesc.Count - 1)
                            {
                                CmdSave.IsEnabled = sb.Text != string.Empty;
                            }
                            else
                            {
                                _lstCatSearchBars[sbIdx - _arrFixedCatValues.Count + 1].Text = string.Empty;
                                _lstCatSearchBars[sbIdx - _arrFixedCatValues.Count + 1].IsEnabled = sb.Text != string.Empty;
                            }
                        };
                        sb.Focused += (s, args) =>
                        {
                            var arrCatValues = new string[sbIdx - _arrFixedCatValues.Count + 1];
                            for (var j = _arrFixedCatValues.Count; j < sbIdx; j++)
                            {
                                arrCatValues[j - _arrFixedCatValues.Count] = $"Cat{j + 1}={HttpUtility.UrlEncode(_lstCatSearchBars[j - _arrFixedCatValues.Count].Text)}";
                            }
                            Navigation.PushAsync(
                                new SearchPage(arrCatDesc[sbIdx], true, async (searchText) =>
                                {
                                    Logger.Debug($"Search.{arrCatDesc[sbIdx]}.BEGIN(searchText={searchText})");
                                    arrCatValues[sbIdx - _arrFixedCatValues.Count] = $"SearchText={HttpUtility.UrlEncode(searchText)}";

                                    var catValueList = await App.FriendStore.GetGroupCatValues(_selectedGroup.Id, true, arrCatValues);
                                    var searchResult = catValueList.Where(x =>
                                            CultureInfo.CurrentCulture.CompareInfo.IndexOf(x.CatValue, searchText, CompareOptions.IgnoreCase) >= 0)
                                        .Select(x => new SearchItemModel
                                        {
                                            Text = x.CatValue,
                                            SubItemCount = x.MemberCount
                                        }).OrderBy(x => x.Text);
                                    //Logger.Debug($"searchResult={JsonConvert.SerializeObject(searchResult)}");
                                    Logger.Debug($"Search.{arrCatDesc[sbIdx]}.END");
                                    return searchResult;
                                }, selectedValue => { sb.Text = selectedValue; }, sb.Text));
                        };
                        SlMain.Children.Insert(SlMain.Children.Count - 1, sb);
                        _lstCatSearchBars.Add(sb);
                    }
                }

                if (selectedApiGroup != null)
                {
                    switch (selectedApiGroup.UserRight)
                    {
                        case UserType.Pending:
                            LblSubscriptionMsg.Text = res.MsgGroupSubscriptionPending;
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

        private async void CmdSave_OnClicked(object sender, EventArgs e)
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
                    App.DisplayMsgInfo("Subscription successful");
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
    }
}