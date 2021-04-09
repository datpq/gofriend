using Xamarin.Essentials;
using Xamarin.Forms;
using Plugin.LatestVersion;

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

        protected override async void OnAppearing()
        {
            SpanLatestVersion.Text = (await CrossLatestVersion.Current.IsUsingLatestVersion()) ?
                res.LatestVersionInfo : string.Format(res.LatestVersionWarn, await CrossLatestVersion.Current.GetLatestVersionNumber());
        }
    }
}