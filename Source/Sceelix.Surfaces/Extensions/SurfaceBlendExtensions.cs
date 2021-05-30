using System.Collections.Generic;
using System.Linq;
using Sceelix.Helpers;
using Sceelix.Mathematics.Data;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Extensions
{
    public static class SurfaceBlendExtensions
    {
        public static IEnumerable<BlendLayer> ToBlendSurfaces(Color[,] colors, int baseTextureIndex = 0)
        {
            var columns = colors.GetLength(0);
            var rows = colors.GetLength(1);

            var blendValues = new float[4][,];
            for (int i = 0; i < 4; i++)
                blendValues[i] = new float[columns, rows];

            ParallelHelper.For(0, columns, i =>
            {
                for (int j = 0; j < rows; j++)
                {
                    var systemColor = colors[i, j];

                    blendValues[0][i, j] = systemColor.R / 255f;
                    blendValues[1][i, j] = systemColor.G / 255f;
                    blendValues[2][i, j] = systemColor.B / 255f;
                    blendValues[3][i, j] = systemColor.A / 255f;
                }
            });

            for (int i = 0; i < 4; i++)
                yield return new BlendLayer(blendValues[i], baseTextureIndex + i);
        }



        /// <summary>
        /// Builds a color array from a set of blend layers.
        /// </summary>
        /// <remarks>The blend layers are ordered by their texture id before the array is extracted.</remarks>
        /// <param name="blendLayers">The blend layers.</param>
        /// <param name="numColumns">The number of columns of the returned array.</param>
        /// <param name="numRows">The number of rows of the returned array.</param>
        /// <returns></returns>
        /*public static Color[,] ToColorArray(this IEnumerable<BlendLayer> blendLayers, int numColumns, int numRows)
        {
            //var values = new float[numColumns, numRows][];
            var colors = new Color[numColumns, numRows];

            var blendLayerList = blendLayers.OrderBy(x => x.TextureIndex).Take(4).ToList();

            for (int i = 0; i < blendLayerList.Count; i++)
            {
                var currentLayer = blendLayerList[i];

                Parallel.For(0, numColumns, (x) =>
                {
                    for (int y = 0; y < numRows; y++)
                    {
                        var array = colors[x, y].ToFloatArray();
                        array[i] = currentLayer.GetValue(x, y);

                        colors[x,y] = new Color(array);
                    }
                });
            }

            return colors;
        }*/
        public static Color[,] ToColorArray(this IEnumerable<BlendLayer> blendLayers, int numColumns, int numRows, int offset = 0)
        {
            var colors = new Color[numColumns, numRows];

            var blendLayerList = blendLayers.ToList();

            for (int i = 0; i < blendLayerList.Count; i++)
            {
                var currentLayer = blendLayerList[i];
                var currentTextureIndex = currentLayer.TextureIndex;

                //we only accept texture indices between 0 and 3
                if (!(currentTextureIndex >= offset && currentTextureIndex < offset + 4))
                    continue;

                ParallelHelper.For(0, numColumns, x =>
                {
                    for (int y = 0; y < numRows; y++)
                    {
                        var array = colors[x, y].ToFloatArray();
                        array[currentTextureIndex - offset] = currentLayer.GetGenericValue(new Coordinate(x, y));

                        colors[x, y] = new Color(array);
                    }
                });
            }

            return colors;
        }



        /*public static void AddMissingLayers(SurfaceEntity surfaceEntity)
        {
            var blendLayers = surfaceEntity.Layers.OfType<BlendLayer>().ToList();
            if (blendLayers.Any())
                return;

            var maxTextureIndex = blendLayers.Max(x => x.TextureIndex);
            for (int i = 0; i < maxTextureIndex; i++)
            {
                
            }
        }*/
    }
}