using System.Collections.Generic;

namespace goFriend.Models
{
    public class ServiceNotification
    {
        public string ContentTitle { get; set; }
        public string ContentText { get; set; }
        public string SummaryText { get; set; }
        public string LargeIconUrl { get; set; }
        public NotificationType NotificationType { get; set; }
        public List<string[]>InboxLines  {get; set;}
        public int ExtraId { get; set; }
    }

    public enum NotificationType
    {
        ChatReceiveMessage = 1,
        ChatReceiveCreateChat,
        AppearOnMap
    }
}
