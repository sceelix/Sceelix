using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Sceelix.Extensions
{
    public static class BitmapExtension
    {
        public static readonly string[] SupportedFileExtensions = {".jpg", ".jpeg", ".bmp", ".png", ".gif", ".tiff", ".tif"};



        /// <summary>
        /// Converts the given bitmap to the indicated type. Clones the bitmaps in the process.
        /// </summary>
        /// <param name="bitmap">Bitmap to convert.</param>
        /// <param name="format">Format to which the bitmap should be converted. If the format is already the requested one, the same bitmap is returned.</param>
        /// <param name="dispose">Determines if the original image should be disposed. True by default.</param>
        /// <returns>The converted bitmap.</returns>
        public static Bitmap ConvertFormat(this Bitmap bitmap, PixelFormat format, bool dispose = true)
        {
            if (bitmap.PixelFormat == format)
                return bitmap;

            Bitmap clone = new Bitmap(bitmap.Width, bitmap.Height, format);

            using (Graphics gr = Graphics.FromImage(clone))
            {
                gr.DrawImage(bitmap, new Rectangle(0, 0, clone.Width, clone.Height));
            }

            if (dispose)
                bitmap.Dispose();

            return clone;
        }



        /// <summary>
        /// Creates a 1x1 color bitmap.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Bitmap CreateColorBitmap(Color color, PixelFormat format = PixelFormat.Format32bppArgb)
        {
            Bitmap bitmap = new Bitmap(1, 1, format);

            using (Graphics gr = Graphics.FromImage(bitmap))
            {
                gr.DrawRectangle(new Pen(color), new Rectangle(0, 0, 1, 1));
            }

            return bitmap;
        }



        public static byte[] ImageToByte(this Image img)
        {
            byte[] byteArray = new byte[0];

            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, ImageFormat.Png);
                stream.Close();

                byteArray = stream.ToArray();
            }

            return byteArray;
        }
    }
}