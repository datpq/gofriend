using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace goFriend.DataModel
{
    public class Configuration
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "VARCHAR(50)")]
        public string Key { get; set; }

        [Column(TypeName = "VARCHAR(200)")]
        public string Value { get; set; }

        public bool Enabled { get; set; }

        [JsonIgnore]
        [Column(TypeName = "VARCHAR(50)")]
        public string Comment { get; set; }
    }
}
