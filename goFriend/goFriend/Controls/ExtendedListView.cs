using System;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace goFriend.Controls
{
    public class ExtendedListView : ListView
    {
        public ExtendedListView() : this(ListViewCachingStrategy.RecycleElement)
        {
        }

        public ExtendedListView(ListViewCachingStrategy cachingStrategy) : base(cachingStrategy)
        {
            ItemSelected += OnItemSelected;
            ItemTapped += OnItemTapped;
            ItemAppearing += OnItemAppearing;
            ItemDisappearing += OnItemDisappering;
        }

        public static readonly BindableProperty TappedCommandProperty =
            BindableProperty.Create(nameof(TappedCommand), typeof(ICommand), typeof(ExtendedListView), default(ICommand));

        public ICommand TappedCommand
        {
            get { return (ICommand)GetValue(TappedCommandProperty); }
            set { SetValue(TappedCommandProperty, value); }
        }

        public static readonly BindableProperty ItemAppearingCommandProperty =
            BindableProperty.Create(nameof(ItemAppearingCommand), typeof(ICommand), typeof(ExtendedListView), default(ICommand));

        public ICommand ItemAppearingCommand
        {
            get { return (ICommand)GetValue(ItemAppearingCommandProperty); }
            set { SetValue(ItemAppearingCommandProperty, value); }
        }

        public static readonly BindableProperty ItemDisappearingCommandProperty =
         BindableProperty.Create(nameof(ItemDisappearingCommand), typeof(ICommand), typeof(ExtendedListView), default(ICommand));


        public ICommand ItemDisappearingCommand
        {
            get { return (ICommand)GetValue(ItemDisappearingCommandProperty); }
            set { SetValue(ItemDisappearingCommandProperty, value); }
        }


        private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var listView = (ExtendedListView)sender;
            if (e == null) return;
            listView.SelectedItem = null;

        }

        private void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (TappedCommand != null)
            {
                TappedCommand?.Execute(e.Item);
            }
            SelectedItem = null;
        }

        private void OnItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            if (ItemAppearingCommand != null)
            {
                ItemAppearingCommand?.Execute(e.Item);
            }
        }


        private void OnItemDisappering(object sender, ItemVisibilityEventArgs e)
        {
            ItemDisappearingCommand?.Execute(e.Item);
        }


        public void ScrollToFirst(object msgObj = null)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    if (msgObj == null)
                    {
                        if (ItemsSource != null && ItemsSource.Cast<object>().Any())
                        {
                            msgObj = ItemsSource.Cast<object>().FirstOrDefault();
                        }
                    }
                    if (msgObj != null)
                    {
                        ScrollTo(msgObj, ScrollToPosition.Start, true);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            });
        }

        public void ScrollToLast(object msgObj = null)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    if (msgObj == null)
                    {
                        if (ItemsSource != null && ItemsSource.Cast<object>().Any())
                        {
                            msgObj = ItemsSource.Cast<object>().LastOrDefault();
                        }
                    }
                    if (msgObj != null)
                    {
                        ScrollTo(msgObj, ScrollToPosition.End, true);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            });
        }
    }
}
