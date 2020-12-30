using goFriend.Droid;
using goFriend.Services;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(LocationService))]
namespace goFriend.Droid
{
    public class LocationService : ILocationService
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        public static MainActivity MainActivity;
        public static AsyncAutoResetEvent StandByWhenStart = new AsyncAutoResetEvent();
        public static bool IsTracing = false;

        public LocationService()
        {
            Logger.Debug($"LocationService creating instance...");
        }

        public event EventHandler StateChanged;

        private async Task Start(bool startOrPause)
        {
            Logger.Debug($"Start.BEGIN(startOrPause={startOrPause})");
            if (!MainActivity.IsServiceStarted)
            {
                Logger.Debug("Service's not started. Starting now...");
                MainActivity.StartService(MainActivity.StartServiceIntent);
                MainActivity.IsServiceStarted = true;
                StandByWhenStart = new AsyncAutoResetEvent();
                await StandByWhenStart.WaitAsync().ConfigureAwait(true);
            }
            if (LocationForegroundService.GetInstance() != null)
            {
                IsTracing = startOrPause;
                await LocationForegroundService.GetInstance().StartForegroundService();
                StateChanged?.Invoke(this, null);
            }
            else
            {
                Logger.Error("Service started but LocationForegroundService is null");
            }
            Logger.Debug("Start.END");
        }

        public async void Start()
        {
            await Start(true);
        }

        public async void Pause()
        {
            Logger.Debug($"Pause.BEGIN");

            IsTracing = !IsTracing;
            Logger.Debug("Tracing is " + (IsTracing ? "on" : "off"));

            await Start(IsTracing); // update just the interface (Activate and Stop button)

            Logger.Debug($"Pause.END");
        }

        public async void Stop()
        {
            Logger.Debug("Stop.BEGIN");
            if (LocationForegroundService.GetInstance() != null)
            {
                await LocationForegroundService.GetInstance().StopForegroundService();
                MainActivity.IsServiceStarted = false;
                IsTracing = false;
                StateChanged?.Invoke(this, null);
            }
            else
            {
                Logger.Error("Stopping but LocationForegroundService is null");
            }
            Logger.Debug("Stop.END");
        }

        public async void RefreshStatus()
        {
            Logger.Debug($"UpdateStatus.BEGIN");
            if (MainActivity.IsServiceStarted && LocationForegroundService.GetInstance() != null)
            {
                await LocationForegroundService.GetInstance().StartForegroundService();
            }
            else
            {
                Logger.Error("Service not started or LocationForegroundService is null");
            }
            Logger.Debug("UpdateStatus.END");
        }

        public bool IsRunning()
        {
            //return MainActivity.IsServiceStarted;
            return MainActivity.IsServiceStarted && IsTracing;
        }
    }
}