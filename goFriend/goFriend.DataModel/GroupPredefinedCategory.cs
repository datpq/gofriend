using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace goFriend.DataModel
{
    public class GroupPredefinedCategory
    {
        public int Id { get; set; }

        public int GroupId { get; set; }
        [JsonIgnore]
        public Group Group { get; set; }

        [Column(TypeName = "NVARCHAR(50)")]
        public string Category { get; set; }

        public int? ParentId { get; set; }
        [JsonIgnore]
        public GroupPredefinedCategory Parent { get; set; }
    }
}
