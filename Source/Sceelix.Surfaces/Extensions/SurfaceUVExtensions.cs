using Sceelix.Mathematics.Data;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Extensions
{
    /// <summary>
    /// Performs UV calculation for surfaces.
    /// </summary>
    public static class SurfaceUVExtensions
    {
        public static Vector2D CalculateBaseUV(this SurfaceEntity surfaceEntity, Coordinate surfaceCoordinate, bool flipU = false, bool flipV = false)
        {
            var u = surfaceCoordinate.X / (float) (surfaceEntity.NumColumns - 1);
            var v = (surfaceEntity.NumRows - 1 - surfaceCoordinate.Y) / (float) (surfaceEntity.NumRows - 1);

            if (flipU)
                u = 1 - u;

            if (flipV)
                v = 1 - v;

            return new Vector2D(u, v);
        }



        public static Vector2D CalculateUV(this SurfaceEntity surfaceEntity, Coordinate surfaceCoordinate)
        {
            var u = surfaceCoordinate.X / (float) (surfaceEntity.NumColumns - 1);
            var v = (surfaceEntity.NumRows - 1 - surfaceCoordinate.Y) / (float) (surfaceEntity.NumRows - 1);

            return new Vector2D(u, v);
        }



        public static Vector2D CalculateUV(this SurfaceEntity surfaceEntity, Coordinate surfaceCoordinate, Vector2D tiling, Vector2D offset, bool flipU = false, bool flipV = false)
        {
            var u = surfaceCoordinate.X / (float) (surfaceEntity.NumColumns - 1);
            var v = (surfaceEntity.NumRows - 1 - surfaceCoordinate.Y) / (float) (surfaceEntity.NumRows - 1);

            if (flipU)
                u = 1 - u;

            if (flipV)
                v = 1 - v;

            return new Vector2D(u * tiling.X + offset.X, v * tiling.Y + offset.Y);
        }
    }
}