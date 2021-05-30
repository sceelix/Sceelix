using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sceelix.Core.Attributes;
using Sceelix.Meshes.Materials;
using Sceelix.Serialization;
using Sceelix.Unity.Annotations;

namespace Sceelix.Unity.Serialization
{
    [UnityJsonConverter(typeof(MeshMaterial))]
    public class MeshMaterialConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            //not needed
            throw new NotImplementedException();
        }



        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //not needed
            throw new NotImplementedException();
        }



        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var material = (MeshMaterial) value;

            var reference = material.Reference;

            writer.WriteStartObject();

            writer.WritePropertyName("Name");
            writer.WriteValue(reference);

            if (!serializer.HasReference(reference))
            {
                writer.WritePropertyName("Type");
                writer.WriteValue(material.Type);

                writer.WritePropertyName("Properties");
                writer.WriteStartObject();
                foreach (KeyValuePair<AttributeKey, object> keyValuePair in material.Attributes.OfKeyType<AttributeKey>())
                {
                    writer.WritePropertyName(keyValuePair.Key.Name);
                    serializer.Serialize(writer, keyValuePair.Value);
                }

                writer.WriteEndObject();

                serializer.AddObject(reference, material);
            }


            writer.WriteEndObject();
        }
    }
}