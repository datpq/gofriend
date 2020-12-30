using System;
using System.Threading.Tasks;

namespace goFriend.Services
{
    public interface ILocationService
    {
        void Start();
        void Pause();
        void Stop();
        void RefreshStatus();
        bool IsRunning();
        event EventHandler StateChanged;
    }
}
