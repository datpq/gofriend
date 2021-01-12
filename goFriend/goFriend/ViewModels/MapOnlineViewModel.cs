using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using goFriend.DataModel;
using System;
using System.Collections.Generic;
using goFriend.Services;
using goFriend.Controls;
using goFriend.Views;
using Xamarin.Essentials;
using Xamarin.Forms.GoogleMaps;
using goFriend.Helpers;

namespace goFriend.ViewModels
{
    public class MapOnlineViewModel : INotifyPropertyChanged
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        public Group Group { get; set; }
        public GroupFriend GroupFriend { get; set; }
        public int FixedCatsCount { get; set; }

        private readonly object _locker = new object();
        private readonly List<FriendLocation> _friendLocations = new List<FriendLocation>();
        public async void ReceiveLocation(FriendLocation friendLocation)
        {
            if (!friendLocation.GroupFriends.Any(x => x.GroupId == Group.Id))
            {
                var groupFriend = await App.FriendStore.GetGroupFriend(Group.Id, friendLocation.FriendId);
                friendLocation.GroupFriends.Add(groupFriend);
            }
            if (friendLocation.FriendId == App.User.Id) return;
            lock(_locker)
            {
                _friendLocations.RemoveAll(x => x.FriendId == friendLocation.FriendId);
                _friendLocations.Add(friendLocation);
            }
        }
        public void Refresh()
        {
            //TODO: Update the list, not clear and add again
            Items.ToList().ForEach(x => {
                x.FriendLocations.Clear();
                x.OnlineFriends = 0;
            });
            var myLocation = MapOnlinePage.MyLocation;
            if (IsRunning && myLocation != null && myLocation.IsOnline())
            {
                lock(_locker)
                {
                    // remove all the offline friends first
                    _friendLocations.RemoveAll(x => x.IsOffline());
                    _friendLocations.ForEach(x => {
                        x.DistanceToMyLocation = Location.CalculateDistance(myLocation.Location.Y,
                            myLocation.Location.X, x.Location.Y, x.Location.X, DistanceUnits.Kilometers);
                    });
                    _friendLocations.OrderBy(x => x.DistanceToMyLocation).ToList().ForEach(x =>
                    {
                        Items.Where(y => y.RadiusInKm >= x.DistanceToMyLocation
                        && x.GetRadiusInKm(Group.Id) >= x.DistanceToMyLocation).ToList().ForEach(y => y.FriendLocations.Add(x));
                    });
                }
                Items.ToList().ForEach(x =>
                {
                    x.FriendLocations.Add(myLocation);
                    x.OnlineFriends = x.FriendLocations.Count;
                }); // Include yourself in OnlineFriends

                //Refresh GUI
                Items.FireEventCollectionChanged();
                OnPropertyChanged(nameof(RadiusSelectedItem));
            }
        }
        public List<DphPin> GetPins()
        {
            var result = new List<DphPin>();
            var myLocation = MapOnlinePage.MyLocation;
            if (IsRunning && myLocation != null && myLocation.IsOnline())
            {
                result = RadiusSelectedItem.FriendLocations.Select(x =>
                {
                    var groupFriend = x.GroupFriends.SingleOrDefault(y => y.GroupId == Group.Id);
                    return new DphPin
                    {
                        Position = new Position(x.Location.Y, x.Location.X),
                        Title = x.Friend.Name,
                        SubTitle1 = groupFriend?.GetCatValueDisplay(FixedCatsCount),
                        SubTitle2 = x.IsOnlineActive() ? res.Online : string.Format(res.OnlineEarlier, x.ModifiedDate.Value.GetSpentTime()),
                        //SubTitle1 = $"{res.Groups} {Group.Name}",
                        //SubTitle2 = GroupFriend.GetCatValueDisplay(FixedCatsCount),
                        IconUrl = x.Friend.GetImageUrl(),
                        //UserRight = Constants.SuperUserIds.Contains(App.User.Id) ? UserType.Normal : vm.GroupFriend.UserRight,
                        UserRight = x.IsOnlineInactive() ? UserType.Pending : (groupFriend?.UserRight == UserType.Admin
                        && !x.FriendId.IsSuperUser()) ? UserType.Admin : UserType.Normal,
                        //Url = $"facebook://facebook.com/info?user={_viewModel.Friend.FacebookId}",
                        IsDraggable = false,
                        Type = PinType.Place
                    };
                }).ToList();
            }
            return result;
        }

        public bool IsRunningSaved { get; set; }
        public double RadiusSaved { get; set; }

        private DateTime _disabledExpiredTime = DateTime.MinValue;
        public DateTime DisabledExpiredTime {
            get => _disabledExpiredTime;
            set
            {
                _disabledExpiredTime = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        public bool IsEnabled => DateTime.Now > DisabledExpiredTime;

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
                //OnPropertyChanged(nameof(RadiusDisplay));
                //OnPropertyChanged(nameof(RadiusDisplayWithSummary));
                OnPropertyChanged(nameof(RadiusSelectedItem));
            }
        }

        //public string RadiusDisplay => Items.Single(x => x.Radius == Radius).Display;
        //public string RadiusDisplayWithSummary => Items.Single(x => x.Radius == Radius).DisplayWithSummary;
        public RadiusItemModel RadiusSelectedItem => Items.Single(x => x.Radius == Radius);

        public DphObservableCollection<RadiusItemModel> Items { get; } = new DphObservableCollection<RadiusItemModel>
            (Constants.MAPONLINE_RADIUS_LIST.Select(x => new RadiusItemModel { Radius = x }));

        //INotifyPropertyChanged implementation method
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RadiusItemModel : INotifyPropertyChanged
    {
        private double _radius;
        public double Radius { 
            get => _radius;
            set
            {
                _radius = value;
                OnPropertyChanged(nameof(Radius));
                OnPropertyChanged(nameof(Display));
                OnPropertyChanged(nameof(DisplayWithSummary));
            }
        }
        public double RadiusInKm => Radius == 0 ? double.MaxValue : Radius;

        public ObservableCollection<FriendLocation> FriendLocations { get; } = new ObservableCollection<FriendLocation>();
        private int _onlineFriends = 0;
        public int OnlineFriends {
            get => _onlineFriends;
            set
            {
                _onlineFriends = value;
                OnPropertyChanged(nameof(OnlineFriends));
                OnPropertyChanged(nameof(DisplayWithSummary));
            }
        }

        public string Display => Radius == 0 ? res.RadiusNoLimit : Radius < 1 ? $"{Radius * 1000} {res.RadiusM}" : $"{Radius} {res.RadiusKM}";
        public string DisplayWithSummary => $"{Display} ({OnlineFriends})";

        //INotifyPropertyChanged implementation method
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
