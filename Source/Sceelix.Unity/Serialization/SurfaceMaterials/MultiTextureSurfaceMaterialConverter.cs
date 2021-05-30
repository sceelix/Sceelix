using System;
using System.Linq;
using Newtonsoft.Json;
using Sceelix.Helpers;
using Sceelix.Mathematics.Data;
using Sceelix.Meshes.Materials;
using Sceelix.Serialization;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Materials;
using Sceelix.Unity.Annotations;

namespace Sceelix.Unity.Serialization.SurfaceMaterials
{
    [UnityJsonConverter(typeof(MultiTextureSurfaceMaterial))]
    public class MultiTextureSurfaceMaterialConverter : SurfaceMaterialConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }



        protected float[,,] GetMultiSplatmap(SurfaceEntity surfaceEntity, int limit)
        {
            var blendLayers = surfaceEntity.GetLayers<BlendLayer>().ToList();
            if (blendLayers.Count > 0)
            {
                var numRows = surfaceEntity.NumRows;
                var blendLayerCount = Math.Min(blendLayers.Count, limit);

                float[,,] splatmapData = new float[surfaceEntity.NumRows, surfaceEntity.NumColumns, blendLayerCount];
                ParallelHelper.For(0, surfaceEntity.NumColumns, i =>
                {
                    for (int j = 0; j < numRows; j++)
                    for (int k = 0; k < blendLayerCount; k++)
                        splatmapData[numRows - j - 1, i, k] = blendLayers[k].GetGenericValue(new Coordinate(i, j));
                });

                return splatmapData;
            }

            return GetSingleSplatmap(surfaceEntity);
        }



        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }



        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var material = (MultiTextureSurfaceMaterial) value;
            var surface = (SurfaceEntity) serializer.GetObject("Surface");

            writer.WriteStartObject();
            {
                SerializeSplatmap(writer, serializer, GetMultiSplatmap(surface, material.TextureSetups.Length));

                writer.WritePropertyName("Layers");
                writer.WriteStartArray();

                //do not send more textures than the available layers, nor send
                foreach (TextureSetup textureSetup in material.TextureSetups)
                {
                    writer.WriteStartObject();

                    if (!string.IsNullOrEmpty(textureSetup.DiffuseMapPath))
                    {
                        writer.WritePropertyName("DiffuseMap");
                        serializer.Serialize(writer, new ResourcePath(textureSetup.DiffuseMapPath));
                    }


                    /*if (!String.IsNullOrEmpty(textureSetup.Path))
                        
                    else
                    {
                        writer.WriteStartObject();//check what this is for
                        writer.WritePropertyName("Name");
                        writer.WriteValue(String.Empty);
                        writer.WriteEndObject();
                    }*/

                    if (!string.IsNullOrEmpty(textureSetup.NormalMapPath))
                    {
                        writer.WritePropertyName("NormalMap");
                        serializer.Serialize(writer, new ResourcePath(textureSetup.NormalMapPath));

                        writer.WritePropertyName("NormalScale");
                        writer.WriteValue(textureSetup.NormalScale);
                    }

                    writer.WritePropertyName("TileSize");
                    serializer.Serialize(writer, textureSetup.UVTiling);

                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }
}