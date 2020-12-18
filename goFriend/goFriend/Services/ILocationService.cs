using goFriend.Models;
using System;
using System.Threading.Tasks;

namespace goFriend.Services
{
    public interface ILocationService
    {
        Task Start();
        Task Pause();
        Task Stop();
        bool IsRunning();
        void SendNotification(ServiceNotification serviceNotification);
        void CancelNotification(NotificationType? notificationType = null);
        event EventHandler StateChanged;
    }
}
