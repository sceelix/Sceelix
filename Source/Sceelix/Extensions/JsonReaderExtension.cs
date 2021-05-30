using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sceelix.Extensions
{
    public static class JsonReaderExtension
    {
        public static bool IsProperty(this JsonReader reader, string propertyName)
        {
            return reader.TokenType == JsonToken.PropertyName && (string) reader.Value == propertyName;
        }



        public static IEnumerable<JsonReader> ReadArray(this JsonReader reader)
        {
            //read the <Array>
            reader.Read();

            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                yield return reader;
        }
    }
}