using System;
using System.Globalization;
using Newtonsoft.Json;
using Sceelix.Annotations;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Serialization
{
    [StandardJsonConverter(typeof(Vector3D))]
    public class Vector3DConverter : JsonConverter
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
            Vector3D vector = (Vector3D) value;

            writer.WriteValue(vector.X.ToString(CultureInfo.InvariantCulture) + "," + vector.Y.ToString(CultureInfo.InvariantCulture) + "," + vector.Z.ToString(CultureInfo.InvariantCulture));
        }
    }
}