using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace goFriend.DataModel
{
    public class Notification
    {
        public const string NotifIdSep = ",";

        [Key]
        public int Id { get; set; }

        public NotificationType Type { get; set; }

        private object _notificationObject;
        [JsonIgnore]
        [NotMapped]
        public object NotificationObject
        {
            get => _notificationObject;
            set
            {
                _notificationObject = value;
                if (_notificationObject is NotifNewSubscriptionRequest)
                {
                    Type = NotificationType.NewSubscriptionRequest;
                }
                else if (_notificationObject is NotifUpdateSubscriptionRequest)
                {
                    Type = NotificationType.UpdateSubscriptionRequest;
                }
                else if (_notificationObject is NotifSubscriptionApproved)
                {
                    Type = NotificationType.SubscriptionApproved;
                }
                else if (_notificationObject is NotifSubscriptionRejected)
                {
                    Type = NotificationType.SubscriptionRejected;
                }
                else
                {
                    throw new ArgumentException();
                }
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

        public bool IsRead(int friendId)
        {
            return $"{NotifIdSep}{Reads}{NotifIdSep}".IndexOf($"{NotifIdSep}u{friendId}{NotifIdSep}", StringComparison.Ordinal) >= 0;
        }

        public bool DoRead(int friendId)
        {
            if (IsRead(friendId)) return false;
            Reads = string.IsNullOrEmpty(Reads) ? $"u{friendId}" : $"{Reads},u{friendId}";
            return true;
        }
    }

    public enum NotificationType
    {
        NewSubscriptionRequest = 0, // sent to all admins of the group. u1g2 ==> user 1 want to join group 2
        UpdateSubscriptionRequest, // sent to all admins of the group. u1g2 ==> user 1 modified connection info when joining the group 2
        SubscriptionApproved, // sent to the owner of the request. u1g2 ==> user 1's subscription to the group 2 request has been approved.
        SubscriptionRejected // sent to the owner of the request. u1g2 ==> user 1's subscription to the group 2 request has been rejected.
    }

    public abstract class GroupSubscriptionNotifBase
    {
        private Friend _friend;
        private Group _group;

        public int FriendId { get; set; }
        public string FriendName { get; set; }
        public string FacebookId { get; set; }

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
                FriendName = _friend.Name;
                FacebookId = _friend.FacebookId;
            }
        }

        public int GroupId { get; set; }
        public string GroupName { get; set; }

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
                GroupName = _group.Name;
            }
        }

        //[Key]
        //protected int Id { get; set; }
        public abstract NotificationType Type { get; }
        public virtual string ImageFile => $"notif_{Type}.png";
    }

    public class NotifNewSubscriptionRequest : GroupSubscriptionNotifBase
    {
        public override NotificationType Type => NotificationType.NewSubscriptionRequest;
    }

    public class NotifUpdateSubscriptionRequest : GroupSubscriptionNotifBase
    {
        public override NotificationType Type => NotificationType.UpdateSubscriptionRequest;
    }

    public class NotifSubscriptionApproved : GroupSubscriptionNotifBase
    {
        public override NotificationType Type => NotificationType.SubscriptionApproved;
    }

    public class NotifSubscriptionRejected : GroupSubscriptionNotifBase
    {
        public override NotificationType Type => NotificationType.SubscriptionRejected;
    }
}
