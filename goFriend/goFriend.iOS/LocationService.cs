using CoreLocation;
using goFriend.Services;
using goFriend.iOS;
using System;
using UIKit;
using Xamarin.Forms;
using System.Threading.Tasks;

[assembly: Dependency(typeof(LocationService))]
namespace goFriend.iOS
{
    public class LocationService : ILocationService
    {
        private ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        public event EventHandler StateChanged;

        protected CLLocationManager LocationManager { get; set; }
        private bool _isRunning = false;

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

        public Task Start()
        {
            try
            {
                Logger.Debug($"Start.BEGIN");

                // We need the user's permission for our app to use the GPS in iOS. This is done either by the user accepting
                // the popover when the app is first launched, or by changing the permissions for the app in Settings
                if (CLLocationManager.LocationServicesEnabled)
                {

                    //set the desired accuracy, in meters
                    LocationManager.DesiredAccuracy = 1;

                    LocationManager.LocationsUpdated += LocationManager_LocationsUpdated;

                    LocationManager.StartUpdatingLocation();
                    _isRunning = true;
                    StateChanged?.Invoke(this, null);
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
            return Task.CompletedTask;
        }

        private async void LocationManager_LocationsUpdated(object sender, CLLocationsUpdatedEventArgs e)
        {
            var location = e.Locations[e.Locations.Length - 1];
            Logger.Debug($"LocationManager_LocationsUpdated Latitude: {location.Coordinate.Latitude}, Longitude: {location.Coordinate.Longitude}");
            if (_isRunning)
            {
                //LocationUpdated(this, new LocationUpdatedEventArgs(e.Locations[e.Locations.Length - 1]));
                await App.ReceiveLocationUpdate(location.Coordinate.Latitude, location.Coordinate.Longitude);
            }
        }

        public Task Stop()
        {
            try
            {
                Logger.Debug($"Stop.BEGIN");
                LocationManager.StopUpdatingLocation();
                LocationManager.LocationsUpdated -= LocationManager_LocationsUpdated;
                _isRunning = false;
                StateChanged?.Invoke(this, null);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                Logger.Debug($"Stop.END");
            }
            return Task.CompletedTask;
        }

        public Task Pause()
        {
            if (!_isRunning)
            {
                return Start();
            }
            else
            {
                return Stop();
            }
        }
    }
}