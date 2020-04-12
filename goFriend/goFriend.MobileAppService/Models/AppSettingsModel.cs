namespace goFriend.MobileAppService.Models
{
    public class AppSettingsModel
    {
        public int NotificationFetchingDays { get; set; }
        public int CacheTableTimeout { get; set; }
        public int CacheDefaultTimeout { get; set; }
        public string AzureSignalRConnectionString { get; set; }
    }
}
