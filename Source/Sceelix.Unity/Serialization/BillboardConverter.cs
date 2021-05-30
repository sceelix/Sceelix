using System;
using Newtonsoft.Json;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Materials;
using Sceelix.Unity.Annotations;

namespace Sceelix.Unity.Serialization
{
    [UnityJsonConverter(typeof(BillboardEntity))]
    public class BillboardConverter : JsonConverter
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
            BillboardEntity billboardEntity = (BillboardEntity) value;

            writer.WriteStartObject();
            {
                writer.WritePropertyName("EntityType");
                writer.WriteValue("BillboardEntity");

                EntityConverter.SerializeCommonUnityAttributes(writer, billboardEntity, serializer);

                writer.WritePropertyName("Position");
                serializer.Serialize(writer, billboardEntity.BoxScope.Translation.FlipYZ());

                writer.WritePropertyName("ForwardVector");
                serializer.Serialize(writer, billboardEntity.BoxScope.YAxis.FlipYZ());

                writer.WritePropertyName("UpVector");
                serializer.Serialize(writer, billboardEntity.BoxScope.ZAxis.FlipYZ());

                writer.WritePropertyName("Scale");
                serializer.Serialize(writer, billboardEntity.BoxScope.Sizes.FlipYZ().ReplaceValue(0, 1));

                writer.WritePropertyName("Image");
                serializer.Serialize(writer, new ResourcePath(billboardEntity.Image));
                /*writer.WriteStartObject();
                {
                    
                }
                writer.WriteEndObject();*/
            }
            writer.WriteEndObject();
        }
    }
}