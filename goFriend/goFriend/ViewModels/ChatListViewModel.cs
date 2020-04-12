using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using goFriend.Services;
using PCLAppConfig;
using Xamarin.Forms;

namespace goFriend.ViewModels
{
    public class ChatListViewModel : INotifyPropertyChanged
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        private ObservableCollection<ChatListItemViewModel> _items = new ObservableCollection<ChatListItemViewModel>();
        public ObservableCollection<ChatListItemViewModel> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        public ICommand RefreshCommand
        {
            get
            {
                return new Command(async () =>
                {
                    Logger.Debug("RefreshCommand.BEGIN");
                    Items.Clear();
                    await FetchItems();
                    Logger.Debug("RefreshCommand.END");
                });
            }
        }

        private async Task FetchItems()
        {
            IsRefreshing = true;
            var chats = await App.FriendStore.ChatGetChats();
            foreach (var chat in chats)
            {
                if (chat.LogoUrl == null)
                {
                    chat.LogoUrl = "/logos/group.png";
                }
                chat.LogoUrl = $"{ConfigurationManager.AppSettings["HomePageUrl"]}{chat.LogoUrl}";
                _items.Add(new ChatListItemViewModel {Chat = chat});
            }
            IsRefreshing = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
