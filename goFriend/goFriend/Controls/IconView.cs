using Xamarin.Forms;

namespace goFriend.Controls
{
    public class IconView : View
    {
        #region ForegroundProperty

        public static readonly BindableProperty ForegroundProperty = BindableProperty.Create(nameof(Foreground), typeof(Color), typeof(IconView), default(Color));

        public Color Foreground
        {
            get => (Color)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        #endregion

        #region SourceProperty

        public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(string), typeof(IconView), default(string));

        public string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        #endregion
    }
}
