using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace goFriend.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ConcurrentDictionary<string, DateTimeOffset> _cacheKeys = new ConcurrentDictionary<string, DateTimeOffset>();
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        public CacheService()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
        }

        public object Get(string key)
        {
            return _memoryCache.Get(key);
        }

        public void Set<TItem>(string key, TItem value, DateTimeOffset absoluteExpiration)
        {
            _cacheKeys[key] = absoluteExpiration;
            _memoryCache.Set(key, value, absoluteExpiration);
            Logger.Debug($"key={key}, absoluteExpiration={absoluteExpiration}, count={_cacheKeys.Count}");
        }

        public void Remove(string key)
        {
            Logger.Debug($"BEGIN(key={key}, count={_cacheKeys.Keys.Count})");
            //foreach (var cacheKey in _cacheKeys.Keys.Where(x => x.Contains(key, StringComparison.CurrentCultureIgnoreCase)).ToList())
            foreach (var cacheKey in _cacheKeys.Keys.Where(x => x.Contains(key)).ToList())
            {
                Logger.Debug($"Remove cacheKey={cacheKey}, absoluteExpiration={_cacheKeys[cacheKey]}");
                _cacheKeys.TryRemove(cacheKey, out DateTimeOffset outValue);
                _memoryCache.Remove(cacheKey);
            }
            Logger.Debug($"END(count={_cacheKeys.Keys.Count})");
        }
    }
}
