using System;
using System.Collections.Generic;
using System.IO;
using Sceelix.Collections;
using Sceelix.Conversion;

namespace Sceelix.Core.Conversions
{
    [ConversionFunctions]
    public class StreamConversions
    {
        public static SceeList DictionaryToSceelist(Dictionary<string, object> dic)
        {
            return new SceeList(dic);
        }



        public static byte[] StreamToByteArray(Stream stream)
        {
            //always reset to beginning
            stream.Seek(0, SeekOrigin.Begin);

            //create a memory stream, better to handle
            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream.ToArray();
        }



        public static string StreamToString(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }



        public static string[] StreamToStringArray(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);

            return reader.ReadToEnd().Split(new[] {"\n", "\r\n"}, StringSplitOptions.None);
        }
    }
}