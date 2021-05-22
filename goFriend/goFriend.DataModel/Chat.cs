using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace goFriend.DataModel
{
    public class Chat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(255)")]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR(255)")]
        public string Members { get; set; }

        [Column(TypeName = "VARCHAR(255)")]
        public string LogoUrl { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? OwnerId { get; set; }
        //[ForeignKey("OwnerId")]
        [JsonIgnore]
        public Friend Owner { get; set; }
        [NotMapped]
        public string Token { get; set; }

        //[JsonIgnore]
        //public ICollection<ChatMessage> ChatMessages { get; set; }
    }
}
