using CoreLocation;
using goFriend.Services;
using goFriend.iOS;
using System;
using UIKit;
using Xamarin.Forms;
using goFriend.Models;
using goFriend.Views;

[assembly: Dependency(typeof(LocationService))]
namespace goFriend.iOS
{
    public class LocationService : ILocationService
    {
        private ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        public event EventHandler StateChanged;

        protected CLLocationManager LocationManager { get; set; }
        private bool _isRunning = false;
        private double _lastLatitude;
        private double _lastLongitude;

        public LocationService()
        {
            LocationManager = new CLLocationManager
            {
                PausesLocationUpdatesAutomatically = false
            };

            // iOS 8 has additional permissions requirements
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                LocationManager.RequestAlwaysAuthorization(); // works in background
                //locMgr.RequestWhenInUseAuthorization (); // only in foreground
            }

            // iOS 9 requires the following for background location updates
            // By default this is set to false and will not allow background updates
            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                LocationManager.AllowsBackgroundLocationUpdates = true;
            }
        }

        public bool IsRunning()
        {
            return _isRunning;
        }

        public void Start()
        {
            try
            {
                Logger.Debug($"Start.BEGIN");

                //AppTrackingTransparency.ATTrackingManager.RequestTrackingAuthorization((result) =>
                //{
                //    switch (result)
                //    {
                //        case AppTrackingTransparency.ATTrackingManagerAuthorizationStatus.NotDetermined:
                //            break;
                //        case AppTrackingTransparency.ATTrackingManagerAuthorizationStatus.Restricted:
                //            break;
                //        case AppTrackingTransparency.ATTrackingManagerAuthorizationStatus.Denied:
                //            break;
                //        case AppTrackingTransparency.ATTrackingManagerAuthorizationStatus.Authorized:
                //            break;
                //        default:
                //            break;
                //    }
                //});
                //if (AppTrackingTransparency.ATTrackingManager.TrackingAuthorizationStatus
                //    != AppTrackingTransparency.ATTrackingManagerAuthorizationStatus.Authorized) return;

                // We need the user's permission for our app to use the GPS in iOS. This is done either by the user accepting
                // the popover when the app is first launched, or by changing the permissions for the app in Settings
                if (CLLocationManager.LocationServicesEnabled)
                {

                    //set the desired accuracy, in meters
                    LocationManager.DesiredAccuracy = 1;

                    LocationManager.LocationsUpdated += LocationManager_LocationsUpdated;

                    LocationManager.StartUpdatingLocation();
                    _isRunning = true;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                Logger.Debug($"Start.END");
            }
        }

        private async void LocationManager_LocationsUpdated(object sender, CLLocationsUpdatedEventArgs e)
        {
            var location = e.Locations[e.Locations.Length - 1];
            if (_lastLatitude != location.Coordinate.Latitude || _lastLongitude != location.Coordinate.Longitude)
            {
                Logger.Debug($"LocationManager_LocationsUpdated Latitude: {location.Coordinate.Latitude}, Longitude: {location.Coordinate.Longitude}");
                _lastLatitude = location.Coordinate.Latitude;
                _lastLongitude = location.Coordinate.Longitude;
            }
            if (_isRunning)
            {
                //LocationUpdated(this, new LocationUpdatedEventArgs(e.Locations[e.Locations.Length - 1]));
                await App.ReceiveLocationUpdate(location.Coordinate.Latitude, location.Coordinate.Longitude);
            }
        }

        public void Stop()
        {
            try
            {
                Logger.Debug($"Stop.BEGIN");
                LocationManager.StopUpdatingLocation();
                LocationManager.LocationsUpdated -= LocationManager_LocationsUpdated;
                _isRunning = false;
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                Logger.Debug($"Stop.END");
            }
        }

        public void Pause()
        {
            if (!_isRunning)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }

        public void RefreshStatus()
        {
            var result = new ServiceNotification
            {
                ContentTitle = res.SvcBackground,
                ContentText = res.SvcBackgroundContentText,
                SummaryText = null,
                NotificationType = NotificationType.FriendsAroundStatusChanged,
                InboxLines = MapOnlinePage.GetMapOnlineStatus()
            };
            App.NotificationService.SendNotification(result);
        }
    }
}