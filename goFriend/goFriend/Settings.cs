using Newtonsoft.Json;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using goFriend.DataModel;
using System.Collections.Generic;
using goFriend.Services;
using System;

namespace goFriend
{
    public static class Settings
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        private static ISettings AppSettings => CrossSettings.Current;

        public static bool IsUserLoggedIn
        {
            get => AppSettings.GetValueOrDefault(nameof(IsUserLoggedIn), false);
            set => AppSettings.AddOrUpdateValue(nameof(IsUserLoggedIn), value);
        }

        public static bool IsTracing
        {
            get => AppSettings.GetValueOrDefault(nameof(IsTracing), true);
            set => AppSettings.AddOrUpdateValue(nameof(IsTracing), value);
        }

        public static Friend LastUser
        {
            get
            {
                string json = null;
                try
                {
                    json = AppSettings.GetValueOrDefault(nameof(LastUser), string.Empty);
                    return JsonConvert.DeserializeObject<Friend>(json);
                }
                catch (Exception e)
                {
                    Logger.Error($"DeserializeObject LastUser json={json}");
                    Logger.Error(e.ToString());
                    return null;
                }
            }
            set => AppSettings.AddOrUpdateValue(nameof(LastUser), JsonConvert.SerializeObject(value));
        }

        public static Dictionary<int, int> LastMsgIdxRetrievedByChatId
        {
            get => JsonConvert.DeserializeObject<Dictionary<int, int>>(
                AppSettings.GetValueOrDefault(nameof(LastMsgIdxRetrievedByChatId), string.Empty));
            set => AppSettings.AddOrUpdateValue(nameof(LastMsgIdxRetrievedByChatId), JsonConvert.SerializeObject(value));
        }

        //public static string LastGroupName
        //{
        //    get => AppSettings.GetValueOrDefault(nameof(LastGroupName), string.Empty);
        //    set => AppSettings.AddOrUpdateValue(nameof(LastGroupName), value);
        //}

        public static string LastBrowsePageGroupName
        {
            get => AppSettings.GetValueOrDefault(nameof(LastBrowsePageGroupName), string.Empty);
            set => AppSettings.AddOrUpdateValue(nameof(LastBrowsePageGroupName), value);
        }

        public static string LastMapPageGroupName
        {
            get => AppSettings.GetValueOrDefault(nameof(LastMapPageGroupName), string.Empty);
            set => AppSettings.AddOrUpdateValue(nameof(LastMapPageGroupName), value);
        }

        public static string LastAdminPageGroupNme
        {
            get => AppSettings.GetValueOrDefault(nameof(LastAdminPageGroupNme), string.Empty);
            set => AppSettings.AddOrUpdateValue(nameof(LastAdminPageGroupNme), value);
        }
    }
}
