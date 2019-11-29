using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DphOverlapImage : ContentView
    {
        public static readonly BindableProperty Source1Property = BindableProperty.CreateAttached(nameof(Source1), typeof(string), typeof(DphOverlapImage), null,
            defaultBindingMode: BindingMode.TwoWay, propertyChanged: OnSource1Changed);
        public static readonly BindableProperty Source2Property = BindableProperty.CreateAttached(nameof(Source2), typeof(string), typeof(DphOverlapImage), null,
            defaultBindingMode: BindingMode.TwoWay, propertyChanged: OnSource2Changed);

        private static void OnSource1Changed(BindableObject bindable, object oldValue, object newValue)
        {
            var obj = (DphOverlapImage)bindable;
            obj.Image1.Source = (string)newValue;
        }

        private static void OnSource2Changed(BindableObject bindable, object oldValue, object newValue)
        {
            var obj = (DphOverlapImage)bindable;
            obj.Image2.Source = (string)newValue;
        }

        public string Source1
        {
            get => (string)GetValue(Source1Property);
            set => SetValue(Source1Property, value);
        }

        public string Source2
        {
            get => (string)GetValue(Source2Property);
            set => SetValue(Source2Property, value);
        }

        public DphOverlapImage()
        {
            InitializeComponent();
        }
    }
}