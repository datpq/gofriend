using System;
using System.Collections.Generic;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Widget;
using FFImageLoading;
using FFImageLoading.Transformations;
using goFriend.Controls;
using goFriend.Droid.Renderers;
using NLog;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.GoogleMaps.Android;
using BitmapDescriptorFactory = Android.Gms.Maps.Model.BitmapDescriptorFactory;
using Map = Xamarin.Forms.GoogleMaps.Map;

[assembly: ExportRenderer(typeof(DphMap), typeof(DphMapRenderer))]

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
            //var marker = new MarkerOptions();
            markerOptions.SetPosition(new LatLng(pin.Position.Latitude, pin.Position.Longitude));
            markerOptions.SetTitle(pin.Label);
            markerOptions.SetSnippet(pin.Address);
            markerOptions.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.pin));
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
            if (string.IsNullOrWhiteSpace(dphPin?.Url)) return;
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
            Launcher.OpenAsync(new Uri(dphPin.Url));
        }

        public Android.Views.View GetInfoContents(Marker marker)
        {
            var inflater = Android.App.Application.Context.GetSystemService(Context.LayoutInflaterService) as Android.Views.LayoutInflater;
            if (inflater != null)
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

        public Android.Views.View GetInfoWindow(Marker marker)
        {
            return null;
        }
    }
}