using Newtonsoft.Json;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using goFriend.DataModel;

namespace goFriend
{
    public static class Settings
    {
        private static ISettings AppSettings => CrossSettings.Current;

        public static bool IsUserLoggedIn
        {
            get => AppSettings.GetValueOrDefault(nameof(IsUserLoggedIn), false);
            set => AppSettings.AddOrUpdateValue(nameof(IsUserLoggedIn), value);
        }

        public static Friend LastUser
        {
            get => JsonConvert.DeserializeObject<Friend>(AppSettings.GetValueOrDefault(nameof(LastUser), string.Empty));
            set => AppSettings.AddOrUpdateValue(nameof(LastUser), JsonConvert.SerializeObject(value));
        }

        public static string LastGroupName
        {
            get => AppSettings.GetValueOrDefault(nameof(LastGroupName), string.Empty);
            set => AppSettings.AddOrUpdateValue(nameof(LastGroupName), value);
        }

        public static string LastBrowsePageGroupNme
        {
            get => AppSettings.GetValueOrDefault(nameof(LastBrowsePageGroupNme), string.Empty);
            set => AppSettings.AddOrUpdateValue(nameof(LastBrowsePageGroupNme), value);
        }

        public static string LastMapPageGroupNme
        {
            get => AppSettings.GetValueOrDefault(nameof(LastMapPageGroupNme), string.Empty);
            set => AppSettings.AddOrUpdateValue(nameof(LastMapPageGroupNme), value);
        }

        public static string LastAdminPageGroupNme
        {
            get => AppSettings.GetValueOrDefault(nameof(LastAdminPageGroupNme), string.Empty);
            set => AppSettings.AddOrUpdateValue(nameof(LastAdminPageGroupNme), value);
        }
    }
}
