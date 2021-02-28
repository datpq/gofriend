using Xamarin.Essentials;
using Xamarin.Forms;

namespace goFriend.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();

            SpanTitle.Text = AppInfo.Name;
            SpanVersion.Text = $"{AppInfo.VersionString}.{AppInfo.BuildString}";
        }
    }
}