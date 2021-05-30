using System;
using Newtonsoft.Json;
using Sceelix.Extensions;
using Sceelix.Helpers;
using Sceelix.Mathematics.Data;
using Sceelix.Serialization;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Materials;
using Sceelix.Unity.Annotations;
using Color = System.Drawing.Color;

namespace Sceelix.Unity.Serialization.SurfaceMaterials
{
    [UnityJsonConverter(typeof(SurfaceMaterial))]
    public class SurfaceMaterialConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }



        protected float[,,] GetSingleSplatmap(SurfaceEntity surfaceEntity)
        {
            float[,,] splatmapData = new float[surfaceEntity.NumRows, surfaceEntity.NumColumns, 1];

            var numRows = surfaceEntity.NumRows;

            ParallelHelper.For(0, surfaceEntity.NumColumns, i =>
            {
                for (int j = 0; j < numRows; j++) splatmapData[numRows - j - 1, i, 0] = 1;
            });

            return splatmapData;
        }



        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }



        protected void SerializeSplatmap(JsonWriter writer, JsonSerializer serializer, float[,,] splatmap)
        {
            writer.WritePropertyName("SplatmapSize");
            serializer.Serialize(writer, new Vector3D(splatmap.GetLength(0), splatmap.GetLength(1), splatmap.GetLength(2)));

            writer.WritePropertyName("Splatmap");
            serializer.Serialize(writer, splatmap.ToByteArray());
        }



        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var material = (SurfaceMaterial) value;
            var surface = (SurfaceEntity) serializer.GetObject("Surface");

            writer.WriteStartObject();
            {
                SerializeSplatmap(writer, serializer, GetSingleSplatmap(surface));

                writer.WritePropertyName("Layers");
                writer.WriteStartArray();
                {
                    writer.WriteStartObject();

                    writer.WritePropertyName("TileSize");
                    serializer.Serialize(writer, new Vector2D(1, 1));

                    writer.WritePropertyName("Texture");
                    serializer.Serialize(writer, new ResourceContent("WhiteColor", BitmapExtension.CreateColorBitmap(Color.White).ImageToByte()));

                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }
}