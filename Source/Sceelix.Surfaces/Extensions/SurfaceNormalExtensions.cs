using Sceelix.Mathematics.Data;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Extensions
{
    public static class SurfaceNormalExtensions
    {
        /// <summary>
        /// Calculates the normal from a heightlayer and surface coordinates.
        /// </summary>
        /// <param name="heightLayer">The height layer. If null, the default (0,0,1) vector will be returned.</param>
        /// <param name="surfaceColumn">The surface column.</param>
        /// <param name="surfaceRow">The surface row.</param>
        /// <returns></returns>
        public static Vector3D CalculateNormal(this HeightLayer heightLayer, Coordinate surfaceCoordinate)
        {
            if (heightLayer == null)
                return Vector3D.ZVector;

            Vector3D[] directions = new Vector3D[4];
            Vector3D centralPosition = heightLayer.GetPosition(surfaceCoordinate);

            directions[0] = heightLayer.GetPosition(new Coordinate(surfaceCoordinate.X, surfaceCoordinate.Y - 1), SampleMethod.Clamp) - centralPosition;
            directions[1] = heightLayer.GetPosition(new Coordinate(surfaceCoordinate.X - 1, surfaceCoordinate.Y), SampleMethod.Clamp) - centralPosition;
            directions[2] = heightLayer.GetPosition(new Coordinate(surfaceCoordinate.X, surfaceCoordinate.Y + 1), SampleMethod.Clamp) - centralPosition;
            directions[3] = heightLayer.GetPosition(new Coordinate(surfaceCoordinate.X + 1, surfaceCoordinate.Y), SampleMethod.Clamp) - centralPosition;

            Vector3D normal = Vector3D.Zero;
            for (int i = 0; i < 4; i++)
            {
                Vector3D direction1 = directions[i];
                Vector3D direction2 = i + 1 > 3 ? directions[0] : directions[i + 1];

                normal += Vector3D.Cross(direction1, direction2);
            }

            return normal.Normalize();
        }
    }
}