using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using goFriend.Services;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchPage : ContentPage
    {
        private readonly Action<SearchItemModel> _setItemAction;
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();
        private IEnumerable<SearchItemModel> _searchResults;
        public ObservableCollection<SearchItemModel> Items { get; } = new ObservableCollection<SearchItemModel>();

        public SearchPage(string title, Func<Task<IEnumerable<SearchItemModel>>> getSearchItemsFunc,
            Action<SearchItemModel> setItemAction = null, string searchText = "")
        {
            _setItemAction = setItemAction;
            Logger.Debug("SearchPage.BEGIN");

            InitializeComponent();

            LvResults.ItemsSource = Items;

            Title = title;
            Sb.IsEnabled = false;
            getSearchItemsFunc().ContinueWith(task =>
            {
                Sb.IsEnabled = true;
                _searchResults = task.Result;
                //Logger.Debug($"_searchResults={JsonConvert.SerializeObject(_searchResults)}");
                Sb.Text = searchText;
            }, TaskScheduler.FromCurrentSynchronizationContext());

            Logger.Debug("SearchPage.END");
        }

        private void Sb_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var sb = (SearchBar) sender;
            //Logger.Debug($"sb.Text={sb.Text}");
            Items.Clear();
            (sb.Text == string.Empty ? _searchResults
                : _searchResults.Where(x => CultureInfo.CurrentCulture.CompareInfo.IndexOf(
                                                x.Text, sb.Text, CompareOptions.IgnoreCase) >= 0))
                .OrderByDescending(x => x.ItemType).ThenBy(x => x.Text).ForEach(x  => Items.Add(x));
            //Logger.Debug($"_searchItems={JsonConvert.SerializeObject(Items)}");
        }

        private void Cell_OnTapped(object sender, EventArgs e)
        {
            var selectedItem = (SearchItemModel) LvResults.SelectedItem;
            _setItemAction?.Invoke(selectedItem);
            Navigation.PopAsync();
        }
    }

    public class SearchItemModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
        public int ItemType { get; set; }
        public string ImageSource { get; set; }
        public Color ImageForeground { get; set; }
        public int SubItemCount { get; set; }
    }
}