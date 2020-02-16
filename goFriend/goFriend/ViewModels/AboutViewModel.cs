using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace goFriend.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = AppInfo.Name;

            OpenWebCommand = new Command(() => Launcher.OpenAsync("https://xamarin.com/platform"));
        }

        public ICommand OpenWebCommand { get; }
    }
}