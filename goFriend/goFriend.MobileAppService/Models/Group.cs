using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace goFriend.MobileAppService.Models
{
    public class Group
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Column(TypeName = "VARCHAR(50)")]
        public string Name { get; set; }

        [Column(TypeName = "VARCHAR(255)")]
        public string Desc { get; set; }

        public ICollection<GroupFriend> GroupFriends { get; set; }
    }
}
