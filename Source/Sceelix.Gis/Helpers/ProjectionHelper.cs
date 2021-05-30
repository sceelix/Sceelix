using DotSpatial.Projections;
using Sceelix.Gis.Data;
using Sceelix.Mathematics.Data;

namespace Sceelix.Gis.Helpers
{
    public class ProjectionHelper
    {
        public static Vector3D ProjectGeoToMercator(GeoLocation geolocation)
        {
            var sourceProjection = KnownCoordinateSystems.Geographic.World.WGS1984;
            var targetProjection = KnownCoordinateSystems.Projected.World.WebMercator;

            double[] xy = new double[2];
            xy[0] = geolocation.Longitude;
            xy[1] = geolocation.Latitude;
            //An array for the z coordinate
            double[] z = new double[1];

            Reproject.ReprojectPoints(xy, z, sourceProjection, targetProjection, 0, 1);

            return new Vector3D((float) xy[0], (float) xy[1], 0);
        }
    }
}