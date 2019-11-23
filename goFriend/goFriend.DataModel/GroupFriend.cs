using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace goFriend.DataModel
{
    public class GroupFriend
    {
        [Key]
        public int Id { get; set; }

        public int FriendId { get; set; }
        //[ForeignKey("FriendId")]
        //[JsonIgnore]
        public Friend Friend { get; set; }

        public int GroupId { get; set; }
        //[ForeignKey("GroupId")]
        //[JsonIgnore]
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

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public IEnumerable<string> GetCatList()
        {
            var result = new[]
            {
                Cat1, Cat2, Cat3, Cat4, Cat5, Cat6, Cat7, Cat8, Cat9
            }.Where(x => !string.IsNullOrEmpty(x));
            return result;
        }

        public string GetCatValueDisplay(int startIdx)
        {
            var arrCats = Group.GetCatDescList().ToList();
            var arrCatValueDisplay = new string[arrCats.Count - startIdx];
            for (var i = startIdx; i < arrCats.Count; i++)
            {
                arrCatValueDisplay[i - startIdx] = $"{arrCats[i]} {GetCatByIdx(i + 1)}";
            }
            return string.Join(", ", arrCatValueDisplay);
        }

        public string GetCatByIdx(int idx)
        {
            var propertyInfo = GetType().GetProperty($"Cat{idx}");
            var result = (string)propertyInfo?.GetValue(this, null);
            return result;
        }

        public void SetCatByIdx(int idx, string value)
        {
            var propertyInfo = GetType().GetProperty($"Cat{idx}");
            propertyInfo?.SetValue(this, value);
        }

        public override string ToString()
        {
            var arrCatValues = new string[GetCatList().Count()];
            for (var i=0; i<arrCatValues.Length; i++)
            {
                arrCatValues[i] = $"Cat{i+1}={GetCatByIdx(i+1)}";
            }
            var result = $"FriendId={FriendId}, GroupId={GroupId} {string.Join(",", arrCatValues)}";
            return result;
        }
    }

    public enum UserType
    {
        NotMember = 0,
        Pending = 1,
        Normal = 2,
        Admin = 3
    }
}
