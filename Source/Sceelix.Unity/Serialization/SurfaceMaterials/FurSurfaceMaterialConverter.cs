using System;
using Newtonsoft.Json;
using Sceelix.Core.Environments;
using Sceelix.Logging;
using Sceelix.Meshes.Materials;
using Sceelix.Serialization;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Materials;
using Sceelix.Unity.Annotations;

namespace Sceelix.Unity.Serialization.SurfaceMaterials
{
    [UnityJsonConverter(typeof(FurSurfaceMaterial))]
    public class FurSurfaceMaterialConverter : SurfaceMaterialConverter
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
            var material = (FurSurfaceMaterial) value;
            var surface = (SurfaceEntity) serializer.GetObject("Surface");

            var procedureEnvironment = serializer.GetService<IProcedureEnvironment>();

            writer.WriteStartObject();
            {
                SerializeSplatmap(writer, serializer, GetSingleSplatmap(surface));

                writer.WritePropertyName("Layers");
                writer.WriteStartArray();
                {
                    writer.WriteStartObject();

                    writer.WritePropertyName("TileSize");
                    serializer.Serialize(writer, material.UVTiling);

                    writer.WritePropertyName("Texture");
                    serializer.Serialize(writer, new ResourcePath(material.Texture));

                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }

            writer.WriteEndObject();

            procedureEnvironment.GetService<ILogger>().Log("A fur material does not exist in Unity, so only an incomplete material conversion can be performed.", LogType.Warning);
        }
    }
}