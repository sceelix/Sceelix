using System;
using System.Linq;
using Newtonsoft.Json;
using Sceelix.Core.Environments;
using Sceelix.Extensions;
using Sceelix.Helpers;
using Sceelix.Logging;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Helpers;
using Sceelix.Serialization;
using Sceelix.Surfaces.Data;
using Sceelix.Unity.Annotations;
using Sceelix.Unity.Data;

namespace Sceelix.Unity.Serialization
{
    [UnityJsonConverter(typeof(SurfaceComponent))]
    public class SurfaceComponentConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            //not used
            throw new NotImplementedException();
        }



        private float[,] GetSurfaceHeights(SurfaceEntity surfaceEntity)
        {
            //unity has its y inverted, so...
            float[,] invertedHeights = new float[surfaceEntity.NumRows, surfaceEntity.NumColumns];

            var numRows = surfaceEntity.NumRows;
            var numColumns = surfaceEntity.NumColumns;
            var height = surfaceEntity.MaximumZ;

            var heightLayer = surfaceEntity.GetLayer<HeightLayer>();

            ParallelHelper.For(0, numColumns, i =>
            {
                for (int j = 0; j < numRows; j++) invertedHeights[numRows - j - 1, i] = (heightLayer != null ? heightLayer.GetGenericValue(new Coordinate(i, j)) : 0) / height; //numRows - 1 - j
            });

            return invertedHeights;
        }



        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //not used
            throw new NotImplementedException();
        }



        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var surfaceComponent = (SurfaceComponent) value;
            var surface = surfaceComponent.SurfaceEntity;
            var procedureEnvironment = serializer.GetService<IProcedureEnvironment>();
            var logger = procedureEnvironment.GetService<ILogger>();

            writer.WriteStartObject();

            writer.WritePropertyName("ComponentType");
            writer.WriteValue("Terrain");

            writer.WritePropertyName("Resolution");
            writer.WriteValue(surface.NumRows);

            writer.WritePropertyName("Size");
            serializer.Serialize(writer, new Vector3D(surface.Width, Math.Max(1, surface.MaximumZ), surface.Length));

            var surfaceHeights = GetSurfaceHeights(surface);
            writer.WritePropertyName("HeightsResolution");
            serializer.Serialize(writer, new Vector2D(surfaceHeights.GetLength(0), surfaceHeights.GetLength(1)));

            writer.WritePropertyName("Heights");
            serializer.Serialize(writer, surfaceHeights.ToByteArray());

            //warn the user of Unity's limitations
            if (!MathHelper.IsPowerOfTwo(surface.NumColumns - 1) || !MathHelper.IsPowerOfTwo(surface.NumRows - 1)) logger.Log("Your surface's resolution is not a power of 2, which is required by Unity. You may encounter unexpected results.", LogType.Warning);
            if (surface.NumColumns - 1 > 4096 || surface.NumRows - 1 > 4096) logger.Log("Your surface's resolution is higher than 4069, which the maximum allowed by Unity. You may encounter unexpected results.", LogType.Warning);
            if (surface.NumColumns != surface.NumRows) logger.Log("Your surface's is not square, which is required by Unity. You may encounter unexpected results.", LogType.Warning);

            //serialize the material
            serializer.SetObject("Surface", surface);

            writer.WritePropertyName("Material");
            serializer.Serialize(writer, surface.Material);

            writer.WritePropertyName("TreeInstances");
            writer.WriteStartArray();
            foreach (var instancesByPrefab in surfaceComponent.TreeInstances.GroupBy(x => x.Prefab))
            {
                writer.WriteStartObject();

                writer.WritePropertyName("Prefab");
                writer.WriteValue(instancesByPrefab.Key);

                var firstOrDefault = instancesByPrefab.First();

                writer.WritePropertyName("BendFactor");
                writer.WriteValue(firstOrDefault.BendFactor);

                writer.WritePropertyName("Transforms");
                writer.WriteStartArray();

                foreach (var treeInstance in instancesByPrefab)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Position");
                    serializer.Serialize(writer, treeInstance.Scope.Translation.FlipYZ() / new Vector3D(surface.Width, surface.MaximumZ, surface.Length));

                    writer.WritePropertyName("Rotation");
                    serializer.Serialize(writer, treeInstance.Rotation); //needs pitch, yaw, roll  //MathHelper.ToDegrees(treeInstance.Scope.XAxis.AngleTo(Vector3D.XVector)

                    writer.WritePropertyName("Scale");
                    serializer.Serialize(writer, treeInstance.Scale);

                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
            }

            writer.WriteEndArray();

            var unityEntity = (UnityEntity) serializer.GetObject("UnityEntity");

            EntityConverter.WriteUnityAttributeKeys(writer, unityEntity, SurfaceEntityConverter.SurfaceAttributeKeys);

            writer.WriteEndObject();
        }
    }
}