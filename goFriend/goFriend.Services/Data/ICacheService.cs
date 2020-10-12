using System;

namespace goFriend.Services.Data
{
    public interface ICacheService
    {
        int GetCacheTimeout(IDataRepository dataRepository, string cachePrefix,
            string cacheSuffixPattern = null, bool isTableCache = false);
        void Set<TItem>(string key, TItem value, DateTimeOffset absoluteExpiration);
        void Remove(string key);
        object Get(string key);
    }
}
