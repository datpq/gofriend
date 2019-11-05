using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;

namespace goFriend.DataModel
{
    public class GroupFixedCatValues
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        [JsonIgnore]
        public Group Group { get; set; }

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

        public IEnumerable<string> GetCatList()
        {
            var result = new[]
            {
                Cat1, Cat2, Cat3, Cat4, Cat5, Cat6, Cat7, Cat8, Cat9
            }.Where(x => !string.IsNullOrEmpty(x));
            return result;
        }
    }
}
