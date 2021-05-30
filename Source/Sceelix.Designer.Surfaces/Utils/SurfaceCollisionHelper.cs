using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalRune.Geometry.Shapes;
using Sceelix.Mathematics.Data;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Extensions;

namespace Sceelix.Designer.Surfaces.Utils
{
    public static class SurfaceCollisionHelper
    {
        public static HeightField CalculateHeightField(this SurfaceEntity surfaceEntity)
        {
            var heightLayer = surfaceEntity.GetLayer<HeightLayer>();
            var origin = surfaceEntity.Origin;
            var numColumns = surfaceEntity.NumColumns;
            var numRows = surfaceEntity.NumRows;
            

            float[] heights = new float[numColumns * numRows];
            for (int i = 0; i < numColumns; i++)
            {
                for (int j = 0; j < numRows; j++)
                {
                    var coordinate = new Coordinate(i, j);
                    heights[i + j * numColumns] = heightLayer?.GetPosition(coordinate).Z ?? surfaceEntity.CalculateFlatPosition(coordinate).Z;
                }
            }

            return new HeightField(origin.X, -origin.Y - +surfaceEntity.Length, surfaceEntity.Width, surfaceEntity.Length, heights, numColumns, numRows);
        }
    }
}
