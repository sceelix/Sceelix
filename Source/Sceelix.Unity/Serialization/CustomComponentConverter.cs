using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sceelix.Core.Attributes;
using Sceelix.Unity.Annotations;
using Sceelix.Unity.Data;

namespace Sceelix.Unity.Serialization
{
    [UnityJsonConverter(typeof(CustomComponent))]
    public class CustomComponentConverter : JsonConverter
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
            CustomComponent unityComponent = (CustomComponent) value;

            writer.WriteStartObject();
            writer.WritePropertyName("ComponentType");
            writer.WriteValue(unityComponent.Type);

            writer.WritePropertyName("ComponentName");
            writer.WriteValue(unityComponent.ComponentName);

            writer.WritePropertyName("Properties");
            writer.WriteStartArray();
            foreach (KeyValuePair<AttributeKey, object> keyValuePair in unityComponent.Attributes.OfKeyType<AttributeKey>())
            {
                writer.WriteStartObject();

                writer.WritePropertyName("Name");
                writer.WriteValue(keyValuePair.Key.Name);

                writer.WritePropertyName("Type");
                writer.WriteValue(keyValuePair.Value.GetType().FullName);

                writer.WritePropertyName("Value");
                serializer.Serialize(writer, keyValuePair.Value);

                writer.WriteEndObject();
            }

            writer.WriteEndArray();

            writer.WriteEndObject();
        }
    }
}