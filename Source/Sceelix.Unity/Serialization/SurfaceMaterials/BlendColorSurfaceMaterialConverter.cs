using System;
using Newtonsoft.Json;
using Sceelix.Extensions;
using Sceelix.Mathematics.Data;
using Sceelix.Serialization;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Materials;
using Sceelix.Unity.Annotations;
using Color = System.Drawing.Color;

namespace Sceelix.Unity.Serialization.SurfaceMaterials
{
    [UnityJsonConverter(typeof(BlendColorSurfaceMaterial))]
    public class BlendColorSurfaceMaterialConverter : MultiTextureSurfaceMaterialConverter
    {
        private static readonly ResourceContent[] ResourceContents =
        {
            new ResourceContent("RedColor", BitmapExtension.CreateColorBitmap(Color.Red).ImageToByte()),
            new ResourceContent("GreenColor", BitmapExtension.CreateColorBitmap(Color.FromArgb(255, 0, 255, 0)).ImageToByte()),
            new ResourceContent("BlueColor", BitmapExtension.CreateColorBitmap(Color.Blue).ImageToByte()),
            new ResourceContent("AlphaColor", BitmapExtension.CreateColorBitmap(Color.FromArgb(255, 0, 0, 0)).ImageToByte())
        };



        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }



        /*private float[,,] GetMultiSplatmap(SurfaceEntity surfaceEntity, int limit)
        {
            var blendLayers = surfaceEntity.GetLayers<BlendLayer>().ToList();
            if (blendLayers.Count > 0)
            {
                var numRows = surfaceEntity.NumRows;
                
                var blendLayerCount = Math.Min(blendLayers.Count, limit);

                float[,,] splatmapData = new float[surfaceEntity.NumRows, surfaceEntity.NumColumns, blendLayerCount];
                Parallel.For(0, surfaceEntity.NumColumns, i =>
                {
                    for (int j = 0; j < numRows; j++)
                    {
                        for (int k = 0; k < blendLayerCount; k++)
                        {
                            splatmapData[numRows - j - 1, i, k] = blendLayers[k].GetValue(i, j);
                        }

                    }
                });

                return splatmapData;
            }

            return GetSingleSplatmap(surfaceEntity);
        }*/



        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }



        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var material = (BlendColorSurfaceMaterial) value;
            var surface = (SurfaceEntity) serializer.GetObject("Surface");

            writer.WriteStartObject();
            {
                var splatmap = GetMultiSplatmap(surface, 4);
                var splatmapLayerCount = splatmap.GetLength(2);

                SerializeSplatmap(writer, serializer, splatmap);

                writer.WritePropertyName("Layers");
                writer.WriteStartArray();
                {
                    for (int i = 0; i < splatmapLayerCount; i++)
                    {
                        writer.WriteStartObject();

                        writer.WritePropertyName("TileSize");
                        serializer.Serialize(writer, new Vector2D(1, 1));

                        writer.WritePropertyName("Texture");
                        serializer.Serialize(writer, ResourceContents[i]);

                        writer.WriteEndObject();
                    }
                }
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }
}