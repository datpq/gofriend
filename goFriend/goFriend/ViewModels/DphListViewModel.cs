using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using goFriend.Services;
using Xamarin.Forms;

namespace goFriend.ViewModels
{
    public class DphListViewModel : INotifyPropertyChanged
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        private ObservableCollection<DphListViewItemModel> _items = new ObservableCollection<DphListViewItemModel>();
        public ObservableCollection<DphListViewItemModel> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        public Func<Task<IEnumerable<DphListViewItemModel>>> GetListViewItemsFunc { get; set; }
        public ICommand RefreshCommand
        {
            get
            {
                return new Command(async () =>
                {
                    Logger.Debug("RefreshCommand.BEGIN");
                    IsRefreshing = true;
                    Items.Clear();
                    var result = await GetListViewItemsFunc();
                    if (result != null)
                    {
                        foreach (var x in result.ToList())
                        {
                            Items.Add(x);
                        };
                    }
                    IsRefreshing = false;
                    Logger.Debug("RefreshCommand.END");
                });
            }
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "", Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value)) return false;
            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        //INotifyPropertyChanged implementation method  
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DphListViewItemModel
    {
        public int Id { get; set; }
        public FormattedString FormattedText { get; set; }
        public string ImageUrl { get; set; }

        public double Button1Width => Button1ImageSource == null ? 0 : 30;
        public double Button1Height => Button1Width;
        public double Button1Radius => Button1Width / 2;
        public ImageSource Button1ImageSource { get; set; }
        public double Button2Width => Button2ImageSource == null ? 0 : 30;
        public double Button2Height => Button2Width;
        public double Button2Radius => Button2Width / 2;
        public ImageSource Button2ImageSource { get; set; }
    }
}
