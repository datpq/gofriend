using System.Threading.Tasks;
using goFriend.DataModel;

namespace goFriend.Services
{
    public interface IFriendStore
    {
        Task<bool> AddOrUpdateFriendAsync(Friend friend);
    }
}
