using goFriend.Services;
using goFriend.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace goFriend.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DphGroupSearch : ContentView
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());
        private SearchViewModel _searchViewModel;
        private Action<string> _selectItemAction;
        private Action _unselectItemAction;
        private bool _isSearchingMode = true;

        public DphGroupSearch()
        {
            InitializeComponent();
        }

        public void Initialize(Func<string, Task<IEnumerable<SearchItemModel>>> getSearchItemsFunc,
            Action<string> selectItemAction = null, Action unselectItemAction = null)
        {
            BindingContext = _searchViewModel = new SearchViewModel
            {
                SearchCommand = new Command<string>(text =>
                {
                    Logger.Debug($"SearchCommand.BEGIN(_isSearchingMode={_isSearchingMode})");
                    if (!_isSearchingMode) // after cell tapped, do not search
                    {
                        _isSearchingMode = true;
                        return;
                    }
                    _unselectItemAction?.Invoke();
                    LvResults.IsVisible = true;
                    _searchViewModel.Items.Clear();
                    getSearchItemsFunc(text).ContinueWith(task =>
                    {
                        Sb.IsEnabled = true;
                        Sb.Focus();
                        var searchResults = task.Result.ToList();
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
                        Logger.Debug($"SearchCommand.END(_isSearchingMode={_isSearchingMode})");
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                })
            };
            Sb.IsEnabled = false; //disabled when doing the search in background
            Sb.SearchCommand?.Execute(string.Empty); // first time starting, TextChanged is not called automatically

            _selectItemAction = selectItemAction;
            _unselectItemAction = unselectItemAction;
        }

        public void Reset()
        {
            Sb.Text = string.Empty;
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

        private void Cell_OnTapped(object sender, EventArgs e)
        {
            Logger.Debug($"Cell_OnTapped.BEGIN(_isSearchingMode={_isSearchingMode})");
            var selectedItem = (SearchItemModel)LvResults.SelectedItem;
            
            _isSearchingMode = false;
            Sb.Text = selectedItem.Text;

            LvResults.IsVisible = false;
            _selectItemAction?.Invoke(selectedItem.Text);
            Logger.Debug($"Cell_OnTapped.END(_isSearchingMode={_isSearchingMode})");
        }
    }
}