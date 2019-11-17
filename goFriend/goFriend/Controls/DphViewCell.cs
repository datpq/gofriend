using Xamarin.Forms;

namespace goFriend.Controls
{
    public class DphViewCell : ViewCell
    {
        public static readonly BindableProperty SelectedItemBackgroundColorProperty =
            BindableProperty.Create("SelectedItemBackgroundColor", typeof(Color), typeof(DphViewCell),
                (Color)Application.Current.Resources["ColorListViewSelectedItem"]);
        public Color SelectedItemBackgroundColor
        {
            get => (Color)GetValue(SelectedItemBackgroundColorProperty);
            set => SetValue(SelectedItemBackgroundColorProperty, value);
        }
    }
}
