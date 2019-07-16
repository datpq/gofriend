using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using goFriend.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Color = Xamarin.Forms.Color;

//[assembly: ExportRenderer(typeof(Entry), typeof(MyEntryRenderer))]
namespace goFriend.Droid.Renderers
{
    public class MyEntryRenderer : EntryRenderer
    {
        public MyEntryRenderer(Android.Content.Context context) : base(context){ }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control == null || e.NewElement == null) return;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                Control.BackgroundTintList = ColorStateList.ValueOf(((Color)Application.Current.Resources["ColorPrimary"]).ToAndroid());
            else
                Control.Background.SetColorFilter(((Color)Application.Current.Resources["ColorPrimary"]).ToAndroid(), PorterDuff.Mode.SrcAtop);
            Control.SetPadding(Control.PaddingLeft, 0, Control.PaddingRight, Control.PaddingBottom);
        }
    }
}