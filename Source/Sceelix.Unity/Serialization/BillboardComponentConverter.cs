using System;
using Newtonsoft.Json;
using Sceelix.Meshes.Materials;
using Sceelix.Unity.Annotations;
using Sceelix.Unity.Data;

namespace Sceelix.Unity.Serialization
{
    [UnityJsonConverter(typeof(BillboardComponent))]
    public class BillboardComponentConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            //not used
            throw new NotImplementedException();
        }



        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //not used
            throw new NotImplementedException();
        }



        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            BillboardComponent billboardComponent = (BillboardComponent) value;

            writer.WriteStartObject();
            {
                writer.WritePropertyName("ComponentType");
                writer.WriteValue("Billboard");

                writer.WritePropertyName("Image");
                serializer.Serialize(writer, new ResourcePath(billboardComponent.BillboardEntity.Image));
            }
            writer.WriteEndObject();
        }
    }
}