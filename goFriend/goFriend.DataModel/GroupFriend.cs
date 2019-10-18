using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace goFriend.DataModel
{
    public class GroupFriend
    {
        public int FriendId { get; set; }
        //[ForeignKey("FriendId")]
        [JsonIgnore]
        public Friend Friend { get; set; }

        public int GroupId { get; set; }
        //[ForeignKey("GroupId")]
        [JsonIgnore]
        public Group Group { get; set; }

        public bool Active { get; set; }
        public UserType UserRight { get; set; }

        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat1 { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat2 { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat3 { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat4 { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat5 { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat6 { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat7 { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat8 { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat9 { get; set; }

        public string GetCatByIdx(int idx)
        {
            var propertyInfo = GetType().GetProperty($"Cat{idx}");
            var result = (string)propertyInfo?.GetValue(this, null);
            return result;
        }
    }

    public enum UserType
    {
        None = 0,
        Admin = 1,
        Basic
    }
}
