using Android.App;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Text;
using Android.Text.Style;
using goFriend.Droid;
using goFriend.Models;
using goFriend.Services;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Color = Android.Graphics.Color;


[assembly: Dependency(typeof(NotificationService))]
namespace goFriend.Droid
{
    public class NotificationService : INotificationService
    {
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());
        private IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());

        const string CacheBitmapPrefix = "CacheBitmap.";

        public void SendNotification(ServiceNotification serviceNotification)
        {
            var foregroundService = LocationForegroundService.GetInstance();
            if (serviceNotification == null || foregroundService  == null) return;
            if (serviceNotification.NotificationType == NotificationType.FriendsAroundStatusChanged) return;

            Logger.Debug($"SendNotification.BEGIN(NotificationType={serviceNotification.NotificationType})");
            var builder = new Notification.Builder(foregroundService)
                    .SetColor(new Color(ContextCompat.GetColor(LocationForegroundService.GetInstance(), Resource.Color.colorPrimary)))
                    .SetContentTitle(serviceNotification.ContentTitle)
                    .SetContentText(serviceNotification.ContentText)
                    .SetSmallIcon(Resource.Drawable.logo)
                    .SetShowWhen(true)
                    .SetAutoCancel(true);

            try
            {
                var cacheKey = $"{CacheBitmapPrefix}{serviceNotification.LargeIconUrl}.";
                var largeIcon = _memoryCache.Get(cacheKey) as Bitmap;
                if (largeIcon == null)
                {
                    Logger.Debug($"First time getting url: {serviceNotification.LargeIconUrl}");
                    if (serviceNotification.LargeIconUrl.Contains("//"))
                    {
                        var largeIconFromUrl = ExtensionAndroid.GetImageBitmapFromUrl(serviceNotification.LargeIconUrl);
                        if (largeIcon != null)
                        {
                            largeIcon = largeIconFromUrl.CreateRoundedBitmap(/*10*/);
                        }
                    } else
                    {
                        largeIcon = ExtensionAndroid.CreateTextBitmap(serviceNotification.LargeIconUrl, MainActivity.COLOR_PRIMARY, fontSize:32);
                        //largeIcon = largeIcon.CreateRoundedBitmap(/*10*/);
                    }
                    Logger.Debug($"CreateRoundedBitmap done.");
                    if (largeIcon != null)
                    {
                        _memoryCache.Set(cacheKey, largeIcon, DateTimeOffset.Now.AddDays(1));
                    }
                }

                builder.SetLargeIcon(largeIcon);
                //builder.SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.logo));
            }
            catch (Exception e)
            {
                Logger.TrackError(e);
            }

            switch (serviceNotification.NotificationType)
            {
                case NotificationType.AppearOnMap:
                    builder.SetContentIntent(foregroundService.BuildIntentToShowMainActivity(Constants.ACTION_GOTO_MAPONLINE));
                    BuildNotificationStyle(builder, serviceNotification.InboxLines);
                    break;
                case NotificationType.ChatReceiveMessage:
                    builder.SetContentIntent(foregroundService.BuildIntentToShowMainActivity(
                        Constants.ACTION_GOTO_CHAT, serviceNotification.ExtraId));
                    BuildNotificationStyle(builder, serviceNotification.InboxLines);
                    break;
                case NotificationType.ChatReceiveCreateChat:
                    builder.SetContentIntent(foregroundService.BuildIntentToShowMainActivity(
                        Constants.ACTION_GOTO_CHAT, serviceNotification.ExtraId));
                    var textStyle = new Notification.BigTextStyle();

                    textStyle.BigText(serviceNotification.ContentText);
                    textStyle.SetSummaryText(serviceNotification.SummaryText);
                    builder.SetStyle(textStyle);
                    break;
                default:
                    builder.SetContentIntent(foregroundService.BuildIntentToShowMainActivity(Constants.ACTION_GOTO_HOME));
                    break;
            }

            builder.SetVisibility(NotificationVisibility.Public);
            builder.SetPriority((int)NotificationPriority.High);

            // Build the notification:
            var notification = builder.Build();
            //notification.Defaults |= NotificationDefaults.Sound;
            notification.Defaults |= NotificationDefaults.Vibrate;

            // Launch notification:
            foregroundService.NotificationManager.Notify((int)serviceNotification.NotificationType, notification);

            // Uncomment this code to update the notification 5 seconds later:
            // Thread.Sleep(5000);
            // builder.SetContentTitle("Updated Notification");
            // builder.SetContentText("Changed to this message after five seconds.");
            // notification = builder.Build();
            // notificationManager.Notify(notificationId, notification);
            Logger.Debug($"SendNotification.END");
        }

        public void CancelNotification(NotificationType? notificationType = null)
        {
            var foregroundService = LocationForegroundService.GetInstance();
            if (foregroundService != null)
            {
                if (notificationType.HasValue)
                {
                    foregroundService.NotificationManager.Cancel((int)notificationType.Value);
                }
                else
                {
                    foregroundService.NotificationManager.CancelAll();
                }
            }
        }

        public static void BuildNotificationStyle(Notification.Builder builder,
            List<string[]> arrInboxLines, string contentText = null)
        {
            if  (arrInboxLines.Count > 0)
            {
                var inboxStyle = new Notification.InboxStyle();
                for (var i = 0; i < arrInboxLines.Count; i++)
                {
                    var inboxLine = arrInboxLines[i];
                    var sb = new SpannableString($"{inboxLine[0]}: {inboxLine[1]}");
                    sb.SetSpan(new StyleSpan(TypefaceStyle.Bold), 0, inboxLine[0].Length, SpanTypes.ExclusiveExclusive);
                    inboxStyle.AddLine(sb);
                    if (i == arrInboxLines.Count - 1)
                    {
                        builder.SetContentText(sb); //the last line is also the content text
                    }
                }

                if (arrInboxLines.Count > 1)
                {
                    inboxStyle.SetSummaryText(string.Format(res.MoreMessages, arrInboxLines.Count - 1));
                }
                builder.SetStyle(inboxStyle);
            }
            else
            {
                var textStyle = new Notification.BigTextStyle();

                // Use the text in the edit box at the top of the screen.
                textStyle.BigText(contentText);
                //textStyle.SetSummaryText("The summary text goes here.");

                builder.SetContentText(contentText);

                // Plug this style into the builder:
                builder.SetStyle(textStyle);
            }
        }
    }
}