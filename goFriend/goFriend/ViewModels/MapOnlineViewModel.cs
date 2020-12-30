using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using goFriend.DataModel;

namespace goFriend.ViewModels
{
    public class MapOnlineViewModel : INotifyPropertyChanged
    {
        public Group Group { get; set; }
        public GroupFriend GroupFriend { get; set; }
        public int FixedCatsCount { get; set; }

        private bool _isRunning;
        public bool IsRunning {
            get => _isRunning;
            set
            {
                _isRunning = value;
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        private double _radius = Constants.MAPONLINE_DEFAULT_RADIUS;
        public double Radius {
            get => _radius;
            set
            {
                _radius = value;
                OnPropertyChanged(nameof(Radius));
                OnPropertyChanged(nameof(RadiusDisplay));
                OnPropertyChanged(nameof(RadiusDisplayWithSummary));
            }
        }

        public string RadiusDisplay => Items.Single(x => x.Radius == Radius).Display;
        public string RadiusDisplayWithSummary => Items.Single(x => x.Radius == Radius).DisplayWithSummary;

        public ObservableCollection<RadiusItemModel> Items { get; } = new ObservableCollection<RadiusItemModel>
        {
            new RadiusItemModel { Radius = 0.2 },
            new RadiusItemModel { Radius = 0.5 },
            new RadiusItemModel { Radius = 1 },
            new RadiusItemModel { Radius = 10 },
            new RadiusItemModel { Radius = 0 },
        };

        //INotifyPropertyChanged implementation method  
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RadiusItemModel
    {
        public double Radius { get; set; }
        public int OnlineFriends { get; set; } = 0;

        public string Display => Radius == 0 ? res.RadiusNoLimit : Radius < 1 ? $"{Radius * 1000} {res.RadiusM}" : $"{Radius} {res.RadiusKM}";
        public string DisplayWithSummary => $"{Display} ({OnlineFriends})";
    }
}
