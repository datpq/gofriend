using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace goFriend.DataModel
{
    public enum FacebookImageType
    {
        small, // 50 x 50
        normal, // 100 x 100
        album, // 50 x 50
        large, // 200 x 200
        square // 50 x 50
    }

    public static class Extension
    {
        public const string ParamSearchText = "SearchText";
        public const string ParamCategory = "Cat";
        public const string ParamOtherFriendId = "OtherFriendId";
        public const string ParamIsActive = "IsActive";
        public const string Sep = ",";

        public static string ToStringStandardFormat(this TimeSpan ts)
        {
            return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
        }

        public static string CapitalizeFirstLetter(this string str)
        {
            switch (str.Length)
            {
                case 0:
                    return string.Empty;
                case 1:
                    return char.ToUpper(str[0]).ToString();
                default:
                    return (char.ToUpper(str[0]) + str.Substring(1));
            }
        }

        /// <summary>
        /// Perform a deep Copy of the object, using Json as a serialisation method. NOTE: Private members are not cloned using this method.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T CloneJson<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }

        public static string GetImageUrl(this Friend friend, FacebookImageType imageType = FacebookImageType.normal)
        {
            string result;
            if (!string.IsNullOrEmpty(friend.FacebookId))
            {
                result = $"https://graph.facebook.com/{friend.FacebookId}/picture?type={imageType}";
                //Logger.Debug($"URL = {result}");
            }
            else if (friend.ThirdPartyLogin == ThirdPartyLogin.Apple)
            {
                result = GetImageUrl("apple.jpg");
            }
            else
            {
                result = friend.Gender == "female" ? "default_female.jpg" : "default_male.jpg";
                result = GetImageUrl(result);
            }

            return result;
        }

        public static string GetImageUrl(string fileName)
        {
            return $"resource://goFriend.Images.{fileName}";
        }

        public static string GetImageUrlByFacebookId(string facebookId, FacebookImageType imageType = FacebookImageType.normal)
        {
            string result;
            if (!string.IsNullOrEmpty(facebookId))
            {
                result = $"https://graph.facebook.com/{facebookId}/picture?type={imageType}";
                //Logger.Debug($"URL = {result}");
            }
            else
            {
                result = GetImageUrl("default_male.jpg");
            }
            return result;
        }

        public static string Linkify(this string text)
        {
            // www|http|https|ftp|news|file
            text = Regex.Replace(
                    text,
                    @"((www\.|(http|https|ftp|news|file)+\:\/\/)[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])",
                    "<a href=\"$1\" target=\"_blank\">$1</a>",
                    RegexOptions.IgnoreCase)
                .Replace("href=\"www", "href=\"http://www");

            // mailto
            text = Regex.Replace(
                text,
                @"(([a-zA-Z0-9_\-\.])+@[a-zA-Z\ ]+?(\.[a-zA-Z]{2,6})+)",
                "<a href=\"mailto:$1\">$1</a>",
                RegexOptions.IgnoreCase);

            return text;
        }
    }
}
