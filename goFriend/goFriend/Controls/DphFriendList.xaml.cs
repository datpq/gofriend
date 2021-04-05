using System;
using System.Linq;
using goFriend.DataModel;
using goFriend.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DphFriendList : ContentView
    {
        public DphFriendList()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty ImageSizeProperty =
            BindableProperty.CreateAttached(nameof(ImageSize), typeof(double), typeof(DphFriendList), DphListView.MediumImage);
        public double ImageSize
        {
            get => (double)GetValue(ImageSizeProperty);
            set => SetValue(ImageSizeProperty, value);
        }

        public void Initialize(Action<DphListViewItemModel> cellOnTapped = null)
        {
            DphFriendSelection.SelectedGroupName = Settings.LastBrowsePageGroupName;
            DphFriendSelection.Initialize((selectedGroup, searchText, arrFixedCats, arrCatValues) =>
            {
                Settings.LastBrowsePageGroupName = selectedGroup.Group.Name;
                DphListView.Initialize(cellOnTapped);
                DphListView.LoadItems(async () =>
                {
                    var listViewModel = (DphListViewModel) DphListView.BindingContext;
                    var catGroupFriends = await App.FriendStore.GetGroupFriends(selectedGroup.Group.Id, true,
                        listViewModel.PageSize, listViewModel.CurrentPage * listViewModel.PageSize, true, true, searchText,
                        arrCatValues);
                    var result = catGroupFriends.Select(x =>
                        {
                            var formattedString = new FormattedString
                            {
                                Spans =
                                {
                                    new Span
                                    {
                                        Text = x.Friend.Name, FontAttributes = FontAttributes.Bold,
                                        FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Span)),
                                        LineHeight = 1.2
                                    },
                                    new Span {Text = Environment.NewLine},
                                    new Span {Text = x.GetCatValueDisplay(arrFixedCats.Count), LineHeight = 1,
                                        FontSize = Device.GetNamedSize(NamedSize.Caption, typeof(Span)) },
                                }
                            };
                            //if (ImageSize == DphListView.BigImage)
                            {
                                formattedString.Spans.Add(new Span { Text = Environment.NewLine });
                                formattedString.Spans.Add(new Span { Text = x.Friend.CountryName, LineHeight = 1,
                                    FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Span)) });
                            }
                            return new DphListViewItemModel
                            {
                                Id = x.Id,
                                SelectedObject = x,
                                //ImageUrl = x.Friend.GetImageUrl(FacebookImageType.small) // small 50 x 50
                                ImageUrl = x.Friend.GetImageUrl(), // normal 100 x 100
                                FormattedText = formattedString
                            };
                        }
                    );
                    return result;
                });
            });
        }
    }
}