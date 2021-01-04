using goFriend.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Controls
{
    public enum OverlapType
    {
        Notification,
        GroupChat
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DphOverlapImage : ContentView
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        public static readonly BindableProperty Source1Property =
            BindableProperty.CreateAttached(nameof(Source1), typeof(string), typeof(DphOverlapImage),
                null, defaultBindingMode: BindingMode.TwoWay, propertyChanged: OnSource1Changed);
        public static readonly BindableProperty Source2Property =
            BindableProperty.CreateAttached(nameof(Source2), typeof(string), typeof(DphOverlapImage),
                null, defaultBindingMode: BindingMode.TwoWay, propertyChanged: OnSource2Changed);
        public static readonly BindableProperty OverlapTypeProperty =
            BindableProperty.CreateAttached(nameof(OverlapType), typeof(OverlapType), typeof(DphOverlapImage),
                OverlapType.Notification, defaultBindingMode: BindingMode.TwoWay, propertyChanged: OnOverlapTypeChanged);

        public static readonly BindableProperty Source1XConstraintFactorProperty =
            BindableProperty.CreateAttached(nameof(Source1XConstraintFactor), typeof(double), typeof(DphOverlapImage), 0d);
        public static readonly BindableProperty Source1YConstraintFactorProperty =
            BindableProperty.CreateAttached(nameof(Source1YConstraintFactor), typeof(double), typeof(DphOverlapImage), 0d);
        public static readonly BindableProperty Source1SizeConstraintFactorProperty =
            BindableProperty.CreateAttached(nameof(Source1SizeConstraintFactor), typeof(double), typeof(DphOverlapImage), 1d);
        public static readonly BindableProperty Source2XConstraintFactorProperty =
            BindableProperty.CreateAttached(nameof(Source2XConstraintFactor), typeof(double), typeof(DphOverlapImage), 0d);
        public static readonly BindableProperty Source2YConstraintFactorProperty =
            BindableProperty.CreateAttached(nameof(Source2YConstraintFactor), typeof(double), typeof(DphOverlapImage), 0d);
        public static readonly BindableProperty Source2SizeConstraintFactorProperty =
            BindableProperty.CreateAttached(nameof(Source2SizeConstraintFactor), typeof(double), typeof(DphOverlapImage), 1d);

        private static void OnSource1Changed(BindableObject bindable, object oldValue, object newValue)
        {
            var obj = (DphOverlapImage)bindable;
            if (newValue == null)
            {
                obj.CachedImage1.IsVisible = obj.Image1.IsVisible = false;
                return;
            }
            var sourceValue = (string)newValue;
            obj.CachedImage1.IsVisible = sourceValue.Contains("//");
            obj.Image1.IsVisible = !obj.CachedImage1.IsVisible;
            //if image from an URL (http, https, resource)
            if (obj.CachedImage1.IsVisible)
            {
                obj.CachedImage1.Source = sourceValue;
            }
            else // if Image is a character from a Font
            {
                var sourceIconChar = sourceValue.Split(DataModel.Extension.Sep)[0];
                var sourceColor = sourceValue.Split(DataModel.Extension.Sep)[1];
                obj.Image1.BackgroundColor = Color.Transparent;
                obj.Image1.FillColor = Color.FromHex(sourceColor);
                obj.FontImageSource1.Glyph = sourceIconChar;
            }
        }

        private static void OnSource2Changed(BindableObject bindable, object oldValue, object newValue)
        {
            var obj = (DphOverlapImage)bindable;
            if (newValue == null)
            {
                obj.CachedImage2.IsVisible = obj.Image2.IsVisible = false;
                return;
            }
            var sourceValue = (string)newValue;
            obj.CachedImage2.IsVisible = sourceValue.Contains("//");
            obj.Image2.IsVisible = !obj.CachedImage2.IsVisible;
            //if image from an URL (http, https, resource)
            if (obj.CachedImage2.IsVisible)
            {
                obj.CachedImage2.Source = sourceValue;
            }
            else // if Image is a character from a Font
            {
                var sourceIconChar = sourceValue.Split(DataModel.Extension.Sep)[0];
                var sourceColor = sourceValue.Split(DataModel.Extension.Sep)[1];
                obj.Image2.BackgroundColor = Color.Transparent;
                obj.Image2.FillColor = Color.FromHex(sourceColor);
                obj.FontImageSource2.Glyph = sourceIconChar;
            }
        }

        private static void OnOverlapTypeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var obj = (DphOverlapImage)bindable;
            //Logger.Debug($"OnOverlapTypeChanged");
            obj.RefreshLayout();
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

        public OverlapType OverlapType
        {
            get => (OverlapType)GetValue(OverlapTypeProperty);
            set => SetValue(OverlapTypeProperty, value);
        }

        public double Source1XConstraintFactor
        {
            get => (double)GetValue(Source1XConstraintFactorProperty);
            set => SetValue(Source1XConstraintFactorProperty, value);
        }

        public double Source1YConstraintFactor
        {
            get => (double)GetValue(Source1YConstraintFactorProperty);
            set => SetValue(Source1YConstraintFactorProperty, value);
        }

        public double Source1SizeConstraintFactor
        {
            get => (double)GetValue(Source1SizeConstraintFactorProperty);
            set => SetValue(Source1SizeConstraintFactorProperty, value);
        }

        public double Source2XConstraintFactor
        {
            get => (double)GetValue(Source2XConstraintFactorProperty);
            set => SetValue(Source2XConstraintFactorProperty, value);
        }

        public double Source2YConstraintFactor
        {
            get => (double)GetValue(Source2YConstraintFactorProperty);
            set => SetValue(Source2YConstraintFactorProperty, value);
        }

        public double Source2SizeConstraintFactor
        {
            get => (double)GetValue(Source2SizeConstraintFactorProperty);
            set => SetValue(Source2SizeConstraintFactorProperty, value);
        }

        public DphOverlapImage()
        {
            InitializeComponent();

            //Logger.Debug($"DphOverlapImage initializing...");
            RefreshLayout();
        }

        public void RefreshLayout()
        {
            //Logger.Debug($"RefreshLayout.OverlapType={OverlapType}");

            switch (OverlapType)
            {
                case OverlapType.Notification:
                    Source1XConstraintFactor = Source1YConstraintFactor = 0;
                    Source1SizeConstraintFactor = 1;
                    Source2XConstraintFactor = Source2YConstraintFactor = 0.55;
                    Source2SizeConstraintFactor = 0.42;
                    break;
                case OverlapType.GroupChat:
                    Source1XConstraintFactor = 0.25;
                    Source1YConstraintFactor = 0;
                    Source2XConstraintFactor = 0;
                    Source2YConstraintFactor = 0.25;
                    Source1SizeConstraintFactor = Source2SizeConstraintFactor = 0.75;
                    break;
            }

            RelativeLayout.Children.Clear();

            //Logger.Debug($"Source2XConstraintFactor={Source2XConstraintFactor}, Source2YConstraintFactor={Source2YConstraintFactor}, Source2SizeConstraintFactor={Source2SizeConstraintFactor}");
            RelativeLayout.Children.Add(Image1,
                Constraint.RelativeToParent(p => Source1XConstraintFactor * p.Width),
                Constraint.RelativeToParent(p => Source1YConstraintFactor * p.Height),
                Constraint.RelativeToParent(p => Source1SizeConstraintFactor * p.Width),
                Constraint.RelativeToParent(p => Source1SizeConstraintFactor * p.Height));
            RelativeLayout.Children.Add(CachedImage1,
                Constraint.RelativeToParent(p => Source1XConstraintFactor * p.Width),
                Constraint.RelativeToParent(p => Source1YConstraintFactor * p.Height),
                Constraint.RelativeToParent(p => Source1SizeConstraintFactor * p.Width),
                Constraint.RelativeToParent(p => Source1SizeConstraintFactor * p.Height));

            RelativeLayout.Children.Add(Image2,
                Constraint.RelativeToParent(p => Source2XConstraintFactor * p.Width),
                Constraint.RelativeToParent(p => Source2YConstraintFactor * p.Height),
                Constraint.RelativeToParent(p => Source2SizeConstraintFactor * p.Width),
                Constraint.RelativeToParent(p => Source2SizeConstraintFactor * p.Height));
            RelativeLayout.Children.Add(CachedImage2,
                Constraint.RelativeToParent(p => Source2XConstraintFactor * p.Width),
                Constraint.RelativeToParent(p => Source2YConstraintFactor * p.Height),
                Constraint.RelativeToParent(p => Source2SizeConstraintFactor * p.Width),
                Constraint.RelativeToParent(p => Source2SizeConstraintFactor * p.Height));
        }
    }
}