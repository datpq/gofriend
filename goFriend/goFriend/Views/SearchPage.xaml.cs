using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using goFriend.Services;
using goFriend.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchPage : ContentPage
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        private readonly SearchViewModel _searchViewModel;
        private readonly Action<string> _setSelectedItemAction;
        //private string _lastNoResultText;
        private string _lastSearchText;

        public SearchPage(string title, bool acceptNotFoundValue, Func<string, Task<IEnumerable<SearchItemModel>>> getSearchItemsFunc,
            Action<string> setSelectedItemAction = null, string searchText = "")
        {
            Logger.Debug($"SearchPage.BEGIN(searchText={searchText})");
            InitializeComponent();

            CmdOk.BackgroundColor = Sb.BackgroundColor;
            BindingContext = _searchViewModel = new SearchViewModel
            {
                Title = title,
                AcceptNotFoundValue = acceptNotFoundValue,
                Text = searchText,
                SearchCommand = new Command<string>(text =>
                {
                    if (text == _lastSearchText) return;
                    _lastSearchText = text;
                    _searchViewModel.Items.Clear();
                    //if (_lastNoResultText != null && text.StartsWith(
                    //        _lastNoResultText, StringComparison.CurrentCultureIgnoreCase)) return; //search deeper always return nothing
                    //UserDialogs.Instance.ShowLoading(res.Processing); // Do not display processing dialog as the keyboard will disappear. Search bar will lose focus
                    getSearchItemsFunc(text).ContinueWith(task =>
                    {
                        Sb.IsEnabled = true;
                        var searchResults = task.Result.ToList();
                        //searchResults.Where(x => CultureInfo.CurrentCulture.CompareInfo.IndexOf(x.Text, text, CompareOptions.IgnoreCase) >= 0)
                        searchResults.OrderByDescending(x => x.ItemType).ThenBy(x => x.Text).ForEach(x =>
                        {
                            _searchViewModel.Items.Add(x);
                            var lastCell = LvResults.TemplatedItems.Last();
                            var stackLayout = ((ViewCell)lastCell).View as StackLayout;
                            if (x.Description == null && x.ImageSource == null && stackLayout?.Children.First() is Grid grid)
                            {
                                grid.RowDefinitions[1].Height = new GridLength(0);
                            }
                        });
                        //if (!searchResults.Any()) _lastNoResultText = text;
                        //UserDialogs.Instance.HideLoading();
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                })
            };
            Sb.IsEnabled = false; //disabled when doing the search in background
            _lastSearchText = $"{searchText}_"; //just something different from searchText
            Sb.SearchCommand?.Execute(searchText); // first time starting, TextChanged is not called automatically

            //_searchViewModel.Text = searchText; //do not set the Text in constructor as SearchCommand is still null. Here SearchCommand is already set
            _setSelectedItemAction = setSelectedItemAction;

            Logger.Debug("SearchPage.END");
        }

        private void Cell_OnTapped(object sender, EventArgs e)
        {
            var selectedItem = (SearchItemModel) LvResults.SelectedItem;
            _setSelectedItemAction?.Invoke(selectedItem.Text);
            Navigation.PopAsync();
        }

        private void LvResults_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            // don't do anything if we just de-selected the row.
            if (e.Item == null) return;

            // Optionally pause a bit to allow the preselect hint.
            Task.Delay(500);

            // Deselect the item.
            if (sender is ListView lv) lv.SelectedItem = null;
        }

        private async void CmdOk_Clicked(object sender, EventArgs e)
        {
            var searchText = _searchViewModel.Text.Trim();
            //var searchText = Sb.Text.Trim();
            if (searchText != string.Empty)
            {
                _setSelectedItemAction?.Invoke(searchText);
                await Navigation.PopAsync();
            }
        }
    }
}