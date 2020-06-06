using goFriend.Controls;
using goFriend.DataModel;
using goFriend.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MediaPage : ContentPage
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public MediaPage(ChatMessage chatMessage)
        {
            InitializeComponent();
            BindingContext = chatMessage;
            Logger.Debug($"Attachments={chatMessage.Attachments}");

            //hide Shell tab bar for this page
            Shell.SetTabBarIsVisible(this, false);

            //NavigationPage.TitleView in XAML not working, so the code below is for this purpose.
            Shell.SetTitleView(this, new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.Center,
                Children = {
                    new DphOverlapImage
                    {
                        HorizontalOptions = LayoutOptions.Start,
                        WidthRequest = HeightRequest = 40,
                        Margin = new Thickness(0, 5), //for iOS
                        Source1 = chatMessage.LogoUrl
                    },
                    new Label
                    {
                        HorizontalOptions = LayoutOptions.StartAndExpand,
                        Text = chatMessage.OwnerName,
                        TextColor = (Color)Application.Current.Resources["ColorTitle"],
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        VerticalTextAlignment = TextAlignment.Center
                    },
                }
            });

        }
    }
}