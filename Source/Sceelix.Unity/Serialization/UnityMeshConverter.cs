using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sceelix.Mathematics.Data;
using Sceelix.Serialization;
using Sceelix.Unity.Annotations;
using Sceelix.Unity.Data;

namespace Sceelix.Unity.Serialization
{
    [UnityJsonConverter(typeof(UnityMesh))]
    public class UnityMeshConverter : JsonConverter
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
            var mesh = (UnityMesh) value;

            writer.WriteStartObject();

            string reference = "UnityMesh_" + serializer.GetUniqueId(mesh.MeshEntity);
            writer.WritePropertyName("Name");
            writer.WriteValue(reference);

            if (!serializer.HasReference(reference))
            {
                writer.WritePropertyName("Positions");
                writer.WriteStartArray();
                foreach (Vector3D position in mesh.Positions)
                    serializer.Serialize(writer, position);
                writer.WriteEndArray();

                writer.WritePropertyName("Normals");
                writer.WriteStartArray();
                foreach (Vector3D normal in mesh.Normals)
                    serializer.Serialize(writer, normal);
                writer.WriteEndArray();

                writer.WritePropertyName("Colors");
                writer.WriteStartArray();
                foreach (Color color in mesh.Colors)
                    serializer.Serialize(writer, color);
                writer.WriteEndArray();

                writer.WritePropertyName("UVs");
                writer.WriteStartArray();
                foreach (Vector2D uv in mesh.Uvs)
                    serializer.Serialize(writer, uv);
                writer.WriteEndArray();

                writer.WritePropertyName("Tangents");
                writer.WriteStartArray();
                foreach (Vector4D tangent in mesh.Tangents)
                    serializer.Serialize(writer, tangent);
                writer.WriteEndArray();

                writer.WritePropertyName("Triangles");
                writer.WriteStartArray();
                foreach (List<int> list in mesh.SubmeshTriangles)
                    serializer.Serialize(writer, list);
                writer.WriteEndArray();

                serializer.AddObject(reference, mesh);
            }

            writer.WriteEndObject();
        }
    }
}