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

        public bool Active { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

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
            CopyToIfNull(result);
            return result;
        }

        public void CopyToIfNull(Friend friend)
        {
            var isChanged = false;
            if (string.IsNullOrEmpty(friend.FacebookId))
            {
                isChanged = true;
                friend.FacebookId = FacebookId;
            }
            if (string.IsNullOrEmpty(friend.Name))
            {
                isChanged = true;
                friend.Name = Name;
            }
            if (string.IsNullOrEmpty(friend.FirstName))
            {
                isChanged = true;
                friend.FirstName = FirstName;
            }
            if (string.IsNullOrEmpty(friend.LastName))
            {
                isChanged = true;
                friend.LastName = LastName;
            }
            if (string.IsNullOrEmpty(friend.MiddleName))
            {
                isChanged = true;
                friend.MiddleName = MiddleName;
            }
            if (friend.Birthday == null)
            {
                isChanged = true;
                friend.Birthday = Birthday;
            }
            if (string.IsNullOrEmpty(friend.Gender))
            {
                isChanged = true;
                friend.Gender = Gender;
            }
            if (string.IsNullOrEmpty(friend.Email))
            {
                isChanged = true;
                friend.Email = Email;
            }
            if (friend.CreatedDate == null)
            {
                friend.CreatedDate = DateTime.Now;
            }
            if (friend.DeviceInfo != DeviceInfo)
            {
                isChanged = true;
                friend.DeviceInfo = DeviceInfo;
            }
            if (Image != null)
            {
                isChanged = true;
                friend.Image = Image;
            }
            if (isChanged)
            {
                friend.ModifiedDate = DateTime.Now;
            }
        }
    }
}
