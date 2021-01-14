using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using goFriend.iOS.Renderers;
using goFriend.Views;

[assembly: ExportRenderer(typeof(ChatPage), typeof(RightToolbarMenuCustomRenderer))]
[assembly: ExportRenderer(typeof(ChatListPage), typeof(RightToolbarMenuCustomRenderer))]
//[assembly: ExportRenderer(typeof(ContentPage), typeof(RightToolbarMenuCustomRenderer))]

namespace goFriend.iOS.Renderers
{
    public class RightToolbarMenuCustomRenderer : PageRenderer
    {
        //I used UITableView for showing the menulist of secondary toolbar items.
        List<ToolbarItem> _secondaryItems;
        UITableView table;

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            //Get all secondary toolbar items and fill it to the gloabal list variable and remove from the content page.
            if (e.NewElement is ContentPage page)
            {
                _secondaryItems = page.ToolbarItems.Where(i => i.Order == ToolbarItemOrder.Secondary).ToList();
                _secondaryItems.ForEach(t => page.ToolbarItems.Remove(t));
            }
            base.OnElementChanged(e);
        }

        public override void ViewWillAppear(bool animated)
        {
            var element = (ContentPage)Element;
            //If global secondary toolbar items are not null, I created and added a primary toolbar item with image(Overflow) I         
            // want to show.
            if (_secondaryItems != null && _secondaryItems.Count > 0)
            {
                element.ToolbarItems.Add(new ToolbarItem()
                {
                    Order = ToolbarItemOrder.Primary,
                    IconImageSource = Constants.IconMnuMore,
                    Priority = 1,
                    Command = new Command(() =>
                    {
                        ToolClicked();
                    })
                });
            }
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            var element = (ContentPage)Element;
            if (_secondaryItems != null && _secondaryItems.Count > 0)
            {
                element.ToolbarItems.RemoveAt(element.ToolbarItems.Count - 1);
            }
            base.ViewWillAppear(animated);
        }

        //Create a table instance and added it to the view.
        private void ToolClicked()
        {
            if (table == null)
            {
                //Set the table position to right side. and set height to the content height.
                var childRect = new RectangleF((float)View.Bounds.Width - 250, 0, 250, _secondaryItems.Count() * 56 + 70);
                table = new UITableView(childRect)
                {
                    Source = new TableSource(_secondaryItems) // Created Table Source Class as Mentioned in the 
                                                              //Xamarin.iOS   Official site
                };
                Add(table);
                return;
            }
            foreach (var subview in View.Subviews)
            {
                if (subview == table)
                {
                    table.RemoveFromSuperview();
                    return;
                }
            }
            Add(table);
        }
    }

    public class TableSource : UITableViewSource
    {
        // Global variable for the secondary toolbar items and text to display in table row
        List<ToolbarItem> _tableItems;
        string[] _tableItemTexts;
        string CellIdentifier = "TableCell";

        public TableSource(List<ToolbarItem> items)
        {
            //Set the secondary toolbar items to global variables and get all text values from the toolbar items
            _tableItems = items;
            _tableItemTexts = items.Select(a => a.Text).ToArray();
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _tableItemTexts.Length;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell = tableView.DequeueReusableCell(CellIdentifier);
            string item = _tableItemTexts[indexPath.Row];
            if (cell == null)
            { cell = new UITableViewCell(UITableViewCellStyle.Default, CellIdentifier); }
            cell.TextLabel.Text = item;
            return cell;
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return 56; // Set default row height.
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            //Used command to excute and deselct the row and removed the table.
            var command = _tableItems[indexPath.Row].Command;
            command?.Execute(_tableItems[indexPath.Row].CommandParameter);
            tableView.DeselectRow(indexPath, true);
            tableView.RemoveFromSuperview();
        }
    }
}