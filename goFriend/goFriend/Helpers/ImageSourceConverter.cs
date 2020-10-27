using System;
using System.Globalization;
using System.IO;
using System.Net;
using Xamarin.Forms;

namespace goFriend.Helpers
{
    public class ImageSourceConverter : IValueConverter
    {
        static WebClient Client = new WebClient();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            return ConvertFromImageUrl(value.ToString());
            //var byteArray = Client.DownloadData(value.ToString());
            //return ImageSource.FromStream(() => new MemoryStream(byteArray));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public static ImageSource ConvertFromImageUrl(string imageUrl)
        {
            var byteArray = Client.DownloadData(imageUrl);
            return ImageSource.FromStream(() => new MemoryStream(byteArray));
        }
    }
}
