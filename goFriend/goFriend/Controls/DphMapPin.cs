using System;
using System.Collections.Generic;
using System.Linq;
using goFriend.Services;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Map = Xamarin.Forms.GoogleMaps.Map;

namespace goFriend.Controls
{
    public class DphPin
    {
        public DphMap Map { get; }

        public DphPin(DphMap map)
        {
            Map = map;
            Pin = new Pin();
        }

        public string IconUrl { get; set; }
        public string Url { get; set; }

        public bool IsDraggable {
            get => Pin.IsDraggable;
            set => Pin.IsDraggable = value;
        }

        public Position Position {
            get => Pin.Position;
            set => Pin.Position = value;
        }

        public PinType Type {
            get => Pin.Type;
            set => Pin.Type = value;
        }

        public Pin Pin { get; set; }

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                Pin.Label = _title;
            }
        }

        private string _subTitle1;
        public string SubTitle1
        {
            get => _subTitle1;
            set
            {
                _subTitle1 = value;
                Pin.Address = $"{_subTitle1}{Environment.NewLine}{_subTitle2}";
            }
        }

        private string _subTitle2;
        public string SubTitle2
        {
            get => _subTitle2;
            set
            {
                _subTitle2 = value;
                Pin.Address = $"{_subTitle1}{Environment.NewLine}{_subTitle2}";
            }
        }
    }

    public class DphMap : Map
    {
        public const double DefaultDistance = 5;
        public static readonly Position DefaultPosition = new Position(21.022642, 105.814416); // B7 thanh cong, Hanoi

        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        //workaround for MoveToLastRegionOnLayoutChange
        private CameraPosition _lastPosition;

        public DphMap()
        {
            InitialCameraUpdate = CameraUpdateFactory.NewPositionZoom(DefaultPosition, DefaultDistance);
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
            } else if (Pins.Count > 0)
            {
                MoveToRegionToCoverAllPins();
            }
        }

        public void MoveToRegionToCoverAllPins()
        {
            Logger.Debug($"MoveToRegionToCoverAllPins. Pins count = {Pins.Count}");
            if (Pins.Count == 1)
            {
                MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Position(Pins.Single().Position.Latitude, Pins.Single().Position.Longitude), Distance.FromKilometers(DefaultDistance)));
                return;
            }
            var latitudes = new List<double>();
            var longitudes = new List<double>();
            foreach (var pin in Pins)
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

            MoveToRegion(MapSpan.FromCenterAndRadius(new Position(finalLat, finalLong), Distance.FromKilometers(distance)));
        }
    }
}
