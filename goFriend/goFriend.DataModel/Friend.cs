using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace goFriend.DataModel
{
    public class Friend : ICloneable
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

        [Column(TypeName = "VARCHAR(170)")]
        public string DeviceInfo { get; set; }

        public byte[] Image { get; set; }

        [JsonIgnore]
        public ICollection<GroupFriend> GroupFriends { get; set; }

        public override string ToString()
        {
            return $"{FacebookId}|{Id}|{Name}|{Email}";
        }

        public object Clone()
        {
            var result = new Friend();
            CopyTo(result);
            return result;
        }

        public void CopyTo(Friend friend)
        {
            friend.FacebookId = FacebookId;
            friend.Name = Name;
            friend.FirstName = FirstName;
            friend.LastName = LastName;
            friend.MiddleName = MiddleName;
            friend.Birthday = Birthday;
            friend.Gender = Gender;
            friend.Email = Email;
            friend.DeviceInfo = DeviceInfo;
            friend.Image = Image;
        }
    }
}
