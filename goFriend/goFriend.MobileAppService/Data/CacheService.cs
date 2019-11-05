using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using goFriend.DataModel;
using goFriend.MobileAppService.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NLog;

namespace goFriend.MobileAppService.Data
{
    public class CacheService : ICacheService
    {
        private readonly IOptions<AppSettingsModel> _appSettings;
        private readonly IMemoryCache _memoryCache;
        private readonly Dictionary<string, DateTimeOffset> _cacheKeys = new Dictionary<string, DateTimeOffset>();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public CacheService(IMemoryCache memoryCache, IOptions<AppSettingsModel> appSettings)
        {
            _memoryCache = memoryCache;
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
            Logger.Debug($"BEGIN(key={key}, count={_cacheKeys.Keys.Count})");
            foreach (var cacheKey in _cacheKeys.Keys.Where(x => x.Contains(key, StringComparison.CurrentCultureIgnoreCase)).ToList())
            {
                Logger.Debug($"Remove cacheKey={cacheKey}, absoluteExpiration={_cacheKeys[cacheKey]}");
                _cacheKeys.Remove(cacheKey);
                _memoryCache.Remove(cacheKey);
            }
            Logger.Debug($"END(count={_cacheKeys.Keys.Count})");
        }

    }
}
