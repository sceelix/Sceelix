using System;
using Newtonsoft.Json;
using Sceelix.Serialization;
using Sceelix.Unity.Annotations;
using Sceelix.Unity.Data;

namespace Sceelix.Unity.Serialization
{
    [UnityJsonConverter(typeof(MeshComponent))]
    public class MeshComponentConverter : JsonConverter
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
            MeshComponent meshComponent = (MeshComponent) value;
            var meshEntity = meshComponent.MeshEntity;

            //we have to avoid sending the same mesh entity several times, which can occur if we are sending mesh instances
            //we use the hash code of the meshentity to identify the object
            //and only create the objects
            string reference = "UnityMeshWithMaterials_" + serializer.GetUniqueId(meshEntity);
            var meshWithMaterials = serializer.GetObject(reference) as UnityMeshWithMaterials;
            if (meshWithMaterials == null)
            {
                meshWithMaterials = new UnityMeshWithMaterials(meshEntity);

                serializer.AddObject(reference, meshWithMaterials);
            }

            writer.WriteStartObject();
            writer.WritePropertyName("ComponentType");
            writer.WriteValue("MeshFilter");

            writer.WritePropertyName("Mesh");
            serializer.Serialize(writer, meshWithMaterials.UnityMesh);
            writer.WriteEndObject();


            writer.WriteStartObject();
            writer.WritePropertyName("ComponentType");
            writer.WriteValue("MeshRenderer");

            writer.WritePropertyName("Materials");
            writer.WriteStartArray();
            foreach (var material in meshWithMaterials.Materials)
                serializer.Serialize(writer, material);
            writer.WriteEndArray();
            writer.WriteEndObject();

            //serializer.Serialize(writer, meshComponent.MeshEntity);
        }
    }
}