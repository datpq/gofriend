﻿using System;
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
        private readonly DphListViewModel _dphListViewModel;
        private Action<DphListViewItemModel> _cellOnTapped;
        //private Func<Task<IEnumerable<DphListViewItemModel>>> _getListViewItemsFunc;
        private DateTime _lastRefreshDateTime = DateTime.Today.AddYears(-1); //default value is a very small value

        public static readonly BindableProperty TimeoutProperty = BindableProperty.CreateAttached(nameof(Timeout), typeof(int), typeof(DphListView), 0);
        public int Timeout
        {
            get => (int)GetValue(TimeoutProperty);
            set => SetValue(TimeoutProperty, value);
        }

        public DphListView()
        {
            InitializeComponent();
            BindingContext = _dphListViewModel = new DphListViewModel();
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
    }
}