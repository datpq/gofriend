using Android.App;
using Android.Graphics;
using System.Net;

namespace goFriend.Droid
{
    public static class ExtensionAndroid
    {
        public static Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;

            using (var webClient = new WebClient())
            {
                var imageBytes = webClient.DownloadData(url);
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }
            return imageBitmap;
        }

        public static Bitmap CreateRoundedBitmap(this Bitmap bitmap/*, int padding*/)
        {
            Bitmap output = Bitmap.CreateBitmap(bitmap.Width, bitmap.Height, Bitmap.Config.Argb8888);
            var canvas = new Canvas(output);

            var paint = new Paint();
            var rect = new Rect(0, 0, bitmap.Width, bitmap.Height);
            var rectF = new RectF(rect);

            paint.AntiAlias = true;
            canvas.DrawARGB(0, 0, 0, 0);
            canvas.DrawOval(rectF, paint);
            paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.SrcIn));

            canvas.DrawBitmap(bitmap, rect, rect, paint);
            return output;
        }

        public const string CHARCODE_PIN = "\uF1AE";
        public const string CHARCODE_PIN_FEMALE = "\uF182";
        public static Bitmap ICON_PIN_ADMIN = CreateTextBitmap(CHARCODE_PIN, Color.DarkViolet);
        public static Bitmap ICON_PIN_ADMIN_FEMALE = CreateTextBitmap(CHARCODE_PIN_FEMALE, Color.DarkViolet);
        public static Bitmap ICON_PIN_NORMAL = CreateTextBitmap(CHARCODE_PIN, MainActivity.COLOR_PRIMARY);
        public static Bitmap ICON_PIN_NORMAL_FEMALE = CreateTextBitmap(CHARCODE_PIN_FEMALE, Color.ParseColor("#FF6699"));
        public static Bitmap ICON_PIN_PENDING = CreateTextBitmap(CHARCODE_PIN, Color.Gray);
        public static Bitmap ICON_PIN_PENDING_FEMALE = CreateTextBitmap(CHARCODE_PIN_FEMALE, Color.Gray);

        public static Bitmap CreateTextBitmap(string text, Color color,
            string fontName = "fa-solid-900.ttf", int bitmapSize = 48, int fontSize = 48)
        {
            Bitmap bmp = Bitmap.CreateBitmap(bitmapSize, bitmapSize, Bitmap.Config.Argb8888);
            Canvas can = new Canvas(bmp);
            Paint paint = new Paint();
            //myCanvas.DrawRect(0, 0, size, size, paint);
            Typeface typeface = Typeface.CreateFromAsset(Application.Context.Assets, fontName);
            paint.AntiAlias = true;
            paint.SubpixelText = true;
            paint.SetTypeface(typeface);
            paint.SetStyle(Paint.Style.Fill);
            paint.Color = color;
            paint.TextSize = fontSize;
            can.DrawText(text, (bitmapSize-fontSize)/2, (bitmapSize+fontSize)/2 * (7f / 8), paint);// * (7f/8)
            return bmp;
        }
    }
}