using System;
using Newtonsoft.Json;
using Sceelix.Meshes.Data;
using Sceelix.Serialization;
using Sceelix.Unity.Annotations;
using Sceelix.Unity.Data;

namespace Sceelix.Unity.Serialization
{
    [UnityJsonConverter(typeof(MeshEntity))]
    public class MeshEntityConverter : JsonConverter
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
            var meshEntity = (MeshEntity) value;

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
            {
                writer.WritePropertyName("EntityType");
                writer.WriteValue("MeshEntity");

                EntityConverter.SerializeCommonUnityAttributes(writer, meshEntity, serializer);

                writer.WritePropertyName("MeshFilter");

                writer.WriteStartObject();
                {
                    writer.WritePropertyName("Mesh");
                    serializer.Serialize(writer, meshWithMaterials.UnityMesh);
                }
                writer.WriteEndObject();

                writer.WritePropertyName("MeshRenderer");
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Materials");
                    writer.WriteStartArray();
                    foreach (var material in meshWithMaterials.Materials)
                        serializer.Serialize(writer, material);
                    writer.WriteEndArray();
                }
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }
    }
}