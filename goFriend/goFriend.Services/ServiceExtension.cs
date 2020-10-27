using goFriend.DataModel;

namespace goFriend.Services
{
    public static class ServiceExtension
    {
        public static string GetChatMemberNames(this Data.IDataRepository repo, Chat chat)
        {
            var arrIds = chat.GetMemberIds();
            var arrNames = new string[arrIds.Length];
            for (var i = 0; i < arrNames.Length; i++)
            {
                var localI = i;
                var friend = repo.Get<Friend>(x => x.Id == arrIds[localI]);
                arrNames[i] = friend.FirstName;
            }
            return string.Join(", ", arrNames);
        }
    }
}
