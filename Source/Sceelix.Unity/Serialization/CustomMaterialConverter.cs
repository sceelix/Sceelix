using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sceelix.Core.Attributes;
using Sceelix.Meshes.Materials;
using Sceelix.Serialization;
using Sceelix.Unity.Annotations;

namespace Sceelix.Unity.Serialization
{
    [UnityJsonConverter(typeof(CustomMaterial))]
    public class CustomMaterialConverter : JsonConverter
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
            var material = (CustomMaterial) value;

            var reference = "Material_" + material.ShaderName + "_" + material.Attributes.Hash;

            writer.WriteStartObject();

            writer.WritePropertyName("Name");
            writer.WriteValue(reference);

            if (!serializer.HasReference(reference))
            {
                writer.WritePropertyName("Type");
                writer.WriteValue(material.Type);

                writer.WritePropertyName("Shader");
                writer.WriteValue(material.ShaderName);

                writer.WritePropertyName("Properties");
                writer.WriteStartArray();
                foreach (KeyValuePair<AttributeKey, object> keyValuePair in material.Attributes.OfKeyType<AttributeKey>())
                {
                    writer.WriteStartObject();

                    writer.WritePropertyName("Name");
                    writer.WriteValue(keyValuePair.Key.Name);

                    writer.WritePropertyName("Value");
                    serializer.Serialize(writer, keyValuePair.Value);

                    writer.WritePropertyName("Type");
                    writer.WriteValue(keyValuePair.Value.GetType().Name);

                    writer.WriteEndObject();
                }

                writer.WriteEndArray();

                serializer.AddObject(reference, material);
            }


            writer.WriteEndObject();
        }
    }
}