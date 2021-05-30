using System;
using Newtonsoft.Json;
using Sceelix.Core.Attributes;
using Sceelix.Core.Environments;
using Sceelix.Core.Extensions;
using Sceelix.Extensions;
using Sceelix.Helpers;
using Sceelix.Logging;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Helpers;
using Sceelix.Serialization;
using Sceelix.Surfaces.Data;
using Sceelix.Unity.Annotations;
using Sceelix.Unity.Attributes;

namespace Sceelix.Unity.Serialization
{
    [UnityJsonConverter(typeof(SurfaceEntity))]
    public class SurfaceEntityConverter : JsonConverter
    {
        //Dictionary<Type, ISurfaceMaterialConverter> _surfaceMaterialConverters = AttributeReader.OfTypeKeyAttribute<TypeKeyAttribute>().GetInstancesOfType<ISurfaceMaterialConverter>();

        public static AttributeKey[] SurfaceAttributeKeys =
        {
            new GlobalAttributeKey("PixelError"),
            new GlobalAttributeKey("BillboardStart")
        };


        public GlobalAttributeKey ComponentsAttribute = new GlobalAttributeKey("Components", new UnityMeta());



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
            var height = surfaceEntity.Height;
            var maxHeight = surfaceEntity.MaximumZ;
            var heightLayer = surfaceEntity.GetLayer<HeightLayer>();

            var index = surfaceEntity.GetGlobalAttribute("Index");

            ParallelHelper.For(0, numColumns, i =>
            {
                for (int j = 0; j < numRows; j++) invertedHeights[numRows - j - 1, i] = (heightLayer != null ? heightLayer.GetGenericValue(new Coordinate(i, j)) : 0) / maxHeight; //numRows - 1 - j
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
            var surfaceEntity = (SurfaceEntity) value;
            var procedureEnvironment = serializer.GetService<IProcedureEnvironment>();
            var logger = procedureEnvironment.GetService<ILogger>();

            writer.WriteStartObject();

            writer.WritePropertyName("EntityType");
            writer.WriteValue("SurfaceEntity");

            EntityConverter.SerializeCommonUnityAttributes(writer, surfaceEntity, serializer);

            writer.WritePropertyName("Position");
            serializer.Serialize(writer, surfaceEntity.Origin.ToVector3D().FlipYZ());

            writer.WritePropertyName("Resolution");
            writer.WriteValue(surfaceEntity.NumRows);

            writer.WritePropertyName("Size");
            serializer.Serialize(writer, new Vector3D(surfaceEntity.Width, Math.Max(1, surfaceEntity.MaximumZ), surfaceEntity.Length));

            var surfaceHeights = GetSurfaceHeights(surfaceEntity);
            writer.WritePropertyName("HeightsResolution");
            serializer.Serialize(writer, new Vector2D(surfaceHeights.GetLength(0), surfaceHeights.GetLength(1)));

            writer.WritePropertyName("Heights");
            serializer.Serialize(writer, surfaceHeights.ToByteArray());

            //warn the user of Unity's limitations
            if (!MathHelper.IsPowerOfTwo(surfaceEntity.NumColumns - 1) || !MathHelper.IsPowerOfTwo(surfaceEntity.NumRows - 1)) logger.Log("Your surface's resolution is not a power of 2, which is required by Unity. You may encounter unexpected results.", LogType.Warning);
            if (surfaceEntity.NumColumns - 1 > 4096 || surfaceEntity.NumRows - 1 > 4096) logger.Log("Your surface's resolution is higher than 4069, which the maximum allowed by Unity. You may encounter unexpected results.", LogType.Warning);
            if (surfaceEntity.NumColumns != surfaceEntity.NumRows) logger.Log("Your surface's is not square, which is required by Unity. You may encounter unexpected results.", LogType.Warning);

            //serialize the material
            serializer.SetObject("Surface", surfaceEntity);

            writer.WritePropertyName("Material");
            serializer.Serialize(writer, surfaceEntity.Material);


            //var unityComponents = surfaceEntity.GetAttribute<List<UnityComponent>>(ComponentsAttribute);
            //writer.WritePropertyName("Components");

            //serialize, like others
            EntityConverter.WriteUnityAttributeKeys(writer, surfaceEntity, SurfaceAttributeKeys);


            writer.WriteEndObject();
        }
    }
}