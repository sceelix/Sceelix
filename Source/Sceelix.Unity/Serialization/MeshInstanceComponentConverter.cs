using System;
using Newtonsoft.Json;
using Sceelix.Unity.Annotations;
using Sceelix.Unity.Data;

namespace Sceelix.Unity.Serialization
{
    [UnityJsonConverter(typeof(MeshInstanceComponent))]
    public class MeshInstanceComponentConverter : JsonConverter
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
            MeshInstanceComponent meshComponent = (MeshInstanceComponent) value;

            //in practice, we can just use the standard
            serializer.Serialize(writer, new MeshComponent(meshComponent.MeshInstanceEntity.MeshEntity));
        }
    }
}