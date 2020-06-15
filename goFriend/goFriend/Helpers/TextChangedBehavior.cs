using System;
using System.Threading;
using Xamarin.Forms;

namespace goFriend.Helpers
{
    public class TextChangedBehavior : Behavior<SearchBar>
    {
        private CancellationTokenSource _cancelToken = new CancellationTokenSource();
        //private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        protected override void OnAttachedTo(SearchBar bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.TextChanged += Bindable_TextChanged;
        }

        protected override void OnDetachingFrom(SearchBar bindable)
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
                    ((SearchBar)sender).SearchCommand?.Execute(e.NewTextValue);
                    return false; // or true for periodic behavior
                });
        }

        private void StopSearchCommand()
        {
            Interlocked.Exchange(ref _cancelToken, new CancellationTokenSource()).Cancel();
        }
    }
}
