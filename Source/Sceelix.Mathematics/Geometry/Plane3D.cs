using System;
using System.Collections.Generic;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Geometry
{
    public enum PointToPlaneLocation
    {
        Above,
        OnPlane,
        Below
    }

    /// <summary>
    /// Defines a Plane in 3D Euclidean space.
    /// 
    /// In three-dimensional Euclidean space, we may exploit the following facts that do not hold in higher dimensions:
    /// - Two planes are either parallel or they intersect in a line.
    /// - A line is either parallel to a plane, intersects it at a single point, or is contained in the plane.
    /// - Two lines perpendicular to the same plane must be parallel to each other.
    /// - Two planes perpendicular to the same line must be parallel to each other.
    /// </summary>
    public struct Plane3D
    {
        /// <summary>
        /// A plane can be defined by its normal n = (A,B,C)... 
        /// </summary>
        private readonly Vector3D _normal;

        /// <summary>
        /// ...and any point on the plane P0 = (x0, y0, z0)
        /// </summary>
        private readonly Vector3D _point0;



        /// <summary>
        /// Creates an instance of a 3D Plane from a normal vector and a point.
        /// </summary>
        /// <param name="normal">Normal of the plane. Does not need to be normalized.</param>
        /// <param name="point">Point of the plane.</param>
        public Plane3D(Vector3D normal, Vector3D point)
        {
            _normal = normal.Normalize();
            _point0 = point;
        }



        /// <summary>
        /// Determines if a point lies on the plane.
        /// </summary>
        /// <param name="point">Point to be verified.</param>
        /// <returns>True if it lies on the plane, false otherwise.</returns>
        public bool PointInPlane(Vector3D point)
        {
            //if the point equals 
            if (point == _point0)
                return true;

            //The idea is that a point P with position vector r is in the plane if and only if the vector drawn from P0 to P is perpendicular to n.
            return Math.Abs((_point0 - point).Dot(_normal)) < float.Epsilon;
            //Consider possible floating point errors...
        }



        /// <summary>
        /// Determines the height at a given x,y location in world space.
        /// </summary>
        /// <param name="point">Location in world space where the height is to be determined.</param>
        /// <returns>The height at that point, Infinity if infinite solutions are found or NaN if none is found</returns>
        public float GetHeightAt(Vector2D point)
        {
            var numerator = -_normal.X * (point.X - _point0.X) - _normal.Y * (point.Y - _point0.Y);

            //if the plane is vertical, it either has infinite solutions or none...
            if (Math.Abs(_normal.Z) < float.Epsilon)
            {
                //infinite solutions are found
                if (Math.Abs(numerator) < float.Epsilon)
                    return float.PositiveInfinity;

                //no solution is found
                return float.NaN;
            }

            //solving the equation normal.(point - p0) = 0
            return numerator / _normal.Z + _point0.Z;
        }



        /// <summary>
        /// Determines the minimum distance from a point to this plane
        /// http://paulbourke.net/geometry/pointline/
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public float DistanceToPoint(Vector3D point)
        {
            //should be divided by the size of the normal, but it is 1...
            return (point - _point0).Dot(_normal);
        }



        /// <summary>
        /// Returns 0 if the point is located on the plane, -1 if it below the plane, 1 if above.
        /// The definition of above concerns the direction of the normal.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public PointToPlaneLocation LocationToPlane(Vector3D point)
        {
            float dot = (point - Point0).Dot(_normal);

            if (dot < 0)
                return PointToPlaneLocation.Below;

            return dot > 0 ? PointToPlaneLocation.Above : PointToPlaneLocation.OnPlane;
        }



        public Vector3D Normal => _normal;


        public Vector3D Point0 => _point0;
        //set { _point0 = value; }



        public Plane3D Translate(float translation)
        {
            return new Plane3D(_normal, _point0 + _normal * translation);
        }



        public static Vector3D CalculateIntersection(Plane3D planeX, Plane3D planeY, Plane3D planeZ)
        {
            float denominator = Vector3D.Dot(planeX.Normal, Vector3D.Cross(planeY.Normal, planeZ.Normal));

            //if the denominator is not zero, then the planes DO intersect at one point
            if (Math.Abs(denominator - 0) > float.Epsilon)
            {
                float d1 = planeX.Normal.Dot(planeX.Point0);
                float d2 = planeY.Normal.Dot(planeY.Point0);
                float d3 = planeZ.Normal.Dot(planeZ.Point0);

                Vector3D p = Vector3D.Cross(planeY.Normal, planeZ.Normal) * d1 +
                             Vector3D.Cross(planeZ.Normal, planeX.Normal) * d2 +
                             Vector3D.Cross(planeX.Normal, planeY.Normal) * d3;
                return p / denominator;
            }

            return Vector3D.Zero;

            /*Vector3D N23 = Vector3D.Cross(planeY.Normal, planeZ.Normal);
            Vector3D N31 = Vector3D.Cross(planeZ.Normal, planeX.Normal);
            Vector3D N12 = Vector3D.Cross(planeX.Normal, planeY.Normal);

            double NdN23 = Math.Round(Vector3D.Dot(planeX.Normal, N23), 6);

        if NdN23 != 0.0:
            Vector3D numer = Point(self.k*N23.x, self.k*N23.y, self.k*N23.z) + (self.d2*N31.x, self.d2*N31.y, self.d2*N31.z) + \
                     (self.d3*N12.x, self.d3*N12.y, self.d3*N12.z)

            self.M = Point(numer.x/NdN23, numer.y/NdN23, numer.z/NdN23)
            self.R = self.M.dist(self.p1)
        else:
            self.M = Point(0.0, 0.0, 0.0)
            self.R = 0.0*/
        }



        /// <summary>
        /// Calculates a plane that is "behind" (at most touching) all the indicated points
        /// </summary>
        /// <param name="points"></param>
        /// <param name="direction"></param>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        public static Plane3D GetBackPlane(IList<Vector3D> points, Vector3D direction, out float maxDistance)
        {
            Plane3D plane = new Plane3D(direction, points[0]);
            maxDistance = 0;

            for (int i = 1; i < points.Count; i++)
            {
                float distance = plane.DistanceToPoint(points[i]);
                if (distance < 0)
                {
                    plane = new Plane3D(plane.Normal, points[i]);

                    maxDistance += Math.Abs(distance);
                }

                maxDistance = maxDistance > distance ? maxDistance : distance;
            }

            return plane;
        }



        public static Plane3D GetBackPlane(IList<Vector3D> points, Vector3D direction)
        {
            float maxDistance = 0;
            return GetBackPlane(points, direction, out maxDistance);
        }



        /// <summary>
        /// Indicates if this plane is overlapping a given plane (yet their directions may be opposite).
        /// </summary>
        /// <param name="plane"></param>
        /// <returns></returns>
        public bool Coincident(Plane3D plane)
        {
            //they are 
            return Normal.IsCollinear(plane.Normal) && plane.PointInPlane(plane.Point0);
        }



        /// <summary>
        /// Indicates if this plane is overlapping a given plane (and their directions are the same).
        /// </summary>
        /// <param name="plane"></param>
        /// <returns></returns>
        public bool CoincidentAndWithSameDirection(Plane3D plane)
        {
            //they are 
            return Normal.Equals(plane.Normal) && plane.PointInPlane(plane.Point0);
        }



        /// <summary>
        /// This is an alternative method to distinguish 2 planes. If they have the same normal, they can be coincident.
        /// To verify this is one step (for the sake of grouping)
        /// </summary>
        /// <returns></returns>
        public Vector3D NormalDistanceToZero()
        {
            return _normal * DistanceToPoint(Vector3D.Zero);
        }



        public Vector3D RoundedNormalDistanceToZero()
        {
            return _normal * DistanceToPoint(Vector3D.Zero);
        }



        public override string ToString()
        {
            return string.Format("Distance: {0}, Normal: {1}", DistanceToPoint(Vector3D.Zero), _normal);
        }
    }
}