using System.Threading.Tasks;
using goFriend.DataModel;

namespace goFriend.Services
{
    public interface IFriendStore
    {
        Task<Friend> LoginWithFacebook(Friend friend);
        Task<bool> SaveBasicInfo(Friend friend);
    }
}
