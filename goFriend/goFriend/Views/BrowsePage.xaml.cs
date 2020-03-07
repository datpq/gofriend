using System;
using System.Linq;
using goFriend.DataModel;
using goFriend.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BrowsePage : ContentPage
    {
        public BrowsePage()
        {
            InitializeComponent();

            DphFriendSelection.Initialize((selectedGroup, searchText, arrFixedCats, arrCatValues) =>
            {
                DphListView.Initialize(async(selectedItem) =>
                {
                    var selectedGroupFriend = (GroupFriend)selectedItem.SelectedObject;
                    var accountBasicInfoPage = new AccountBasicInfosPage();
                    await accountBasicInfoPage.Initialize(selectedGroup.Group, selectedGroupFriend, arrFixedCats.Count);
                    await Navigation.PushAsync(accountBasicInfoPage);
                });
                DphListView.LoadItems(async () =>
                {
                    var listViewModel = (DphListViewModel) DphListView.BindingContext;
                    var catGroupFriends = await App.FriendStore.GetGroupFriends(selectedGroup.Group.Id, true,
                        listViewModel.PageSize, listViewModel.CurrentPage * listViewModel.PageSize, true, searchText, arrCatValues);
                    var result = catGroupFriends.Select(x => new DphListViewItemModel
                    {
                        Id = x.Id,
                        SelectedObject = x,
                        //ImageUrl = x.Friend.GetImageUrl(FacebookImageType.small) // small 50 x 50
                        ImageUrl = x.Friend.GetImageUrl(), // normal 100 x 100
                        FormattedText = new FormattedString
                        {
                            Spans =
                            {
                                new Span {Text = x.Friend.Name, FontAttributes = FontAttributes.Bold,
                                    FontSize = (double)Application.Current.Resources["LblFontSize"], LineHeight = 1.2},
                                new Span {Text = Environment.NewLine},
                                new Span {Text = x.GetCatValueDisplay(arrFixedCats.Count), LineHeight = 1.2},
                                new Span {Text = Environment.NewLine},
                                new Span {Text = x.Friend.CountryName, LineHeight = 1.2}
                            }
                        }
                    });
                    return result;
                });
            });
        }
    }
}