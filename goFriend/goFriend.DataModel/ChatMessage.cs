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

        public ChatMessageType MessageType { get; set; }

        [Column(TypeName = "NVARCHAR(1000)")]
        public string Message { get; set; }

        public int OwnerId { get; set; }

        [Column(TypeName = "NVARCHAR(100)")]
        public string OwnerName { get; set; }

        [Column(TypeName = "NVARCHAR(200)")]
        public string Reads { get; set; } // List of users who have read the message

        [JsonIgnore]
        public bool IsSystemMessage => OwnerId == 0;
        [JsonIgnore]
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

    public enum ChatMessageType
    {
        Ping = 0,
        SendMessage
    }
}
