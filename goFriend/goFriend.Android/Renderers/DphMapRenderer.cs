using System;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Widget;
using FFImageLoading;
using FFImageLoading.Transformations;
using goFriend.Controls;
using goFriend.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Android;

[assembly: ExportRenderer(typeof(DphMap), typeof(DphMapRenderer))]

namespace goFriend.Droid.Renderers
{
    public class DphMapRenderer : MapRenderer, GoogleMap.IInfoWindowAdapter
    {
        //private DphMap _map;

        public DphMapRenderer(Context context) : base(context)
        {
        }

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
        }

        private void MapOnMarkerDragEnd(object sender, GoogleMap.MarkerDragEndEventArgs e)
        {
            var pin = GetPinForMarker(e.Marker);
            pin.Position = new Position(e.Marker.Position.Latitude, e.Marker.Position.Longitude);
            MessagingCenter.Send(Application.Current, Constants.MsgLocationChanged, pin);
        }

        //private void MapOnMarkerDragStart(object sender, GoogleMap.MarkerDragStartEventArgs e)
        //{
        //}

        protected override void OnMapReady(GoogleMap map)
        {
            try
            {
                base.OnMapReady(map);
            }
            catch (Exception)
            {
                //Ignored
            }

            NativeMap.InfoWindowClick += OnInfoWindowClick;
            //NativeMap.MarkerDragStart += MapOnMarkerDragStart;
            NativeMap.MarkerDragEnd += MapOnMarkerDragEnd;
            NativeMap.SetInfoWindowAdapter(this);
        }

        protected override MarkerOptions CreateMarker(Pin pin)
        {
            var dphPin = (DphPin)pin;
            var marker = new MarkerOptions();
            marker.SetPosition(new LatLng(pin.Position.Latitude, pin.Position.Longitude));
            marker.SetTitle(pin.Label);
            marker.SetSnippet(pin.Address);
            marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.pin));
            marker.Draggable(dphPin.Draggable);
            return marker;
        }

        void OnInfoWindowClick(object sender, GoogleMap.InfoWindowClickEventArgs e)
        {
            //var pin = GetPinForMarker(e.Marker) as DphPin;

            //if (!string.IsNullOrWhiteSpace(pin.Url))
            //{
            //    var url = Android.Net.Uri.Parse(pin.Url);
            //    var intent = new Intent(Intent.ActionView, url);
            //    intent.AddFlags(ActivityFlags.NewTask);
            //    Android.App.Application.Context.StartActivity(intent);
            //}
        }

        public Android.Views.View GetInfoContents(Marker marker)
        {
            var inflater = Android.App.Application.Context.GetSystemService(Context.LayoutInflaterService) as Android.Views.LayoutInflater;
            if (inflater != null)
            {
                var pin = (DphPin)GetPinForMarker(marker);

                var view = inflater.Inflate(Resource.Layout.MapFriendInfoWindow, null);

                var infoIcon = view.FindViewById<ImageView>(Resource.Id.InfoWindowIcon);
                var infoTitle = view.FindViewById<TextView>(Resource.Id.InfoWindowTitle);
                var infoSubtitle = view.FindViewById<TextView>(Resource.Id.InfoWindowSubtitle);

                infoTitle.Text = pin.Title; //marker.Title
                infoSubtitle.Text = $"{pin.SubTitle1}{Environment.NewLine}{pin.SubTitle2}"; //marker.Snippet

                //ImageService.Instance.LoadUrl(pin.IconUrl).DownloadOnly();
                //ImageService.Instance.LoadUrl(pin.IconUrl).Preload();
                ImageService.Instance.LoadUrl(pin.IconUrl)
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