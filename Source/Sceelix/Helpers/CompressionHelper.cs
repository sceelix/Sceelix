using System.IO;
using System.IO.Compression;
using System.Text;

namespace Sceelix.Helpers
{
    public class CompressionHelper
    {
        public static string UnZipStr(byte[] input)
        {
            using (MemoryStream inputStream = new MemoryStream(input))
            {
                using (DeflateStream gzip =
                    new DeflateStream(inputStream, CompressionMode.Decompress))
                {
                    using (StreamReader reader =
                        new StreamReader(gzip, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }



        public static byte[] ZipStr(string str)
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (DeflateStream gzip =
                    new DeflateStream(output, CompressionMode.Compress))
                {
                    using (StreamWriter writer =
                        new StreamWriter(gzip, Encoding.UTF8))
                    {
                        writer.Write(str);
                    }
                }

                return output.ToArray();
            }
        }
    }
}