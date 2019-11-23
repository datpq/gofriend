using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace goFriend.DataModel
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public NotificationType Type { get; set; }

        private INotification _notificationObj;
        [NotMapped]
        public INotification NotificationObject { get => _notificationObj;
            set
            {
                _notificationObj = value;
                Type = _notificationObj.Type;
            }
        }

        [Column(TypeName = "NVARCHAR(1000)")]
        public string NotificationJson
        {
            get => JsonConvert.SerializeObject(NotificationObject);
            set
            {
                switch (Type)
                {
                    case NotificationType.NewSubscriptionRequest:
                        NotificationObject = JsonConvert.DeserializeObject<NotifNewSubscriptionRequest>(value);
                        break;
                    case NotificationType.SubscriptionApproved:
                        NotificationObject = JsonConvert.DeserializeObject<NotifSubscriptionApproved>(value);
                        break;
                    case NotificationType.SubscriptionRejected:
                        NotificationObject = JsonConvert.DeserializeObject<NotifSubscriptionRejected>(value);
                        break;
                    case NotificationType.UpdateSubscriptionRequest:
                        NotificationObject = JsonConvert.DeserializeObject<NotifUpdateSubscriptionRequest>(value);
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
        }

        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "NVARCHAR(200)")]
        public string Destination { get; set; } // List of users who are notified. u1,u2,g1 ==> user 1, user 2, group 1,

        [Column(TypeName = "NVARCHAR(200)")]
        public string Reads { get; set; } // List of users who have read the notification

        [Column(TypeName = "NVARCHAR(200)")]
        public string Deletions { get; set; } // List of users who have deleted the notification

        public int OwnerId { get; set; }
        //[ForeignKey("OwnerId")]
        //public Friend Owner { get; set; }
    }

    public interface INotification
    {
        NotificationType Type { get; }
        string GetMessage(int friendId);
    }

    public enum NotificationType
    {
        NewSubscriptionRequest = 0, // sent to all admins of the group. u1g2 ==> user 1 want to join group 2
        SubscriptionApproved, // sent to the owner of the request. u1g2 ==> user 1's subscription to the group 2 request has been approved.
        SubscriptionRejected, // sent to the owner of the request. u1g2 ==> user 1's subscription to the group 2 request has been rejected.
        UpdateSubscriptionRequest // sent to all admins of the group. u1g2 ==> user 1 modified connection info when joining the group 2
    }

    public abstract class GroupSubscriptionNotifBase : INotification
    {
        private Friend _friend;
        private Group _group;

        public int FriendId { get; set; }
        [ForeignKey("FriendId")]
        [JsonIgnore]
        //public Friend Friend { get; set; }
        public Friend Friend
        {
            get => _friend;
            set
            {
                _friend = value;
                FriendId = _friend.Id;
            }
        }
        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        [JsonIgnore]
        //public Group Group { get; set; }
        public Group Group
        {
            get => _group;
            set
            {
                _group = value;
                GroupId = _group.Id;
            }
        }

        //[Key]
        //protected int Id { get; set; }
        public abstract NotificationType Type { get; }
        public abstract string GetMessage(int friendId);
    }

    public class NotifNewSubscriptionRequest : GroupSubscriptionNotifBase
    {
        public override NotificationType Type => NotificationType.NewSubscriptionRequest;
        public override string GetMessage(int friendId)
        {
            return $"{Friend.Name} wanted to join the group {Group.Name}";
        }
    }

    public class NotifUpdateSubscriptionRequest : GroupSubscriptionNotifBase
    {
        public override NotificationType Type => NotificationType.UpdateSubscriptionRequest;
        public override string GetMessage(int friendId)
        {
            return $"{Friend.Name} updated connection information when joining the group {Group.Name}";
        }
    }

    public class NotifSubscriptionApproved : GroupSubscriptionNotifBase
    {
        public override NotificationType Type => NotificationType.SubscriptionApproved;
        public override string GetMessage(int friendId)
        {
            return friendId == FriendId ? $"You are now a member of {Group.Name}"
                : $"{Friend.Name} is now a member of {Group.Name}";
        }
    }

    public class NotifSubscriptionRejected : GroupSubscriptionNotifBase
    {
        public override NotificationType Type => NotificationType.SubscriptionRejected;
        public override string GetMessage(int friendId)
        {
            return friendId == FriendId ? $"Your subscription to the group {Group.Name} has been rejected."
                : $"{Friend.Name}'s subscription to the group {Group.Name} has been rejected.";
        }
    }
}
