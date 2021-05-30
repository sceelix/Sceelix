using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sceelix.Annotations;

namespace Sceelix.Unity.Serialization
{
    //[UnityJsonConverter(typeof(Object))]
    public class ObjectConverter : JsonConverter
    {
        private static readonly Dictionary<Type, JsonConverter> _instancesOfType = AttributeReader.OfTypeKeyAttribute<StandardJsonConverterAttribute>().GetInstancesOfType<JsonConverter>();



        public override bool CanConvert(Type objectType)
        {
            return _instancesOfType[objectType].CanConvert(objectType);
        }



        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return _instancesOfType[objectType].ReadJson(reader, objectType, existingValue, serializer);
        }



        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            _instancesOfType[value.GetType()].WriteJson(writer, value, serializer);
        }
    }
}