using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace goFriend.DataModel
{
    public class Chat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(255)")]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR(255)")]
        public string Members { get; set; }

        [Column(TypeName = "VARCHAR(255)")]
        public string LogoUrl { get; set; }
    }
}
