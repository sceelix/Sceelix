using System;
using Newtonsoft.Json;
using Sceelix.Meshes.Materials;
using Sceelix.Serialization;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Materials;
using Sceelix.Unity.Annotations;

namespace Sceelix.Unity.Serialization.SurfaceMaterials
{
    [UnityJsonConverter(typeof(SingleTextureSurfaceMaterial))]
    public class SingleTextureSurfaceMaterialConverter : SurfaceMaterialConverter
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
            var material = (SingleTextureSurfaceMaterial) value;
            var surface = (SurfaceEntity) serializer.GetObject("Surface");

            writer.WriteStartObject();
            {
                SerializeSplatmap(writer, serializer, GetSingleSplatmap(surface));

                writer.WritePropertyName("Layers");
                writer.WriteStartArray();
                {
                    writer.WriteStartObject();

                    writer.WritePropertyName("TileSize");
                    serializer.Serialize(writer, material.UVTiling);

                    writer.WritePropertyName("DiffuseMap");
                    serializer.Serialize(writer, new ResourcePath(material.Texture));

                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }
}