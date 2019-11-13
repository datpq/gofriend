using goFriend.DataModel;
using goFriend.Services;
using Xamarin.Forms;

namespace goFriend
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
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public static string GetImageUrl(this Friend friend, FacebookImageType imageType = FacebookImageType.normal)
        {
            string result;
            if (!string.IsNullOrEmpty(friend.FacebookId))
            {
                result = $"https://graph.facebook.com/{friend.FacebookId}/picture?type={imageType}";
                Logger.Debug($"URL = {result}");
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

        /*
        public static ImageSource GetImageSource(this Friend friend, FacebookImageType imageType = FacebookImageType.normal)
        {
            ImageSource result;
            if (!string.IsNullOrEmpty(friend.FacebookId))
            {
                var url = friend.GetImageUrl(imageType);
                Logger.Debug($"ImageUrl url = {url}");
                //result = ImageSource.FromUri(new Uri(url));
                using (var webClient = new WebClient())
                {
                    var imageBytes = webClient.DownloadData(url);
                    result = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                }
            }
            else
            {
                result = GetImageSourceFromFile(friend.Gender == "female" ? "default_female.jpg" : "default_male.jpg");
            }

            return result;
        }

        public static ImageSource GetImageSourceFromFile(string fileName)
        {
            return ImageSource.FromResource($"goFriend.Images.{fileName}", typeof(ImageResourceExtension).GetTypeInfo().Assembly);
        }
        */
    }
}
