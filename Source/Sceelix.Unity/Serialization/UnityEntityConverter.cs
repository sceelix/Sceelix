using System;
using Newtonsoft.Json;
using Sceelix.Serialization;
using Sceelix.Unity.Annotations;
using Sceelix.Unity.Data;

namespace Sceelix.Unity.Serialization
{
    [UnityJsonConverter(typeof(UnityEntity))]
    public class UnityEntityConverter : JsonConverter
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
            UnityEntity unityEntity = (UnityEntity) value;

            writer.WriteStartObject();

            writer.WritePropertyName("EntityType");
            writer.WriteValue("UnityEntity");

            writer.WritePropertyName("Name");
            writer.WriteValue(unityEntity.Name);

            //the following properties are not sent unless they actually differ from the defaults
            //this helps saving data transfer
            if (!string.IsNullOrWhiteSpace(unityEntity.Tag))
            {
                writer.WritePropertyName("Tag");
                writer.WriteValue(unityEntity.Tag);
            }

            if (!string.IsNullOrWhiteSpace(unityEntity.Layer))
            {
                writer.WritePropertyName("Layer");
                writer.WriteValue(unityEntity.Layer);
            }

            if (unityEntity.Static)
            {
                writer.WritePropertyName("Static");
                writer.WriteValue(unityEntity.Static);
            }

            if (unityEntity.Enabled)
            {
                writer.WritePropertyName("Enabled");
                writer.WriteValue(unityEntity.Enabled);
            }

            if (!string.IsNullOrWhiteSpace(unityEntity.Prefab))
            {
                writer.WritePropertyName("Prefab");
                writer.WriteValue(unityEntity.Prefab);
            }

            if (!string.IsNullOrWhiteSpace(unityEntity.ScaleMode))
            {
                writer.WritePropertyName("ScaleMode");
                writer.WriteValue(unityEntity.ScaleMode);
            }

            if (!string.IsNullOrWhiteSpace(unityEntity.Positioning))
            {
                writer.WritePropertyName("Positioning");
                writer.WriteValue(unityEntity.Positioning);
            }

            //the coordinates have to be converted
            //Unity has different definitions of Up, so we need to flip the Y-Z coordinates
            writer.WritePropertyName("Position");
            serializer.Serialize(writer, unityEntity.BoxScope.Translation.FlipYZ());


            writer.WritePropertyName("ForwardVector");
            serializer.Serialize(writer, unityEntity.BoxScope.YAxis.FlipYZ()); //needs pitch, yaw, roll

            writer.WritePropertyName("UpVector");
            serializer.Serialize(writer, unityEntity.BoxScope.ZAxis.FlipYZ()); //needs pitch, yaw, roll

            //the actual unity "scale" corresponds to the boxscope sizes, multiplied by its internal scale
            //which is the scale to the children (meshes, terrains, etc.)
            writer.WritePropertyName("Scale");
            serializer.Serialize(writer, unityEntity.Scale.FlipYZ());

            writer.WritePropertyName("Size");
            serializer.Serialize(writer, unityEntity.BoxScope.Sizes.FlipYZ());

            serializer.SetObject("UnityEntity", unityEntity);

            writer.WritePropertyName("Components");
            writer.WriteStartArray();
            foreach (UnityComponent unityComponent in unityEntity.GameComponents)
                serializer.Serialize(writer, unityComponent);
            writer.WriteEndArray();

            writer.WriteEndObject();
        }
    }
}