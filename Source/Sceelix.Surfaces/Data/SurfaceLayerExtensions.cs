using System;
using Sceelix.Helpers;
using Sceelix.Mathematics.Data;

namespace Sceelix.Surfaces.Data
{
    public static class SurfaceLayerExtensions
    {
        public static SurfaceLayer ChangeResolution(this SurfaceLayer surfaceLayer, int numColumns, int numRows)
        {
            var newLayer = surfaceLayer.CreateEmpty(numColumns, numRows);
            ParallelHelper.For(0, numColumns, x =>
            {
                for (int y = 0; y < numRows; y++)
                {
                    double fractionX = x / (double) numColumns;
                    double fractionY = y / (double) numRows;

                    var otherX = (int) Math.Round(fractionX * surfaceLayer.Surface.NumColumns);
                    var otherY = (int) Math.Round(fractionY * surfaceLayer.Surface.NumRows);
                    var otherValue = surfaceLayer.GetValue(new Coordinate(otherX, otherY));

                    newLayer.SetValue(new Coordinate(x, y), otherValue);
                }
            });

            return newLayer;
        }



        public static SurfaceLayer Fill(this SurfaceLayer surfaceLayer, object value)
        {
            int numColumns = surfaceLayer.NumColumns;
            int numRows = surfaceLayer.NumRows;
            ParallelHelper.For(0, numColumns, x =>
            {
                for (int y = 0; y < numRows; y++) surfaceLayer.SetValue(new Coordinate(x, y), value);
            });

            return surfaceLayer;
        }
    }
}