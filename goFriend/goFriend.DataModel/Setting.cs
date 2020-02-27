using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace goFriend.DataModel
{
    public class Setting
    {
        public int Id { get; set; }

        [Required]
        public int Order { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR(100)")]
        public string Rule { get; set; }

        public bool LocationSwitch { get; set; }
        public bool DefaultShowLocation { get; set; }
    }
}
