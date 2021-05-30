using System;
using System.Linq;
using Newtonsoft.Json;
using Sceelix.Extensions;
using Sceelix.Mathematics.Data;
using Sceelix.Serialization;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Materials;
using Sceelix.Unity.Annotations;

namespace Sceelix.Unity.Serialization.SurfaceMaterials
{
    [UnityJsonConverter(typeof(ColorSurfaceMaterial))]
    internal class ColorSurfaceMaterialConverter : SurfaceMaterialConverter
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
            var material = (ColorSurfaceMaterial) value;
            var surface = (SurfaceEntity) serializer.GetObject("Surface");

            writer.WriteStartObject();
            {
                writer.WritePropertyName("Splatmap");
                serializer.Serialize(writer, GetSingleSplatmap(surface));

                writer.WritePropertyName("Layers");
                writer.WriteStartArray();
                {
                    writer.WriteStartObject();

                    writer.WritePropertyName("TileSize");
                    serializer.Serialize(writer, new Vector2D(1, 1));

                    writer.WritePropertyName("DiffuseMap");
                    var colorName = string.Join("-", material.DefaultColor.ToArray().Select(x => x.ToString()));
                    serializer.Serialize(writer, new ResourceContent(colorName, BitmapExtension.CreateColorBitmap(material.DefaultColor).ImageToByte()));

                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }
}