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
        private IEnumerable<Tuple<Group, bool, bool>> _groups;

        public GroupConnectionPage()
        {
            InitializeComponent();

            TxtGroup.Focused += (s, e) => {
                Navigation.PushAsync(
                    new SearchPage(res.Groups, async () =>
                    {
                        _groups = await App.FriendStore.GetGroups();
                        var groupList = _groups.OrderBy(x => x.Item2 ? 0 : 1).ThenBy(x => x.Item3 ? 0 : 1)
                            .Select(x => new Tuple<string, string, int>(x.Item1.Name, x.Item1.Desc, x.Item1.Id));
                        Logger.Debug($"groupList={JsonConvert.SerializeObject(groupList)}");
                        return groupList;
                    }, (selectedItem) =>
                    {
                        Logger.Debug($"selectedItem={JsonConvert.SerializeObject(selectedItem)}");
                        TxtGroup.Text = selectedItem.Text;
                        var rowToRemove = Grid.Children.Where(x => Grid.GetRow(x) > 0).ToList();
                        foreach (var child in rowToRemove)
                        {
                            Grid.Children.Remove(child);
                        }

                        var selectedTuple = _groups.Single(x => x.Item1.Id == selectedItem.Id);
                        var sg = selectedTuple.Item1;
                        var arrCatDesc = new[]
                        {
                            sg.Cat1Desc, sg.Cat2Desc, sg.Cat3Desc, sg.Cat4Desc, sg.Cat5Desc,
                            sg.Cat6Desc, sg.Cat7Desc, sg.Cat8Desc, sg.Cat9Desc
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