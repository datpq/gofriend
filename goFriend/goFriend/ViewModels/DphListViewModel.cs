using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using goFriend.Controls;
using goFriend.Services;
using Xamarin.Forms;

namespace goFriend.ViewModels
{
    public class DphListViewModel : INotifyPropertyChanged
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        private ObservableCollection<DphListViewItemModel> _dphListItems = new ObservableCollection<DphListViewItemModel>();
        public ObservableCollection<DphListViewItemModel> DphListItems
        {
            get => _dphListItems;
            set
            {
                _dphListItems = value;
                OnPropertyChanged(nameof(DphListItems));
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

        private int _pageSize;
        public int PageSize
        {
            get => _pageSize;
            set
            {
                _pageSize = value;
                OnPropertyChanged(nameof(PageSize));
            }
        }

        public int CurrentPage { get; set; }
        private bool _allItemsFetched;

        public Func<Task<IEnumerable<DphListViewItemModel>>> GetListViewItemsFunc { get; set; }
        public ICommand RefreshCommand
        {
            get
            {
                return new Command(async () =>
                {
                    if (App.IsInitializing) return;
                    try
                    {
                        Logger.Debug("RefreshCommand.BEGIN");
                        _allItemsFetched = false;
                        CurrentPage = 0; //reset to page 0
                        DphListItems.Clear();
                        await FetchItems();
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.ToString());
                    }
                    finally
                    {
                        Logger.Debug("RefreshCommand.END");
                    }
                });
            }
        }

        public async void FetchMoreItems()
        {
            if (_allItemsFetched) return;
            try
            {
                Logger.Debug("FetchMoreItems.BEGIN");
                CurrentPage++;
                await FetchItems();
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                Logger.Debug("FetchMoreItems.END");
            }
        }

        private async Task FetchItems()
        {
            try
            {
                //do not show Refreshing on iOS, that make ListView to scroll to Top unexpectedly
                if (Device.RuntimePlatform == Device.Android) IsRefreshing = true;
                var result = await GetListViewItemsFunc();
                if (result == null || PageSize == 0 || result.Count() < PageSize)
                {
                    _allItemsFetched = true;
                }
                if (result != null)
                {
                    foreach (var item in result.Where(x => DphListItems.All(y => y.Id != x.Id)))
                    {
                        DphListItems.Add(item);
                    }
                    Logger.Debug($"{result.Count()} item(s) fetched.");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                //do not show Refreshing on iOS, that make ListView to scroll to Top unexpectedly
                if (Device.RuntimePlatform == Device.Android) IsRefreshing = false;
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

        public ICommand Button1Command { get; set; }
        public ICommand Button2Command { get; set; }
    }

    public class DphListViewItemModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public object SelectedObject { get; set; }
        public string ImageUrl { get; set; }
        public string OverlappingImageUrl { get; set; }
        public OverlapType OverlapType { get; set; }

        public bool HighLightVisible {
            get => _highLightColor != Color.Default;
        }

        private Color _highLightColor;
        public Color HighLightColor {
            get => _highLightColor;
            set
            {
                _highLightColor = value;
                OnPropertyChanged(nameof(HighLightColor));
            }
        }
        private FormattedString _formattedText;
        public FormattedString FormattedText {
            get => _formattedText;
            set
            {
                _formattedText = value;
                OnPropertyChanged(nameof(FormattedText));
            }
        }

        public double Button1Width => Button1ImageSource == null ? 0 : 30;
        public double Button1Height => Button1Width;
        public double Button1Radius => Button1Width / 2;
        public ImageSource Button1ImageSource { get; set; }
        public double Button2Width => Button2ImageSource == null ? 0 : 30;
        public double Button2Height => Button2Width;
        public double Button2Radius => Button2Width / 2;

        public ImageSource Button2ImageSource { get; set; }
        //INotifyPropertyChanged implementation method  
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
