using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using goFriend.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DphListView : ContentView
    {
        public const double BigImage = 65;
        public const double MediumImage = 50;

        private readonly DphListViewModel _dphListViewModel;
        private Action<DphListViewItemModel> _cellOnTapped;
        //private Func<Task<IEnumerable<DphListViewItemModel>>> _getListViewItemsFunc;
        private DateTime _lastRefreshDateTime = DateTime.Today.AddYears(-1); //default value is a very small value

        public static readonly BindableProperty ImageSizeProperty =
            BindableProperty.CreateAttached(nameof(ImageSize), typeof(double), typeof(DphListView), MediumImage);
        public double ImageSize
        {
            get => (double)GetValue(ImageSizeProperty);
            set => SetValue(ImageSizeProperty, value);
        }

        public static readonly BindableProperty TimeoutProperty =
            BindableProperty.CreateAttached(nameof(Timeout), typeof(int), typeof(DphListView), 0);
        public int Timeout
        {
            get => (int)GetValue(TimeoutProperty);
            set => SetValue(TimeoutProperty, value);
        }

        public static readonly BindableProperty OverlapTypeProperty =
            BindableProperty.CreateAttached(nameof(OverlapType), typeof(OverlapType), typeof(DphListView), OverlapType.Notification);
        public OverlapType OverlapType
        {
            get => (OverlapType)GetValue(OverlapTypeProperty);
            set => SetValue(OverlapTypeProperty, value);
        }

        public DphListView()
        {
            InitializeComponent();
            BindingContext = _dphListViewModel = new DphListViewModel
            {
                PageSize = Constants.ListViewPageSize
            };
        }

        public void Initialize(Action<DphListViewItemModel> cellOnTapped = null,
            Action<DphListViewItemModel> button1OnClicked = null, Action<DphListViewItemModel> button2OnClicked = null)
        {
            _cellOnTapped = cellOnTapped;
            _dphListViewModel.Button1Command = new Command(selectedItem =>
            {
                button1OnClicked?.Invoke(selectedItem as DphListViewItemModel);
            });
            _dphListViewModel.Button2Command = new Command(selectedItem =>
            {
                button2OnClicked?.Invoke(selectedItem as DphListViewItemModel);
            });
        }

        public void LoadItems(Func<Task<IEnumerable<DphListViewItemModel>>> getListViewItemsFunc)
        {
            _dphListViewModel.GetListViewItemsFunc = getListViewItemsFunc;
            _dphListViewModel.RefreshCommand.Execute(null);
            _lastRefreshDateTime = DateTime.Now;
            //UserDialogs.Instance.ShowLoading(res.Processing);
            //_dphListViewModel.Items.Clear();
            //_getListViewItemsFunc = getListViewItemsFunc;
            //_getListViewItemsFunc?.Invoke().ContinueWith(task =>
            //{
            //    Logger.Debug("_getListViewItemsFunc finished");
            //    var lvResults = task.Result.ToList();
            //    foreach (var x in lvResults)
            //    {
            //        _dphListViewModel.Items.Add(x);
            //    }
            //    Logger.Debug("all items added. HideLoading");
            //    UserDialogs.Instance.HideLoading();
            //}, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void Refresh(bool forced = false)
        {
            if (!_dphListViewModel.IsRefreshing && (forced || _lastRefreshDateTime.AddMinutes(Timeout) < DateTime.Now))
            {
                _dphListViewModel.RefreshCommand.Execute(null);
                _lastRefreshDateTime = DateTime.Now;
            }
        }

        private void Cell_OnTapped(object sender, EventArgs e)
        {
            var selectedItem = (DphListViewItemModel)Lv.SelectedItem;
            _cellOnTapped?.Invoke(selectedItem);
        }

        private void Lv_OnItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            if (_dphListViewModel.IsRefreshing || _dphListViewModel.DphListItems.Count == 0 || App.IsInitializing) return;
            if (((DphListViewItemModel) e.Item).Id == _dphListViewModel.DphListItems[_dphListViewModel.DphListItems.Count - 1].Id)
            {
                _dphListViewModel.FetchMoreItems();
            }
        }

        private void Lv_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            // don't do anything if we just de-selected the row.
            if (e.Item == null) return;

            // Optionally pause a bit to allow the preselect hint.
            Task.Delay(500);

            // Deselect the item.
            if (sender is ListView lv) lv.SelectedItem = null;
        }
    }
}