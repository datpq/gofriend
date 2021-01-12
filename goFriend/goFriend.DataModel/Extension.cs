using System;
using System.Collections.Generic;
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

    public enum ChatType
    {
        StandardGroup,
        MixedGroup,
        Individual,
    }

    public static class Extension
    {
        public const string ParamSearchText = "SearchText";
        public const string ParamCategory = "Cat";
        public const string ParamOtherFriendId = "OtherFriendId";
        public const string ParamIsActive = "IsActive";
        public const char Sep = ',';
        public const char SepMain = ';';
        public const char SepSub = ':';
        public static ChatMessageType[] RealShowableMessageTypes =
            new[] { ChatMessageType.Text, ChatMessageType.Attachment, ChatMessageType.CreateChat };

        public static string ToStringStandardFormat(this TimeSpan ts)
        {
            return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
        }

        public static bool IsRealShowableMessage(this ChatMessageType chatMessageType)
        {
            return Array.IndexOf(RealShowableMessageTypes, chatMessageType) > -1;
        }

        public static string TruncateAtWord(this string input, int length)
        {
            if (input == null || input.Length < length)
                return input;
            int iNextSpace = input.LastIndexOf(" ", length, StringComparison.Ordinal);
            return string.Format("{0}…", input.Substring(0, (iNextSpace > 0) ? iNextSpace : length).Trim());
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

        //public static bool MembersEqual(this Chat chat1, Chat chat2)
        //{
        //    var arrMembers1 = chat1.Members.Split(Sep.ToCharArray());
        //    Array.Sort(arrMembers1);
        //    var arrMembers2 = chat2.Members.Split(Sep.ToCharArray());
        //    Array.Sort(arrMembers2);
        //    return arrMembers1.SequenceEqual(arrMembers2);
        //}

        public static ChatType GetChatType(this Chat chat)
        {
            if (Regex.Match(chat.Members, @"^g\d+$").Success)
            {
                return ChatType.StandardGroup;
            }
            if (Regex.Match(chat.Members, @"^u\d+,u\d+$").Success)
            {
                return ChatType.Individual;
            }
            //if (Regex.Match(chat.Members, @"^([ug]\d+)(,[ug]\d+)*$").Success)
            return ChatType.MixedGroup;
        }

        public static int GetMemberGroupId(this Chat chat)
        {
            if (chat.Members.StartsWith("g"))
            {
                return int.Parse(chat.Members.Substring(1));
            }
            return 0;
        }

        public static int[] GetMemberIds(this Chat chat)
        {
            var arrResults = new List<int>();
            var arrMembers = chat.Members.Split(Sep);
            foreach(var m in arrMembers)
            {
                if (m.StartsWith("u"))
                {
                    arrResults.Add(int.Parse(m.Substring(1)));
                }
            }
            return arrResults.ToArray();
        }

        public static bool MembersContain(this Chat chat, int memberId)
        {
            return $"{Sep}{chat.Members}{Sep}".IndexOf($"{Sep}u{memberId}{Sep}", StringComparison.Ordinal) >= 0;
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
