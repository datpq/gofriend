using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Views;
using Android.Widget;
using FFImageLoading;
using FFImageLoading.Transformations;
using goFriend.Controls;
using goFriend.DataModel;
using goFriend.Droid.Renderers;
using NLog;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.GoogleMaps.Android;
using Xamarin.Forms.GoogleMaps.Clustering.Android;
using BitmapDescriptorFactory = Android.Gms.Maps.Model.BitmapDescriptorFactory;
using Map = Xamarin.Forms.GoogleMaps.Map;
using View = Android.Views.View;

[assembly: ExportRenderer(typeof(DphMap), typeof(DphMapRenderer))]
[assembly: ExportRenderer(typeof(DphClusterMap), typeof(DphClusterRenderer))]

namespace goFriend.Droid.Renderers
{
    public class DphMapRenderer : MapRenderer, GoogleMap.IInfoWindowAdapter, IOnMapReadyCallback
    {
        private readonly Dictionary<string, DphPin> _mapMarkerPins = new Dictionary<string, DphPin>();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DphMapRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<Map> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                NativeMap.InfoWindowClick -= OnInfoWindowClick;
                //NativeMap.MarkerDragStart -= MapOnMarkerDragStart;
                NativeMap.MarkerDragEnd -= MapOnMarkerDragEnd;
            }

            if (e.NewElement != null)
            {
                //_map = (DphMap)e.NewElement;
            }

