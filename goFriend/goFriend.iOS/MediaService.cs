using System;
using System.Drawing;
using goFriend.iOS;
using goFriend.Services;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(MediaService))]
namespace goFriend.iOS
{
    public class MediaService : IMediaService
    {
        public UIKit.UIImage ImageFromByteArray(byte[] data)
        {
            if (data == null)
                return null;

            return new UIKit.UIImage(Foundation.NSData.FromArray(data));
        }

        public byte[] ResizeImage(byte[] imageData, float width, float height)
        {

            UIImage originalImage = ImageFromByteArray(imageData);

            var originalHeight = originalImage.Size.Height;
            var originalWidth = originalImage.Size.Width;

            nfloat newHeight = 0;
            nfloat newWidth = 0;

            if (originalHeight > originalWidth)
            {
                newHeight = height;
                nfloat ratio = originalHeight / height;
                newWidth = originalWidth / ratio;
            }
            else
            {
                newWidth = width;
                nfloat ratio = originalWidth / width;
                newHeight = originalHeight / ratio;
            }

            width = (float)newWidth;
            height = (float)newHeight;

            UIGraphics.BeginImageContext(new SizeF(width, height));
            originalImage.Draw(new RectangleF(0, 0, width, height));
            var resizedImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            var bytesImagen = resizedImage.AsJPEG().ToArray();
            resizedImage.Dispose();
            return bytesImagen;
        }
    }
}