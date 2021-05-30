using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sceelix.Core.Attributes;
using Sceelix.Unity.Annotations;
using Sceelix.Unity.Data;

namespace Sceelix.Unity.Serialization
{
    [UnityJsonConverter(typeof(UnityComponent))]
    public class UnityComponentConverter : JsonConverter
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
            UnityComponent unityComponent = (UnityComponent) value;

            writer.WriteStartObject();
            writer.WritePropertyName("ComponentType");
            writer.WriteValue(unityComponent.Type);

            writer.WritePropertyName("Properties");
            writer.WriteStartObject();
            foreach (KeyValuePair<AttributeKey, object> keyValuePair in unityComponent.Attributes.OfKeyType<AttributeKey>())
            {
                writer.WritePropertyName(keyValuePair.Key.Name);
                serializer.Serialize(writer, keyValuePair.Value);
            }

            writer.WriteEndObject();

            writer.WriteEndObject();
        }
    }
}