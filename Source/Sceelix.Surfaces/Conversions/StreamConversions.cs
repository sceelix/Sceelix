using System.Drawing;
using System.IO;
using Sceelix.Conversion;

namespace Sceelix.Surfaces.Conversions
{
    [ConversionFunctions]
    public class StreamConversions
    {
        public static Bitmap StreamToBitmap(Stream stream)
        {
            return new Bitmap(stream);
        }
    }
}