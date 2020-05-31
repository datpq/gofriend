using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace goFriend.DataModel
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        public int ChatId { get; set; }
        //[ForeignKey("ChatId")]
        [JsonIgnore]
        public Chat Chat { get; set; }

        public int MessageIndex { get; set; }

        public ChatMessageType MessageType { get; set; }
        public int OwnerId { get; set; }
        //[ForeignKey("OwnerId")]
        [JsonIgnore]
        public Friend Owner { get; set; }
        [NotMapped]
        public string Token { get; set; }

        [Column(TypeName = "NVARCHAR(1000)")]
        public string Message { get; set; }

        [NotMapped]
        [JsonIgnore]
        public string MessageLinkify => Message?.Linkify();

        [JsonIgnore]
        [NotMapped]
        public bool IsThumbsUp => string.IsNullOrEmpty(Message);

        [NotMapped]
        public string OwnerName { get; set; }
        [NotMapped]
        public string OwnerFirstName { get; set; }

        [NotMapped]
        public string LogoUrl { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        [Column(TypeName = "NVARCHAR(200)")]
        public string Reads { get; set; } // List of users who have read the message

        public bool IsDeleted { get; set; }

        [Column(TypeName = "NVARCHAR(200)")]
        public string Attachments { get; set; }

        [JsonIgnore]
        [NotMapped]
        public bool IsSystemMessage => OwnerId == 0;
        [JsonIgnore]
        [NotMapped]
        public bool IsOwnMessage { get; set; } // used in client side

        public bool IsRead(int friendId)
        {
            return $"{Extension.Sep}{Reads}{Extension.Sep}".IndexOf($"{Extension.Sep}u{friendId}{Extension.Sep}", StringComparison.Ordinal) >= 0;
        }

        public bool DoRead(int friendId)
        {
            if (IsRead(friendId)) return false;
            Reads = string.IsNullOrEmpty(Reads) ? $"u{friendId}" : $"{Reads},u{friendId}";
            return true;
        }
    }
}
