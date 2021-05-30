using System;
using Newtonsoft.Json;
using Sceelix.Annotations;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Serialization
{
    [StandardJsonConverter(typeof(Color))]
    public class ColorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }



        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }



        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Color color = (Color) value;

            writer.WriteValue(color.R / 255f + "," + color.G / 255f + "," + color.B / 255f + "," + color.A / 255f);
        }
    }
}