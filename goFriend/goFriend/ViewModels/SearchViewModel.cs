using FFImageLoading.Work;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace goFriend.ViewModels
{
    public class SearchViewModel : BaseViewModel
    {
        //private string _text = string.Empty;
        //public string Text
        //{
        //    get => _text;
        //    set => SetProperty(ref _text, value);
        //}
        public string Text { get; set; }
        public ICommand SearchCommand { get; set; }
        public ObservableCollection<SearchItemModel> Items { get; } = new ObservableCollection<SearchItemModel>();
        public bool AcceptNotFoundValue { get; set; }
    }

    public class SearchItemModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
        public int ItemType { get; set; }
        public string ImageSource { get; set; }
        public Color ImageForeground { get; set; }
        public int SubItemCount { get; set; }

        public List<ITransformation> Transformations {get; set;}

        int _imageSize = 0;
        public int ImageSize
        {
            get => _imageSize != 0 ? _imageSize : 26;
            set => _imageSize = value;
        }
    }
}
