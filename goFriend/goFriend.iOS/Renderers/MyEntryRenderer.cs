using CoreAnimation;
using CoreGraphics;
using goFriend.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

//[assembly: ExportRenderer(typeof(Entry), typeof(MyEntryRenderer))]
namespace goFriend.iOS.Renderers
{
    public class MyEntryRenderer : EntryRenderer
    {
        private CALayer _line;

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            _line = null;

            if (Control == null || e.NewElement == null)
                return;

            Control.BorderStyle = UITextBorderStyle.None;

            _line = new CALayer
            {
                BorderColor = ((Color)Xamarin.Forms.Application.Current.Resources["ColorPrimary"]).ToCGColor(),
                BackgroundColor = ((Color)Xamarin.Forms.Application.Current.Resources["ColorPrimary"]).ToCGColor(),
                Frame = new CGRect(0, Frame.Height / 2, Frame.Width * 2, 1f)
            };

            Control.Layer.AddSublayer(_line);
        }
    }
}