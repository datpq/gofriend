using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace goFriend.ViewModels
{
    public class DphListViewModel
    {
        public ObservableCollection<DphListViewItemModel> Items { get; } = new ObservableCollection<DphListViewItemModel>();
    }

    public class DphListViewItemModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
    }
}
