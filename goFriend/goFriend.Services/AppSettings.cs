namespace goFriend.Services
{
    public class AppSettings
    {
        public int NotificationFetchingDays { get; set; }
        public int CacheTableTimeout { get; set; }
        public int CacheDefaultTimeout { get; set; }
        public string AzureSignalRConnectionString { get; set; }
    }
}
