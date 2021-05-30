using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sceelix.Extensions;
using Sceelix.Paths.Data;
using Sceelix.Unity.Annotations;

namespace Sceelix.Unity.Serialization
{
    [UnityJsonConverter(typeof(PathEntity))]
    public class PathEntityConverter : JsonConverter
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
            var pathEntity = (PathEntity) value;

            writer.WriteStartObject();
            {
                writer.WritePropertyName("EntityType");
                writer.WriteValue("PathEntity");

                EntityConverter.SerializeCommonUnityAttributes(writer, pathEntity, serializer);

                writer.WritePropertyName("PathEdges");
                writer.WriteStartArray();

                Dictionary<PathVertex, int> vertexDictionary = new Dictionary<PathVertex, int>();
                foreach (var pathEdge in pathEntity.Edges)
                {
                    var sourceIndex = vertexDictionary.GetOrCompute(pathEdge.Source, () => vertexDictionary.Count);
                    var targetIndex = vertexDictionary.GetOrCompute(pathEdge.Target, () => vertexDictionary.Count);

                    writer.WriteStartObject();
                    {
                        writer.WritePropertyName("Source");
                        writer.WriteValue(sourceIndex);

                        writer.WritePropertyName("Target");
                        writer.WriteValue(targetIndex);
                    }
                    writer.WriteEndObject();
                }

                writer.WriteEndArray();


                writer.WritePropertyName("PathVertices");
                writer.WriteStartArray();

                PathVertex[] vertexArray = new PathVertex[vertexDictionary.Count];
                foreach (var entry in vertexDictionary)
                    vertexArray[entry.Value] = entry.Key;

                foreach (var pathVertex in vertexArray)
                    serializer.Serialize(writer, pathVertex.Position.FlipYZ());

                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }
    }
}