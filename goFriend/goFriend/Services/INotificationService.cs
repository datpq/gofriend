using goFriend.Models;

namespace goFriend.Services
{
    public interface INotificationService
    {
        void SendNotification(ServiceNotification serviceNotification);
        void CancelNotification(NotificationType? notificationType = null);
    }
}
