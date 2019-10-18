using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Acr.UserDialogs;
using goFriend.DataModel;
using goFriend.Services;
using goFriend.ViewModels;
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
        private GroupConnectionStatus _groupConnectionStatus = GroupConnectionStatus.NoGroupSelected;

        private enum GroupConnectionStatus
        {
            NoGroupSelected,
            NotMember,
            ActiveMember,
            InactiveMember
        }

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
                        App.InitTask.Wait();
                        var searchResult = App.GroupModels.Select(x => new SearchItemModel
                        {
                            Text = x.Group.Name,
                            Description = x.Group.Desc,
                            Id = x.Group.Id,
                            ItemType = x.IsActiveMember ? 0 : x.IsMember ? 1 : 2,
                            SubItemCount = x.MemberCount,
                            ImageSource = x.UserRight == UserType.Admin ? "group_admin.png" : "group.png",
                            ImageForeground = x.UserRight == UserType.Admin ? Color.BlueViolet :
                                (x.IsActiveMember ? (Color)Application.Current.Resources["ColorPrimary"] :
                                    (x.IsMember ? (Color)Application.Current.Resources["ColorPrimaryLight"] : Color.LightGray))
                        }).OrderBy(x => x.ItemType);
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
                _lstCatSearchBars.Clear();

                CmdSave.IsVisible = !string.IsNullOrEmpty(groupName);
                if (string.IsNullOrEmpty(groupName))
                {
                    _groupConnectionStatus = GroupConnectionStatus.NoGroupSelected;
                    return;
                }
                Settings.LastGroupName = groupName;

                Logger.Debug("Before Wait");
                App.InitTask.Wait(); //Make sure everything is initialized. (GroupModels)
                Logger.Debug($"After Wait: {App.GroupModels}");

                var apiGetGroup = App.GroupModels.Single(x => x.Group.Name == groupName);
                _groupConnectionStatus = apiGetGroup.IsActiveMember ? GroupConnectionStatus.ActiveMember
                    : apiGetGroup.IsMember ? GroupConnectionStatus.InactiveMember : GroupConnectionStatus.NotMember;
                CmdSave.IsEnabled = _groupConnectionStatus == GroupConnectionStatus.ActiveMember ||
                                    _groupConnectionStatus == GroupConnectionStatus.InactiveMember;

                var selectedGroup = apiGetGroup.Group;
                Logger.Debug($"After get Selected Group");

                var groupFixedCatValues = await App.FriendStore.GetGroupFixedCatValues(selectedGroup.Id);

                var arrFixedCatValues = groupFixedCatValues.GetCatList().ToList();
                var arrCatDesc = selectedGroup.GetCatDescList().ToList();

                for (var i = 0; i < arrCatDesc.Count; i++)
                {
                    if (i < arrFixedCatValues.Count)
                    {
                        var lbl = new Label
                        {
                            VerticalOptions = LayoutOptions.Center,
                            FontSize = (double)Application.Current.Resources["LblDetailFontSize"],
                            TextColor = (Color)Application.Current.Resources["ColorLabelDetail"],
                            Text = arrCatDesc[i]
                        };
                        Grid.SetColumn(lbl, 0);
                        Grid.SetRow(lbl, i);
                        Grid.Children.Add(lbl);
                        var lblVal = new Label
                        {
                            VerticalOptions = LayoutOptions.Center,
                            FontSize = (double)Application.Current.Resources["LblFontSize"],
                            TextColor = (Color)Application.Current.Resources["ColorLabel"],
                            FontAttributes = FontAttributes.Bold,
                            HorizontalTextAlignment = TextAlignment.End,
                            Text = arrFixedCatValues[i]
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
                            Text = string.Empty,
                            IsEnabled = i == arrFixedCatValues.Count // only first SearchBar is enabled
                        };
                        var sbIdx = i;
                        sb.TextChanged += (o, args) =>
                        {
                            if (sbIdx == arrCatDesc.Count - 1)
                            {
                                CmdSave.IsEnabled = sb.Text != string.Empty;
                            }
                            else
                            {
                                _lstCatSearchBars[sbIdx - arrFixedCatValues.Count + 1].Text = string.Empty;
                                _lstCatSearchBars[sbIdx - arrFixedCatValues.Count + 1].IsEnabled = sb.Text != string.Empty;
                            }
                        };
                        sb.Focused += (s, args) =>
                        {
                            var arrCatValues = new string[sbIdx - arrFixedCatValues.Count + 1];
                            for (var j = arrFixedCatValues.Count; j < sbIdx; j++)
                            {
                                arrCatValues[j - arrFixedCatValues.Count] = $"Cat{j + 1}={HttpUtility.UrlEncode(_lstCatSearchBars[j - arrFixedCatValues.Count].Text)}";
                            }
                            Navigation.PushAsync(
                                new SearchPage(arrCatDesc[sbIdx], true, async (searchText) =>
                                {
                                    Logger.Debug($"Search.{arrCatDesc[sbIdx]}.BEGIN(searchText={searchText})");
                                    arrCatValues[sbIdx - arrFixedCatValues.Count] = $"SearchText={HttpUtility.UrlEncode(searchText)}";

                                    var catValueList = await App.FriendStore.GetGroupCatValues(selectedGroup.Id, true, arrCatValues);
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
                        SlMain.Children.Add(sb);
                        _lstCatSearchBars.Add(sb);
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

        private void CmdSave_OnClicked(object sender, EventArgs e)
        {
        }
    }
}