namespace goFriend.MobileAppService.Data
{
    public interface ICacheConfigurationService
    {
        int GetCacheTimeout(string cachePrefix, string cacheSuffixPattern = null, bool isTableCache = false);
    }
}
