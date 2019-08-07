using System.ComponentModel.DataAnnotations.Schema;

namespace goFriend.MobileAppService.Models
{
    public class GroupFriend
    {
        [ForeignKey("Friend")]
        public int FriendId { get; set; }
        public Friend Friend { get; set; }

        [ForeignKey("Group")]
        public int GroupId { get; set; }
        public Group Group { get; set; }
    }
}
