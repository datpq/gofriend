using CoreGraphics;
using Foundation;
using UIKit;

namespace goFriend.iOS
{
    public class ExtensionIOS
    {
        public const string CHARCODE_PIN = "\uF1AE";
        public static UIColor COLOR_PIN_ADMIN = FromHex(0x9400d3);
        public static UIColor COLOR_PIN_NORMAL = FromHex(0x61A830);
        public static UIColor COLOR_PIN_PENDING = UIColor.Gray;
        public static UIImage ICON_PIN_ADMIN = ImageWithFont(CHARCODE_PIN, COLOR_PIN_ADMIN);
        public static UIImage ICON_PIN_NORMAL = ImageWithFont(CHARCODE_PIN, COLOR_PIN_NORMAL);
        public static UIImage ICON_PIN_PENDING = ImageWithFont(CHARCODE_PIN, COLOR_PIN_PENDING);

        public static UIImage ImageWithFont(string text, UIColor color, float fontSize = 32, string fontName = "Font Awesome 5 Free")
        {
            var imageSize = new CGSize(fontSize, fontSize);
            UIGraphics.BeginImageContextWithOptions(imageSize, false, 0.0f);

            NSAttributedString attrString = new NSAttributedString(text, new UIStringAttributes()
            {
                Font = UIFont.FromName(fontName, fontSize),
                ForegroundColor = color
            });

            NSStringDrawingContext strContext = new NSStringDrawingContext();
            var rectFont = attrString.GetBoundingRect(imageSize, NSStringDrawingOptions.UsesDeviceMetrics, strContext);
            attrString.DrawString(new CGRect(imageSize.Width / 2 - rectFont.Width / 2, imageSize.Height / 2 - rectFont.Height / 2, imageSize.Width, imageSize.Height));

            UIImage image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            return image;
        }

        public static UIColor FromHex(int hexValue)
        {
            return UIColor.FromRGB(
                (((float)((hexValue & 0xFF0000) >> 16)) / 255.0f),
                (((float)((hexValue & 0xFF00) >> 8)) / 255.0f),
                (((float)(hexValue & 0xFF)) / 255.0f)
            );
        }
    }
}