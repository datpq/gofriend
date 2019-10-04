using System.ComponentModel;
using Android.Graphics;
using Android.Widget;
using goFriend.Controls;
using goFriend.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(IconView), typeof(IconViewRenderer))]

namespace goFriend.Droid.Renderers
{
    public class IconViewRenderer : ViewRenderer<IconView, ImageView>
    {
        private bool _isDisposed;

        public IconViewRenderer(Android.Content.Context context) : base(context)
        {
            AutoPackage = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            base.Dispose(disposing);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<IconView> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement == null)
            {
                SetNativeControl(new ImageView(Context));
            }
            UpdateBitmap();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == IconView.SourceProperty.PropertyName)
            {
                UpdateBitmap();
            }
            else if (e.PropertyName == IconView.ForegroundProperty.PropertyName)
            {
                UpdateBitmap();
            }
        }

        private void UpdateBitmap()
        {
            if (!_isDisposed && !string.IsNullOrWhiteSpace(Element.Source))
            {
                var d = Resources.GetDrawable(Element.Source).Mutate();
                d.SetColorFilter(new LightingColorFilter(Element.Foreground.ToAndroid(), Element.Foreground.ToAndroid()));
                d.Alpha = Element.Foreground.ToAndroid().A;
                Control.SetImageDrawable(d);
                ((IVisualElementController)Element).NativeSizeChanged();
            }
        }
    }
}
