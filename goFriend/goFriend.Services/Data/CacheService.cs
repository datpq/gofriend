using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using goFriend.DataModel;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NLog;

namespace goFriend.Services.Data
{
    public class CacheService : ICacheService
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IMemoryCache _memoryCache;
        private readonly ConcurrentDictionary<string, DateTimeOffset> _cacheKeys = new ConcurrentDictionary<string, DateTimeOffset>();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public CacheService(IOptions<AppSettings> appSettings)
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _appSettings = appSettings;
        }

        public int GetCacheTimeout(IDataRepository dataRepository,
            string cachePrefix, string cacheSuffixPattern = null, bool isTableCache = false)
        {
            CacheConfiguration result = null;
            if (cacheSuffixPattern != null)
            {
                result = dataRepository.Get<CacheConfiguration>(
                    x => x.Enabled && x.KeyPrefix == cachePrefix
                                   && Regex.Match(cacheSuffixPattern, x.KeySuffixReg, RegexOptions.IgnoreCase).Success, true);
            }
            if (result != null) return result.Timeout;

            result = dataRepository.Get<CacheConfiguration>(x => x.Enabled && x.KeyPrefix == cachePrefix, true);

            if (result != null) return result.Timeout;

            return isTableCache ? _appSettings.Value.CacheTableTimeout : _appSettings.Value.CacheDefaultTimeout;
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
            Logger.Debug($"Remove.BEGIN(key={key}, count={_cacheKeys.Count})");
            //foreach (var cacheKey in _cacheKeys.Keys.Where(x => x.Contains(key, StringComparison.CurrentCultureIgnoreCase)).ToList())
            foreach (var cacheKey in _cacheKeys.Keys.Where(x => x.Contains(key)).ToList())
            {
                Logger.Debug($"Remove cacheKey={cacheKey}, absoluteExpiration={_cacheKeys[cacheKey]}");
                _cacheKeys.TryRemove(cacheKey, out DateTimeOffset outValue);
                _memoryCache.Remove(cacheKey);
            }
            Logger.Debug($"Remove.END(count={_cacheKeys.Count})");
        }

    }
}
