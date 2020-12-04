using goFriend.Models;

namespace goFriend.Services
{
    public interface INotificationService
    {
        void StartService();
        void PauseService();
        void StopService();
        void SendNotification(ServiceNotification serviceNotification);
        void CancelNotification(NotificationType? notificationType = null);
    }
}
