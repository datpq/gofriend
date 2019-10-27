using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace goFriend.DataModel
{
    public class CacheConfiguration
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "VARCHAR(250)")]
        public string KeyPrefix { get; set; }

        [Column(TypeName = "NVARCHAR(250)")]
        public string KeySuffixReg { get; set; }

        public int Timeout { get; set; }

        public bool Enabled { get; set; }
    }
}
