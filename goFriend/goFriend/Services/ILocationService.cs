using System;

namespace goFriend.Services
{
    public interface ILocationService
    {
        void Start();
        void Pause();
        void Stop();
        void RefreshStatus();
        bool IsRunning();
    }
}