            ((MapView)Control)?.GetMapAsync(this);
        }

        //protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    base.OnElementPropertyChanged(sender, e);

        //    if (Element == null || Control == null)
        //        return;

        //    //if (e.PropertyName == _dphMap.CustomPinsProperty.PropertyName || e.PropertyName == _dphMap.ZoomLevelProperty.PropertyName)
        //    //    UpdatePins();
        //}

        private void MapOnMarkerDragEnd(object sender, GoogleMap.MarkerDragEndEventArgs e)
        {
            if (!_mapMarkerPins.ContainsKey(e.Marker.Id))
            {
                Logger.Error("Marker not found");
                return;
            }
            var dphPin = _mapMarkerPins[e.Marker.Id];
            MessagingCenter.Send(Application.Current, Constants.MsgLocationChanged, dphPin);
        }

        //private void MapOnMarkerDragStart(object sender, GoogleMap.MarkerDragStartEventArgs e)
        //{
        //}

        public void OnMapReady(GoogleMap googleMap)
        {
            NativeMap.InfoWindowClick += OnInfoWindowClick;
            //NativeMap.MarkerDragStart += MapOnMarkerDragStart;
            NativeMap.MarkerDragEnd += MapOnMarkerDragEnd;
            NativeMap.SetInfoWindowAdapter(this);
            (Map as DphMap)?.MoveToLastPosition();
        }

        //Important: Do not override this Method (MoveToRegion will not work)
        //protected override void OnMapReady(GoogleMap nativeMap, Map map)
        //{
        //    OnMapReady(nativeMap);
        //}

        protected override void OnMarkerDeleted(Pin pin, Marker marker)
        {
            _mapMarkerPins.Remove(marker.Id);
        }

        protected override void OnMarkerCreated(Pin pin, Marker marker)
        {
            var dphPin = pin.Tag as DphPin;
            _mapMarkerPins.Add(marker.Id, dphPin);
        }

        protected override void OnMarkerCreating(Pin pin, MarkerOptions markerOptions)
        {
            var dphPin = pin.Tag as DphPin;
            //var marker = new MarkerOptions();
            markerOptions.SetPosition(new LatLng(pin.Position.Latitude, pin.Position.Longitude));
            markerOptions.SetTitle(pin.Label);
            markerOptions.SetSnippet(pin.Address);
            var pinIcon = dphPin?.UserRight == UserType.Admin ? (dphPin?.User.IsFemale == true ? ExtensionAndroid.ICON_PIN_ADMIN_FEMALE : ExtensionAndroid.ICON_PIN_ADMIN) :
                dphPin?.UserRight == UserType.Pending ? (dphPin?.User.IsFemale == true ? ExtensionAndroid.ICON_PIN_PENDING_FEMALE : ExtensionAndroid.ICON_PIN_PENDING)
                : (dphPin?.User.IsFemale == true ? ExtensionAndroid.ICON_PIN_NORMAL_FEMALE : ExtensionAndroid.ICON_PIN_NORMAL);
            markerOptions.SetIcon(BitmapDescriptorFactory.FromBitmap(pinIcon));
            //markerOptions.SetIcon(BitmapDescriptorFactory.FromResource(
            //        dphPin?.UserRight == UserType.Admin ? Resource.Drawable.pin_Admin :
            //        dphPin?.UserRight == UserType.Pending ? Resource.Drawable.pin_Pending : Resource.Drawable.pin_Normal));
            markerOptions.Draggable(pin.IsDraggable);
        }

        void OnInfoWindowClick(object sender, GoogleMap.InfoWindowClickEventArgs e)
        {
            if (!_mapMarkerPins.ContainsKey(e.Marker.Id))
            {
                Logger.Error("Marker not found");
                return;
            }
            var dphPin = _mapMarkerPins[e.Marker.Id];
            //if (string.IsNullOrWhiteSpace(dphPin?.Url)) return;
            //try
            //{
            //    //Android.App.Application.Context.PackageManager.GetPackageInfo("com.facebook.katana", 0);
            //    var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(pin.Url));
            //    intent.AddFlags(ActivityFlags.NewTask);
            //    Android.App.Application.Context.StartActivity(intent);
            //}
            //catch (Exception)
            //{
            //    // ignored
            //}
            //Launcher.OpenAsync(new Uri(dphPin.Url));
            MessagingCenter.Send((DphMap)Map, Constants.MsgInfoWindowClick, dphPin);
        }

        public View GetInfoContents(Marker marker)
        {
            if (Android.App.Application.Context.GetSystemService(Context.LayoutInflaterService) is LayoutInflater inflater)
            {
                if (!_mapMarkerPins.ContainsKey(marker.Id))
                {
                    Logger.Error("Marker not found");
                    return null;
                }
                var dphPin = _mapMarkerPins[marker.Id];

                var view = inflater.Inflate(Resource.Layout.MapFriendInfoWindow, null);

                var infoIcon = view.FindViewById<ImageView>(Resource.Id.InfoWindowIcon);
                var infoTitle = view.FindViewById<TextView>(Resource.Id.InfoWindowTitle);
                var infoSubtitle = view.FindViewById<TextView>(Resource.Id.InfoWindowSubtitle);

                infoTitle.Text = dphPin.Title; //marker.Title
                infoSubtitle.Text = $"{dphPin.SubTitle1}{Environment.NewLine}{dphPin.SubTitle2}"; //marker.Snippet

                //ImageService.Instance.LoadUrl(pin.IconUrl).DownloadOnly();
                //ImageService.Instance.LoadUrl(pin.IconUrl).Preload();
                ImageService.Instance.LoadUrl(dphPin.IconUrl)
                    .Transform(new CircleTransformation())
                    //.WithPriority(LoadingPriority.High)
                    //.WithCache(FFImageLoading.Cache.CacheType.All)
                    .Into(infoIcon);
                //ImageService.Instance.LoadUrl(pin.IconUrl)
                //    .Transform(new CircleTransformation())
                //    .Success(() =>
                //    {
                //        Device.BeginInvokeOnMainThread(() =>
                //        {
                //            infoIcon.RefreshDrawableState();
                //        });
                //    }).Into(infoIcon);

                return view;
            }
            return null;
        }

        public View GetInfoWindow(Marker marker)
        {
            return null;
        }
    }

    public class DphClusterRenderer : ClusteredMapRenderer, GoogleMap.IInfoWindowAdapter, IOnMapReadyCallback
    {
        private readonly Dictionary<string, DphPin> _mapMarkerPins = new Dictionary<string, DphPin>();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DphClusterRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<Map> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                NativeMap.InfoWindowClick -= OnInfoWindowClick;
                //NativeMap.InfoWindowLongClick -= OnInfoWindowLongClick;
            }

            ((MapView)Control)?.GetMapAsync(this);
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            NativeMap.InfoWindowClick += OnInfoWindowClick;
            //NativeMap.InfoWindowLongClick += OnInfoWindowLongClick;
            NativeMap.SetInfoWindowAdapter(this);
            (Map as DphClusterMap)?.MoveToLastPosition();
        }

        protected override void OnClusteredMarkerDeleted(Pin pin, ClusteredMarker clusteredMarker)
        {
            if (_mapMarkerPins.Count == 0) return;
            var dphPin = FindPin(clusteredMarker.Id, clusteredMarker.Position);
            if (dphPin != null)
            {
                foreach (var item in _mapMarkerPins.Where(kvp => kvp.Value == dphPin).ToList())
                {
                    _mapMarkerPins.Remove(item.Key);
                    return;
                }
            }
        }

        private DphPin FindPin(string id, LatLng pos)
        {
            if (_mapMarkerPins.ContainsKey(id))
            {
                return _mapMarkerPins[id];
            }
            Logger.Warn("ClusteredMarker not found by key");
            foreach (var markerPin in _mapMarkerPins.Where(kvp =>
                Math.Abs(kvp.Value.Position.Latitude - pos.Latitude) < 0.00001
                && Math.Abs(kvp.Value.Position.Longitude - pos.Longitude) < 0.00001))
            {
                Logger.Debug("ClusteredMarker found by position");
                return markerPin.Value;
            }
            Logger.Error("ClusteredMarker not found by key and position");
            return null;
        }

        protected override void OnClusteredMarkerCreated(Pin pin, ClusteredMarker clusteredMarker)
        {
            var dphPin = pin.Tag as DphPin;
            clusteredMarker.InfoWindowAnchorY = 0;
            var pinIcon = dphPin?.UserRight == UserType.Admin ? (dphPin?.User.IsFemale == true ? ExtensionAndroid.ICON_PIN_ADMIN_FEMALE : ExtensionAndroid.ICON_PIN_ADMIN) :
                dphPin?.UserRight == UserType.Pending ? (dphPin?.User.IsFemale == true ? ExtensionAndroid.ICON_PIN_PENDING_FEMALE : ExtensionAndroid.ICON_PIN_PENDING)
                : (dphPin?.User.IsFemale == true ? ExtensionAndroid.ICON_PIN_NORMAL_FEMALE : ExtensionAndroid.ICON_PIN_NORMAL);
            clusteredMarker.Icon = BitmapDescriptorFactory.FromBitmap(pinIcon);
            //clusteredMarker.Icon = BitmapDescriptorFactory.FromResource(
            //    dphPin?.UserRight == UserType.Admin ? Resource.Drawable.pin_Admin :
            //    dphPin?.UserRight == UserType.Pending ? Resource.Drawable.pin_Pending : Resource.Drawable.pin_Normal);
            _mapMarkerPins.Add(clusteredMarker.Id, dphPin);
        }

        //protected override void OnClusteredMarkerCreating(Pin pin, MarkerOptions clusteredMarker)
        //{
        //    clusteredMarker.SetPosition(new LatLng(pin.Position.Latitude, pin.Position.Longitude));
        //    clusteredMarker.SetTitle(pin.Label);
        //    clusteredMarker.SetSnippet(pin.Address);
        //    clusteredMarker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.pin));
        //    clusteredMarker.Draggable(pin.IsDraggable);
        //}

        void OnInfoWindowClick(object sender, GoogleMap.InfoWindowClickEventArgs e)
        {
            var dphPin = FindPin(e.Marker.Id, e.Marker.Position);
            //if (string.IsNullOrWhiteSpace(dphPin?.Url)) return;

            //Launcher.OpenAsync(new Uri(dphPin.Url));
            MessagingCenter.Send((DphClusterMap)Map, Constants.MsgInfoWindowClick, dphPin);
        }

        //private void OnInfoWindowLongClick(object sender, GoogleMap.InfoWindowLongClickEventArgs e)
        //{
        //    var dphPin = FindPin(e.Marker.Id, e.Marker.Position);
        //    MessagingCenter.Send(Application.Current, Constants.MsgInfoWindowLongClick, dphPin);
        //}

        public View GetInfoContents(Marker marker)
        {
            if (Android.App.Application.Context.GetSystemService(Context.LayoutInflaterService) is LayoutInflater inflater)
            {
                var dphPin = FindPin(marker.Id, marker.Position);
                if (dphPin == null) return null;

                var view = inflater.Inflate(Resource.Layout.MapFriendInfoWindow, null);

                var infoIcon = view.FindViewById<ImageView>(Resource.Id.InfoWindowIcon);
                var infoTitle = view.FindViewById<TextView>(Resource.Id.InfoWindowTitle);
                var infoSubtitle = view.FindViewById<TextView>(Resource.Id.InfoWindowSubtitle);

                infoTitle.Text = dphPin.Title; //marker.Title
                infoSubtitle.Text = $"{dphPin.SubTitle1}{Environment.NewLine}{dphPin.SubTitle2}"; //marker.Snippet

                ImageService.Instance.LoadUrl(dphPin.IconUrl)
                    .Transform(new CircleTransformation())
                    //.WithPriority(LoadingPriority.High)
                    //.WithCache(FFImageLoading.Cache.CacheType.All)
                    .Into(infoIcon);

                return view;
            }
            return null;
        }

        public View GetInfoWindow(Marker marker)
        {
            return null;
        }
    }
}