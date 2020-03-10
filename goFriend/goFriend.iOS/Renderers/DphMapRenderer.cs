using CoreGraphics;
using FFImageLoading;
using FFImageLoading.Transformations;
using goFriend.Controls;
using goFriend.iOS.Renderers;
using MapKit;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.iOS;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(DphMap), typeof(DphMapRenderer))]
namespace goFriend.iOS.Renderers
{
    public class DphMapRenderer : MapRenderer
    {
        private UIView _pinView;
        //private DphMap _map;
        //private ILogger _logger = DependencyService.Get<ILogManager>().GetLog();

        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                var nativeMap = Control as MKMapView;
                if (nativeMap != null)
                {
                    nativeMap.RemoveAnnotations(nativeMap.Annotations);
                    nativeMap.GetViewForAnnotation = null;
                    nativeMap.ChangedDragState -= OnChangedDragState;
                    nativeMap.CalloutAccessoryControlTapped -= OnCalloutAccessoryControlTapped;
                    nativeMap.DidSelectAnnotationView -= OnDidSelectAnnotationView;
                    nativeMap.DidDeselectAnnotationView -= OnDidDeselectAnnotationView;
                }
            }

            if (e.NewElement != null)
            {
                //_map = (DphMap)e.NewElement;
                var nativeMap = Control as MKMapView;

                nativeMap.GetViewForAnnotation = GetViewForAnnotation;
                nativeMap.ChangedDragState += OnChangedDragState;
                nativeMap.CalloutAccessoryControlTapped += OnCalloutAccessoryControlTapped;
                nativeMap.DidSelectAnnotationView += OnDidSelectAnnotationView;
                nativeMap.DidDeselectAnnotationView += OnDidDeselectAnnotationView;
            }
        }

        private void OnChangedDragState(object sender, MKMapViewDragStateEventArgs e)
        {
            if (e.NewState == MKAnnotationViewDragState.Starting)
            {
                e.AnnotationView.DragState = MKAnnotationViewDragState.Dragging;
            } else if (e.NewState == MKAnnotationViewDragState.Ending || e.NewState == MKAnnotationViewDragState.Canceling)
            {
                var pin = GetPinForAnnotation(e.AnnotationView.Annotation);
                pin.Position = new Position(e.AnnotationView.Annotation.Coordinate.Latitude, e.AnnotationView.Annotation.Coordinate.Longitude);
                MessagingCenter.Send(Xamarin.Forms.Application.Current, Constants.MsgLocationChanged, pin);
                e.AnnotationView.DragState = MKAnnotationViewDragState.None;
            }
        }

        protected override MKAnnotationView GetViewForAnnotation(MKMapView mapView, IMKAnnotation annotation)
        {
            var map = (DphMap) Element;
            var pin = GetPinForAnnotation(annotation);
            //var pin = GetCustomPin(annotation as MKPointAnnotation);
            if (pin == null)
            {
                return null;
            }

            var dphPin = map.CustomPins[pin];

            var annotationView = mapView.DequeueReusableAnnotation(pin.Label);
            if (annotationView == null)
            {
                var leftOutView = new UIImageView(new CGRect(0, 0, 50, 50));
                ImageService.Instance.LoadUrl(dphPin.IconUrl)
                    .Transform(new CircleTransformation())
                    //.WithPriority(LoadingPriority.High)
                    //.WithCache(FFImageLoading.Cache.CacheType.All)
                    .Into(leftOutView);
                var detailCallOutView = new UIStackView
                {
                    Axis = UILayoutConstraintAxis.Vertical,
                    Distribution = UIStackViewDistribution.FillEqually,
                    Alignment = UIStackViewAlignment.Fill
                };
                var lblSubTitle1 = new UILabel {
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
                detailCallOutView.AddArrangedSubview(lblSubTitle1);
                detailCallOutView.AddArrangedSubview(lblSubTitle2);
                annotationView = new CustomAnnotationView(annotation, pin.Label)
                {
                    Image = UIImage.FromFile("pin.png"),
                    Draggable = dphPin.Draggable,
                    CalloutOffset = new CGPoint(0, 0),
                    DetailCalloutAccessoryView = detailCallOutView,
                    LeftCalloutAccessoryView = leftOutView //new UIImageView(FromUrl(pin.IconUrl))
                };
                //annotationView.RightCalloutAccessoryView = UIButton.FromType(UIButtonType.DetailDisclosure);
            }
            annotationView.CanShowCallout = true;

            return annotationView;
        }

        private void OnDidDeselectAnnotationView(object sender, MKAnnotationViewEventArgs e)
        {
            if (!e.View.Selected)
            {
                _pinView.RemoveFromSuperview();
                _pinView.Dispose();
                _pinView = null;
            }
        }

        private void OnDidSelectAnnotationView(object sender, MKAnnotationViewEventArgs e)
        {
            //var customView = e.View as CustomAnnotationView;
            _pinView = new UIView();

            _pinView.Frame = new CGRect(0, 0, 200, 84);
            //var image = new UIImageView(new CGRect(0, 0, 200, 84));
            //image.Image = UIImage.FromFile("hn9194_25.png");
            //_pinView.AddSubview(image);
            _pinView.Center = new CGPoint(0, -(e.View.Frame.Height + 75));
            e.View.AddSubview(_pinView);
        }

        private void OnCalloutAccessoryControlTapped(object sender, MKMapViewAccessoryTappedEventArgs e)
        {
        }

        //static UIImage FromUrl(string uri)
        //{
        //    using (var url = new NSUrl(uri))
        //    using (var data = NSData.FromUrl(url))
        //        return UIImage.LoadFromData(data);
        //}
    }
}