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

        public string GetContentBody()
        {
            switch (NotificationType)
            {
                case NotificationType.AppearOnMap:
                case NotificationType.ChatReceiveMessage:
                    var contentBody = string.Empty;
                    foreach (var inboxLine in InboxLines)
                    {
                        contentBody = $"{contentBody}\n{inboxLine[0]}: {inboxLine[1]}";
                    }
                    if (InboxLines.Count > 0)
                    {
                        contentBody = contentBody.Substring(1);
                    }
                    return contentBody;
                case NotificationType.ChatReceiveCreateChat:
                default:
                    return ContentText;
            } 
        }
    }

    public enum NotificationType
    {
        ChatReceiveMessage = 1,
        ChatReceiveCreateChat,
        AppearOnMap
    }
}
