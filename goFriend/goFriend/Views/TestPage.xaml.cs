using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage : ContentPage
    {
        public TestPage()
        {
            BindingContext = new TestModel();
            InitializeComponent();
        }
    }

    public class TestModel
    {
        private ObservableCollection<TestItemModel> _messages = new ObservableCollection<TestItemModel>
        {
            new TestItemModel {Message = "Bảo Anh Bảo Linh Bảo Châu tham gia hội thoại", IsSystemMessage = true},
            new TestItemModel {Message = "Thắng Phạm thoát khỏi hội thoại", IsSystemMessage = false},
            new TestItemModel { Message = "Bảo Thoa tham gia hội thoại", IsSystemMessage = true}
        };
        public ObservableCollection<TestItemModel> Messages
        {
            get => _messages;
            set
            {
                _messages = value;
                OnPropertyChanged(nameof(Messages));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TestItemModel
    {
        public bool IsSystemMessage { get; set; }
        public string Message { get; set; }
    }
}