using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UcLabelEntry : ContentView
    {
        public UcLabelEntry()
        {
            InitializeComponent();
            var labelTextColor = LblMain.TextColor;
            TxtMain.Focused += (s, e) => { LblMain.TextColor = (Color) Application.Current.Resources["ColorPrimary"]; };
            TxtMain.Unfocused += (s, e) => { LblMain.TextColor = labelTextColor; };
            TxtMain.TextChanged += (s, e) =>{ TextChanged?.Invoke(s, e); };
        }

        public event System.EventHandler<TextChangedEventArgs> TextChanged;

        public string LabelText
        {
            get => LblMain.Text;
            set => LblMain.Text = value;
        }

        public string EntryText
        {
            get => TxtMain.Text;
            set => TxtMain.Text = value;
        }

        public bool IsEnabled
        {
            get => TxtMain.IsEnabled;
            set => TxtMain.IsEnabled = value;
        }
    }
}