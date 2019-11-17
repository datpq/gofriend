using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private Action<DphListViewItemModel> _button1OnClicked;
        private Action<DphListViewItemModel> _button2OnClicked;
        //private Func<Task<IEnumerable<DphListViewItemModel>>> _getListViewItemsFunc;

        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public DphListView()
        {
            InitializeComponent();
            BindingContext = _dphListViewModel = new DphListViewModel();
        }

        public void Initialize(Action<DphListViewItemModel> cellOnTapped = null,
            Action<DphListViewItemModel> button1OnClicked = null, Action<DphListViewItemModel> button2OnClicked = null)
        {
            _cellOnTapped = cellOnTapped;
            _button1OnClicked = button1OnClicked;
            _button2OnClicked = button2OnClicked;
        }

        public void LoadItems(Func<Task<IEnumerable<DphListViewItemModel>>> getListViewItemsFunc)
        {
            _dphListViewModel.GetListViewItemsFunc = getListViewItemsFunc;
            _dphListViewModel.RefreshCommand.Execute(null);
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

        private void Cell_OnTapped(object sender, EventArgs e)
        {
            var selectedItem = (DphListViewItemModel)Lv.SelectedItem;
            _cellOnTapped?.Invoke(selectedItem);
        }

        private void Button1_OnClicked(object sender, EventArgs e)
        {
            var selectedItem = (DphListViewItemModel)Lv.SelectedItem;
            _button1OnClicked?.Invoke(selectedItem);
        }

        private void Button2_OnClicked(object sender, EventArgs e)
        {
            var selectedItem = (DphListViewItemModel)Lv.SelectedItem;
            _button2OnClicked?.Invoke(selectedItem);
        }
    }
}