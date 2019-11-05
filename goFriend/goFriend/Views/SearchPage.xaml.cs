using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

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
                        Sb.Focus();
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

        private void Button_OnClicked(object sender, EventArgs e)
        {
            var searchText = _searchViewModel.Text.Trim();
            //var searchText = Sb.Text.Trim();
            if (searchText != string.Empty)
            {
                _setSelectedItemAction?.Invoke(searchText);
                Navigation.PopAsync();
            }
        }
    }

    public class TextChangedBehavior : Behavior<SearchBar>
    {
        private CancellationTokenSource _cancelToken = new CancellationTokenSource();
        //private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        protected override void OnAttachedTo(SearchBar bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.TextChanged += Bindable_TextChanged;
        }

        protected override void OnDetachingFrom(SearchBar bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.TextChanged -= Bindable_TextChanged;
        }

        private void Bindable_TextChanged(object sender, TextChangedEventArgs e)
        {
            StopSearchCommand();
            var cts = _cancelToken; // safe copy
            Device.StartTimer(TimeSpan.FromMilliseconds(Constants.SearchCommandDelayTime),
                () => {
                    //Logger.Debug($"timer running: {e.NewTextValue}");
                    if (cts.IsCancellationRequested) return false;
                    ((SearchBar)sender).SearchCommand?.Execute(e.NewTextValue);
                    return false; // or true for periodic behavior
                });
        }

        private void StopSearchCommand()
        {
            Interlocked.Exchange(ref _cancelToken, new CancellationTokenSource()).Cancel();
        }
    }
}