using System;
using System.Threading;
using Xamarin.Forms;

namespace goFriend.Controls
{
    public class BehaviorTextChangedEntry : Behavior<Entry>
    {
        private CancellationTokenSource _cancelToken = new CancellationTokenSource();

        protected override void OnAttachedTo(Entry bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.TextChanged += Bindable_TextChanged;
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.TextChanged -= Bindable_TextChanged;
        }

        private void Bindable_TextChanged(object sender, TextChangedEventArgs e)
        {
            StopSearchCommand();
            var cts = _cancelToken; // safe copy
            Device.StartTimer(TimeSpan.FromMilliseconds(Constants.SearchCommandDelayTime),
                () => {
                    //Logger.Debug($"timer running: {e.NewTextValue}");
                    if (cts.IsCancellationRequested) return false;
                    ((Entry)sender).ReturnCommand?.Execute(null);
                    return false; // or true for periodic behavior
                });
        }

        private void StopSearchCommand()
        {
            Interlocked.Exchange(ref _cancelToken, new CancellationTokenSource()).Cancel();
        }
    }
}
