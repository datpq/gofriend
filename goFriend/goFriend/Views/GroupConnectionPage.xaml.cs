using System;
using System.Collections.Generic;
using System.Linq;
using goFriend.DataModel;
using goFriend.Services;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GroupConnectionPage : ContentPage
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();
        private IEnumerable<ApiGetGroupsModel> _groups;

        public GroupConnectionPage()
        {
            InitializeComponent();

            TxtGroup.Focused += (s, e) => {
                Navigation.PushAsync(
                    new SearchPage(res.Groups, async () =>
                    {
                        try
                        {
                            _groups = await App.FriendStore.GetGroups();
                            var groupList = _groups.Select(x => new SearchItemModel
                            {
                                Text = x.Group.Name,
                                Description = x.Group.Desc,
                                Id = x.Group.Id,
                                ItemType = x.IsActiveMember ? 0 : x.IsMember ? 1 : 2,
                                SubItemCount = x.MemberCount,
                                ImageSource = x.Group.Name == "Hanoi9194" ? "group_admin.png" : "group.png",
                                ImageForeground = x.Group.Name == "Hanoi9194" ? Color.BlueViolet : (x.Group.Name == "Hanoi9194XaXu" ? (Color)Application.Current.Resources["ColorPrimary"] : (x.Group.Name == "Hanoi9194XaXu-Australia" ? (Color)Application.Current.Resources["ColorPrimaryLight"] : Color.LightGray))
                            }).OrderBy(x => x.ItemType);
                            //Logger.Debug($"groupList={JsonConvert.SerializeObject(groupList)}");
                            return groupList;
                        }
                        catch (GoException ex)
                        {
                            switch (ex.Msg.Code)
                            {
                                case MessageCode.UserTokenError:
                                    App.DisplayMsgError(res.MsgErrWrongToken);
                                    return null;
                                default:
                                    App.DisplayMsgError(ex.Msg.Msg);
                                    return null;
                            }
                        }
                        catch(Exception ex)
                        {
                            App.DisplayMsgError(ex.Message);
                            return null;
                        }
                    }, async (selectedItem) =>
                    {
                        Logger.Debug($"selectedItem={JsonConvert.SerializeObject(selectedItem)}");
                        TxtGroup.Text = selectedItem.Text;
                        var rowToRemove = Grid.Children.Where(x => Grid.GetRow(x) > 0).ToList();
                        foreach (var child in rowToRemove)
                        {
                            Grid.Children.Remove(child);
                        }

                        var selectedGroup = _groups.Single(x => x.Group.Id == selectedItem.Id).Group;
                        var groupCategory = await App.FriendStore.GetGroupCategory(selectedGroup.Id);

                        var arrCatDesc = new[]
                        {
                            selectedGroup.Cat1Desc, selectedGroup.Cat2Desc, selectedGroup.Cat3Desc, selectedGroup.Cat4Desc, selectedGroup.Cat5Desc,
                            selectedGroup.Cat6Desc, selectedGroup.Cat7Desc, selectedGroup.Cat8Desc, selectedGroup.Cat9Desc
                        }.Where(x => !string.IsNullOrEmpty(x));
                        var row = 1;
                        foreach (var cat in arrCatDesc)
                        {
                            var lbl = new Label
                            {
                                VerticalOptions = LayoutOptions.Center,
                                FontSize = (double)Application.Current.Resources["LblFontSize"],
                                TextColor = (Color) Application.Current.Resources["ColorLabel"],
                                Text = cat
                            };
                            Grid.SetColumn(lbl, 0);
                            Grid.SetRow(lbl, row);
                            Grid.Children.Add(lbl);
                            var entry = new Entry
                            {
                                VerticalOptions = LayoutOptions.Center,
                                FontSize = (double)Application.Current.Resources["LblFontSize"],
                            };
                            Grid.SetColumn(entry, 1);
                            Grid.SetRow(entry, row++);
                            Grid.Children.Add(entry);
                        }
                    }));
            };
        }
    }
}