using System;
using System.Collections.Generic;
using System.Linq;
using goFriend.Services;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace goFriend.Controls
{
    public class DphPin
    {
        public DphMap Map { get; }

        public DphPin(DphMap map)
        {
            Map = map;
        }

        public string IconUrl { get; set; }
        public string Url { get; set; }
        public bool Draggable { get; set; }

        public Position Position { get; set; }
        public PinType Type { get; set; }
        private Pin _pin;
        public Pin Pin
        {
            get
            {
                if (_pin == null)
                {
                    _pin = new Pin
                    {
                        Position = Position,
                        Type = Type
                    };
                }
                return _pin;
            }
        }

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
        public readonly Dictionary<Pin, DphPin> CustomPins = new Dictionary<Pin, DphPin>();

        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public DphMap() : base(MapSpan.FromCenterAndRadius(
            DefaultPosition, Distance.FromKilometers(DefaultDistance))) {}

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
