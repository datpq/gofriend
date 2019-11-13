using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using goFriend.Services;
using goFriend.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DphListView : ContentView
    {
        private readonly DphListViewModel _dphListViewModel;
        private Action<DphListViewItemModel> _cellOnTapped;
        private Func<Task<IEnumerable<DphListViewItemModel>>> _getListViewItemsFunc;

        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public DphListView()
        {
            InitializeComponent();
            BindingContext = _dphListViewModel = new DphListViewModel();
        }

        public void Initialize(Func<Task<IEnumerable<DphListViewItemModel>>> getListViewItemsFunc, Action<DphListViewItemModel> cellOnTapped)
        {
            Logger.Debug("DphListView.Initialize.BEGIN");
            UserDialogs.Instance.ShowLoading(res.Processing);
            _dphListViewModel.Items.Clear();
            _getListViewItemsFunc = getListViewItemsFunc;
            _cellOnTapped = cellOnTapped;
            _getListViewItemsFunc?.Invoke().ContinueWith(task =>
            {
                Logger.Debug("_getListViewItemsFunc finished");
                var lvResults = task.Result.ToList();
                foreach (var x in lvResults)
                {
                    _dphListViewModel.Items.Add(x);
                }
                Logger.Debug("all items added. HideLoading");
                UserDialogs.Instance.HideLoading();
            }, TaskScheduler.FromCurrentSynchronizationContext());
            Logger.Debug("DphListView.Initialize.END");
        }

        private void Cell_OnTapped(object sender, EventArgs e)
        {
            var selectedItem = (DphListViewItemModel)Lv.SelectedItem;
            _cellOnTapped?.Invoke(selectedItem);
        }
    }
}