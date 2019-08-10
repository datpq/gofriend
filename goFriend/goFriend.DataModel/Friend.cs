using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace goFriend.DataModel
{
    public class Friend
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(100)")]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(50)")]
        public string FirstName { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(50)")]
        public string LastName { get; set; }

        [Column(TypeName = "NVARCHAR(50)")]
        public string MiddleName { get; set; }

        [Column(TypeName = "VARCHAR(20)")]
        public string FacebookId { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR(50)")]
        public string Email { get; set; }

        public DateTime? Birthday { get; set; }

        [Column(TypeName = "VARCHAR(10)")]
        public string Gender { get; set; }

        [JsonIgnore]
        public ICollection<GroupFriend> GroupFriends { get; set; }

        public override string ToString()
        {
            return $"{FacebookId}|{Id}|{Name}|{Email}";
        }

        [JsonIgnore]
        [NotMapped]
        public string Avatar =>
            string.IsNullOrEmpty(FacebookId) ? (Gender == "male" ? "default_male.jpg" : "default_female.jpg")
                : $"https://graph.facebook.com/{FacebookId}/picture?type=normal";
    }
}
