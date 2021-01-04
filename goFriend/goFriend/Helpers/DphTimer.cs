using System;
using System.Threading;
using Xamarin.Forms;

namespace goFriend.Helpers
{
    public class DphTimer
    {
        public DateTime StartingTime { get; set; }
        private readonly Action callback;

        private CancellationTokenSource cancellation;

        public DphTimer(Action callback)
        {
            this.callback = callback;
            this.cancellation = new CancellationTokenSource();
        }

        public void Start()
        {
            if (StartingTime < DateTime.Now) return;
            CancellationTokenSource cts = this.cancellation; // safe copy
            Device.StartTimer(StartingTime - DateTime.Now,
                () => {
                    if (cts.IsCancellationRequested) return false;
                    this.callback.Invoke();
                    StartingTime = DateTime.MinValue;
                    return false; // or true for periodic behavior
            });
        }

        public void Stop()
        {
            Interlocked.Exchange(ref this.cancellation, new CancellationTokenSource()).Cancel();
        }
    }
}
