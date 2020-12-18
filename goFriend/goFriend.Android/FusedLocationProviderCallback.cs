using Android.Gms.Location;
using goFriend.Services;
using System.Linq;

namespace goFriend.Droid
{
    public class FusedLocationProviderCallback : LocationCallback
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        public FusedLocationProviderCallback()
        {
        }

        public override void OnLocationAvailability(LocationAvailability locationAvailability)
        {
            Logger.Debug($"IsLocationAvailable: {locationAvailability.IsLocationAvailable}");
        }

        public override async void OnLocationResult(LocationResult result)
        {
            if (result.Locations.Any())
            {
                var location = result.Locations.First();
                Logger.Debug($"OnLocationResult Latitude: {location.Latitude}, Longitude: {location.Longitude}");
                if (LocationService.IsTracing)
                {
                    await App.ReceiveLocationUpdate(location.Latitude, location.Longitude);
                }
            }
            else
            {
            }
        }
    }
}