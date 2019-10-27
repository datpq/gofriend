using System.Text.RegularExpressions;
using goFriend.DataModel;
using goFriend.MobileAppService.Models;
using Microsoft.Extensions.Options;

namespace goFriend.MobileAppService.Data
{
    public class CacheConfigurationService : ICacheConfigurationService
    {
        private readonly IOptions<AppSettingsModel> _appSettings;
        private readonly IDataRepository _dataRepository;

        public CacheConfigurationService(IOptions<AppSettingsModel> appSettings, IDataRepository dataRepository)
        {
            _appSettings = appSettings;
            _dataRepository = dataRepository;
        }

        public int GetCacheTimeout(string cachePrefix, string cacheSuffixPattern = null, bool isTableCache = false)
        {
            CacheConfiguration result = null;
            if (cacheSuffixPattern != null)
            {
                result = _dataRepository.Get<CacheConfiguration>(
                    x => x.Enabled && x.KeyPrefix == cachePrefix
                                   && Regex.Match(cacheSuffixPattern, x.KeySuffixReg, RegexOptions.IgnoreCase).Success, true);
            }
            if (result != null) return result.Timeout;

            result = _dataRepository.Get<CacheConfiguration>(x => x.Enabled && x.KeyPrefix == cachePrefix, true);

            if (result != null) return result.Timeout;

            return isTableCache ? _appSettings.Value.CacheTableTimeout : _appSettings.Value.CacheDefaultTimeout;
        }
    }
}
