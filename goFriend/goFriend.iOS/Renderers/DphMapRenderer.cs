using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using CoreLocation;
using FFImageLoading;
using FFImageLoading.Transformations;
using goFriend.Controls;
using goFriend.DataModel;
using goFriend.iOS.Renderers;
using Google.Maps;
using NLog;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.GoogleMaps.Clustering.iOS;
using Xamarin.Forms.GoogleMaps.iOS;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(DphMap), typeof(DphMapRenderer))]
[assembly: ExportRenderer(typeof(DphClusterMap), typeof(DphClusterRenderer))]

namespace goFriend.iOS.Renderers
{
    public class DphMapRenderer : MapRenderer, IMapViewDelegate
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<Marker, DphPin> _mapMarkerPins = new Dictionary<Marker, DphPin>();

        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                if (Control is MapView nativeMap)
                {
                    nativeMap.DraggingMarkerEnded -= OnDraggingMarkerEnded;
                    nativeMap.MarkerInfoContents = null;
                    nativeMap.MarkerInfoWindow = null;
                }
            }

            if (e.NewElement != null)
            {
                //_map = (DphMap)e.NewElement;
                var nativeMap = Control as MapView;
                nativeMap.DraggingMarkerEnded += OnDraggingMarkerEnded;
                nativeMap.MarkerInfoContents = MarkerInfoContents;
                nativeMap.MarkerInfoWindow = MarkerInfoWindow;
                (Map as DphMap)?.MoveToLastPosition();
            }
        }

        private UIView MarkerInfoWindow(MapView mapView, Marker marker)
        {
            return null;
        }

        private UIView MarkerInfoContents(MapView mapView, Marker marker)
        {
            if (!_mapMarkerPins.ContainsKey(marker))
            {
                Logger.Error("Marker not found");
                return null;
            }

            var dphPin = _mapMarkerPins[marker];
            var annotationView = mapView.MarkerInfoWindow.Invoke(mapView, marker);
            if (annotationView == null)
            {
                var leftOutView = new UIImageView(new CGRect(0, 5, 50, 50));
                ImageService.Instance.LoadUrl(dphPin.IconUrl)
                    .Transform(new CircleTransformation())
                    //.WithPriority(LoadingPriority.High)
                    //.WithCache(FFImageLoading.Cache.CacheType.All)
                    .Into(leftOutView);
                var detailCallOutView = new UIStackView
                {
                    Frame = new CGRect(60, 0, 240, 57),
                    Axis = UILayoutConstraintAxis.Vertical,
                    //Distribution = UIStackViewDistribution.FillEqually,
                    Alignment = UIStackViewAlignment.Fill
                };
                var lblTitle = new UILabel
                {
                    Text = dphPin.Title,
                    Font = UIFont.PreferredBody,
                    TextColor = UIColor.Black
                };
                var lblSubTitle1 = new UILabel
                {
                    Text = dphPin.SubTitle1,
                    Font = UIFont.PreferredCaption1,
                    TextColor = UIColor.Gray
                };
                var lblSubTitle2 = new UILabel
                {
                    Text = dphPin.SubTitle2,
                    Font = UIFont.PreferredCaption1,
                    TextColor = UIColor.Gray
                };
                detailCallOutView.AddArrangedSubview(lblTitle);
                detailCallOutView.AddArrangedSubview(lblSubTitle1);
                detailCallOutView.AddArrangedSubview(lblSubTitle2);

                annotationView = new UIView(new CGRect(0, 0, 300, 60));
                annotationView.AddSubview(detailCallOutView);
                annotationView.AddSubview(leftOutView);
            }

            return annotationView;
        }

        private void OnDraggingMarkerEnded(object sender, GMSMarkerEventEventArgs e)
        {
            if (!_mapMarkerPins.ContainsKey(e.Marker))
            {
                Logger.Error("Marker not found");
                return;
            }
            var dphPin = _mapMarkerPins[e.Marker];
            MessagingCenter.Send(Xamarin.Forms.Application.Current, Constants.MsgLocationChanged, dphPin);
        }

        protected override void OnMarkerDeleted(Pin pin, Marker marker)
        {
            _mapMarkerPins.Remove(marker);
        }

        protected override void OnMarkerCreated(Pin pin, Marker marker)
        {
            var dphPin = pin.Tag as DphPin;
            _mapMarkerPins.Add(marker, dphPin);
        }

        protected override void OnMarkerCreating(Pin pin, Marker marker)
        {
            var dphPin = pin.Tag as DphPin;
            marker.Position = new CLLocationCoordinate2D(pin.Position.Latitude, pin.Position.Longitude);
            marker.Title = pin.Label;
            marker.Snippet = pin.Address;
            //marker.Icon = UIImage.FromFile($"pin_{dphPin?.UserRight}.png");
            marker.Icon = dphPin?.UserRight == UserType.Admin ? ExtensionIOS.ICON_PIN_ADMIN :
                dphPin?.UserRight == UserType.Pending ? ExtensionIOS.ICON_PIN_PENDING : ExtensionIOS.ICON_PIN_NORMAL;
            marker.Draggable = pin.IsDraggable;
        }
    }

    public class DphClusterRenderer : ClusteredMapRenderer, IMapViewDelegate
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<Marker, DphPin> _mapMarkerPins = new Dictionary<Marker, DphPin>();

        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                if (Control is MapView nativeMap)
                {
                    nativeMap.MarkerInfoContents = null;
                    nativeMap.MarkerInfoWindow = null;
                }
            }

            if (e.NewElement != null)
            {
                //_map = (DphMap)e.NewElement;
                var nativeMap = Control as MapView;
                nativeMap.MarkerInfoContents = MarkerInfoContents;
                nativeMap.MarkerInfoWindow = MarkerInfoWindow;
                (Map as DphMap)?.MoveToLastPosition();
            }
        }

        private UIView MarkerInfoWindow(MapView mapView, Marker marker)
        {
            return null;
        }

        private UIView MarkerInfoContents(MapView mapView, Marker marker)
        {
            var dphPin = FindPin(marker);
            if (dphPin == null) return null;

            var annotationView = mapView.MarkerInfoWindow.Invoke(mapView, marker);
            if (annotationView == null)
            {
                var leftOutView = new UIImageView(new CGRect(0, 5, 50, 50));
                ImageService.Instance.LoadUrl(dphPin.IconUrl)
                    .Transform(new CircleTransformation())
                    //.WithPriority(LoadingPriority.High)
                    //.WithCache(FFImageLoading.Cache.CacheType.All)
                    .Into(leftOutView);
                var detailCallOutView = new UIStackView
                {
                    Frame = new CGRect(60, 0, 240, 60),
                    Axis = UILayoutConstraintAxis.Vertical,
                    //Distribution = UIStackViewDistribution.FillEqually,
                    Alignment = UIStackViewAlignment.Fill
                };
                var lblTitle = new UILabel
                {
                    Text = dphPin.Title,
                    Font = UIFont.PreferredBody,
                    TextColor = UIColor.Black
                };
                var lblSubTitle1 = new UILabel
                {
                    Text = dphPin.SubTitle1,
                    Font = UIFont.PreferredCaption1,
                    TextColor = UIColor.Gray
                };
                var lblSubTitle2 = new UILabel
                {
                    Text = dphPin.SubTitle2,
                    Font = UIFont.PreferredCaption1,
                    TextColor = UIColor.Gray
                };
                detailCallOutView.AddArrangedSubview(lblTitle);
                detailCallOutView.AddArrangedSubview(lblSubTitle1);
                detailCallOutView.AddArrangedSubview(lblSubTitle2);

                annotationView = new UIView(new CGRect(0, 0, 300, 60));
                annotationView.AddSubview(detailCallOutView);
                annotationView.AddSubview(leftOutView);
            }

            return annotationView;
        }

        private DphPin FindPin(Marker marker)
        {
            if (_mapMarkerPins.ContainsKey(marker))
            {
                return _mapMarkerPins[marker];
            }
            Logger.Warn("ClusteredMarker not found by key");
            foreach (var markerPin in _mapMarkerPins.Where(kvp =>
                Math.Abs(kvp.Value.Position.Latitude - marker.Position.Latitude) < 0.00001
                && Math.Abs(kvp.Value.Position.Longitude - marker.Position.Longitude) < 0.00001))
            {
                Logger.Debug("ClusteredMarker found by position");
                return markerPin.Value;
            }
            Logger.Error("ClusteredMarker not found by key and position");
            return null;
        }

        protected override void OnMarkerDeleted(Pin pin, Marker marker)
        {
            _mapMarkerPins.Remove(marker);
        }

        protected override void OnMarkerCreated(Pin pin, Marker marker)
        {
            var dphPin = pin.Tag as DphPin;
            _mapMarkerPins.Add(marker, dphPin);
        }

        protected override void OnMarkerCreating(Pin pin, Marker marker)
        {
            var dphPin = pin.Tag as DphPin;
            marker.Position = new CLLocationCoordinate2D(pin.Position.Latitude, pin.Position.Longitude);
            marker.Title = pin.Label;
            marker.Snippet = pin.Address;
            //marker.InfoWindowAnchor = new CGPoint(marker.InfoWindowAnchor.X, 0.2);
            marker.InfoWindowAnchor = new CGPoint(marker.InfoWindowAnchor.X, 0);
            //marker.Icon = UIImage.FromFile($"pin_{dphPin?.UserRight}.png");
            marker.Icon = dphPin?.UserRight == UserType.Admin ? ExtensionIOS.ICON_PIN_ADMIN :
                dphPin?.UserRight == UserType.Pending ? ExtensionIOS.ICON_PIN_PENDING : ExtensionIOS.ICON_PIN_NORMAL;
            marker.Draggable = pin.IsDraggable;
        }
    }
}