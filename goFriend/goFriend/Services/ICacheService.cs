using System;

namespace goFriend.Services
{
    public interface ICacheService
    {
        void Set<TItem>(string key, TItem value, DateTimeOffset absoluteExpiration);
        void Remove(string key);
        object Get(string key);
    }
}
