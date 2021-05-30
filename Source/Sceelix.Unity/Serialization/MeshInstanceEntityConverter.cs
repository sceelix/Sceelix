using System;
using Newtonsoft.Json;
using Sceelix.Meshes.Data;
using Sceelix.Serialization;
using Sceelix.Unity.Annotations;
using Sceelix.Unity.Data;

namespace Sceelix.Unity.Serialization
{
    [UnityJsonConverter(typeof(MeshInstanceEntity))]
    public class MeshInstanceEntityConverter : JsonConverter
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
            var meshEntityInstance = (MeshInstanceEntity) value;
            var meshEntity = meshEntityInstance.MeshEntity;

            //we have to avoid sending the same mesh entity several times, which can occur if we are sending mesh instances
            //we use the hash code of the meshentity to identify the object
            //and only create the objects
            string reference = serializer.GetService<SequentialIdGenerator>().GetId(meshEntity);

            //var reference = "UnityMeshWithMaterials_" + serializer.GetReference(meshEntity);
            var meshWithMaterials = serializer.GetObject(reference) as UnityMeshWithMaterials;
            //String reference = meshEntity.GetHashCode().ToString();
            //var meshWithMaterials = serializer.GetObject<UnityMeshWithMaterials>(reference);
            if (meshWithMaterials == null)
            {
                meshWithMaterials = new UnityMeshWithMaterials(meshEntity);
                serializer.AddObject(reference, meshWithMaterials);
            }

            writer.WriteStartObject();
            {
                writer.WritePropertyName("EntityType");
                writer.WriteValue("MeshInstanceEntity");

                EntityConverter.SerializeCommonUnityAttributes(writer, meshEntityInstance, serializer);

                writer.WritePropertyName("Position");
                serializer.Serialize(writer, meshEntityInstance.BoxScope.Translation.FlipYZ());

                writer.WritePropertyName("ForwardVector");
                serializer.Serialize(writer, meshEntityInstance.BoxScope.YAxis.FlipYZ());

                writer.WritePropertyName("UpVector");
                serializer.Serialize(writer, meshEntityInstance.BoxScope.ZAxis.FlipYZ());

                var scale = meshEntityInstance.BoxScope.Sizes * meshEntityInstance.RelativeScale;
                writer.WritePropertyName("Scale");
                serializer.Serialize(writer, scale.FlipYZ().ReplaceValue(0, 1));

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