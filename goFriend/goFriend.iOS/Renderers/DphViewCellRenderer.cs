using goFriend.Controls;
using goFriend.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(DphViewCell), typeof(DphViewCellRenderer))]
namespace goFriend.iOS.Renderers
{
    public class DphViewCellRenderer : ViewCellRenderer
    {
        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        {
            var cell = base.GetCell(item, reusableCell, tv);
            var view = item as DphViewCell;
            cell.SelectedBackgroundView = new UIView
            {
                BackgroundColor = view?.SelectedItemBackgroundColor.ToUIColor(),
            };
            return cell;
        }
    }
}