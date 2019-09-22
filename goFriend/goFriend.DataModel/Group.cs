using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace goFriend.DataModel
{
    public class Group
    {
        public int Id { get; set; }

        [Column(TypeName = "NVARCHAR(50)")]
        public string Name { get; set; }

        [Column(TypeName = "NVARCHAR(255)")]
        public string Desc { get; set; }

        [Column(TypeName = "NVARCHAR(2000)")]
        public string Info { get; set; }

        [JsonIgnore]
        public ICollection<GroupFriend> GroupFriends { get; set; }

        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat1Desc { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat2Desc { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat3Desc { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat4Desc { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat5Desc { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat6Desc { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat7Desc { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat8Desc { get; set; }
        [Column(TypeName = "NVARCHAR(50)")]
        public string Cat9Desc { get; set; }

        public bool Active { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public byte[] Logo { get; set; }

        public override string ToString()
        {
            return $"{Id}|{Name}";
        }
    }
}
