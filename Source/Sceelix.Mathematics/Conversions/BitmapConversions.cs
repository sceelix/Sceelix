using System.Drawing;
using Sceelix.Conversion;
using Sceelix.Extensions;

namespace Sceelix.Mathematics.Conversions
{
    [ConversionFunctions]
    public class BitmapConversions
    {
        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            return bitmap.ImageToByte();
        }
    }
}