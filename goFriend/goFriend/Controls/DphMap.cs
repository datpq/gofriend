using System.Collections.Generic;
using System.Linq;
using goFriend.Services;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.GoogleMaps.Clustering;

namespace goFriend.Controls
{
    public static class MapExtension
    {
        public const double DefaultDistance = 5;
        public static readonly Position DefaultPosition = new Position(21.022642, 105.814416); // B7 thanh cong, Hanoi
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public static void MoveToRegionToCoverAllPins(this Map map)
        {
            Logger.Debug($"MoveToRegionToCoverAllPins. Pins count = {map.Pins.Count}");
            if (map.Pins.Count == 1)
            {
                map.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Position(map.Pins.Single().Position.Latitude, map.Pins.Single().Position.Longitude), Distance.FromKilometers(DefaultDistance)));
                return;
            }
            var latitudes = new List<double>();
            var longitudes = new List<double>();
            foreach (var pin in map.Pins)
            {
                latitudes.Add(pin.Position.Latitude);
                longitudes.Add(pin.Position.Longitude);
            }
            var lowestLat = latitudes.Min();
            var highestLat = latitudes.Max();
            var lowestLong = longitudes.Min();
            var highestLong = longitudes.Max();
            var finalLat = (lowestLat + highestLat) / 2;
            var finalLong = (lowestLong + highestLong) / 2;
            var distance = DistanceCalculation.GeoCodeCalc.CalcDistance(lowestLat, lowestLong, highestLat, highestLong, DistanceCalculation.GeoCodeCalcMeasurement.Kilometers);

            map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(finalLat, finalLong), Distance.FromKilometers(distance)));
        }
    }

    public class DphMap : Map
    {
        //workaround for MoveToLastRegionOnLayoutChange
        private CameraPosition _lastPosition;

        public DphMap()
        {
            InitialCameraUpdate = CameraUpdateFactory.NewPositionZoom(MapExtension.DefaultPosition, MapExtension.DefaultDistance);
            //MoveCamera(CameraUpdateFactory.NewPositionZoom(DefaultPosition, DefaultDistance));
            CameraIdled += (sender, args) =>
            {
                _lastPosition = args.Position;
            };
        }

        public void MoveToLastPosition()
        {
            if (_lastPosition != null)
            {
                MoveCamera(CameraUpdateFactory.NewCameraPosition(_lastPosition));
            }
            else if (Pins.Count > 0)
            {
                this.MoveToRegionToCoverAllPins();
            }
        }
    }

    public class DphClusterMap : ClusteredMap
    {
        //workaround for MoveToLastRegionOnLayoutChange
        private CameraPosition _lastPosition;

        public DphClusterMap()
        {
            InitialCameraUpdate = CameraUpdateFactory.NewPositionZoom(MapExtension.DefaultPosition, MapExtension.DefaultDistance);
            //MoveCamera(CameraUpdateFactory.NewPositionZoom(DefaultPosition, DefaultDistance));
            CameraIdled += (sender, args) =>
            {
                _lastPosition = args.Position;
            };
        }

        public void MoveToLastPosition()
        {
            if (_lastPosition != null)
            {
                MoveCamera(CameraUpdateFactory.NewCameraPosition(_lastPosition));
            }
            else if (Pins.Count > 0)
            {
                this.MoveToRegionToCoverAllPins();
            }
        }
    }
}
