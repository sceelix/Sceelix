using System;
using Newtonsoft.Json;
using Sceelix.Points.Data;
using Sceelix.Unity.Annotations;

namespace Sceelix.Unity.Serialization
{
    [UnityJsonConverter(typeof(PointEntity))]
    public class PointEntityConverter : JsonConverter
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
            var pointEntity = (PointEntity) value;

            writer.WriteStartObject();
            {
                writer.WritePropertyName("EntityType");
                writer.WriteValue("PointEntity");

                EntityConverter.SerializeCommonUnityAttributes(writer, pointEntity, serializer);

                writer.WritePropertyName("Position");
                serializer.Serialize(writer, pointEntity.Position.FlipYZ());
            }
            writer.WriteEndObject();
        }
    }
}