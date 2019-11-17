using System.ComponentModel;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using goFriend.Controls;
using goFriend.Droid.Renderers;

[assembly: ExportRenderer(typeof(DphViewCell), typeof(DphViewCellRenderer))]
namespace goFriend.Droid.Renderers
{
    public class DphViewCellRenderer : ViewCellRenderer
    {
        private Android.Views.View _cellCore;
        private Drawable _unselectedBackground;
        private bool _selected;

        protected override Android.Views.View GetCellCore(Cell item, Android.Views.View convertView, ViewGroup parent, Context context)
        {
            _cellCore = base.GetCellCore(item, convertView, parent, context);
            _selected = false;
            _unselectedBackground = _cellCore.Background;
            return _cellCore;
        }
        protected override void OnCellPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnCellPropertyChanged(sender, args);
            if (args.PropertyName == "IsSelected")
            {
                _selected = !_selected;
                if (_selected)
                {
                    var extendedViewCell = sender as DphViewCell;
                    _cellCore.SetBackgroundColor(extendedViewCell.SelectedItemBackgroundColor.ToAndroid());
                }
                else
                {
                    _cellCore.SetBackground(_unselectedBackground);
                }
            }
        }
    }
}