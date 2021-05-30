using System;
using System.Linq;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Geometry
{
    /// <summary>
    /// Defines an infinite Line in 3D Euclidean space.
    /// A line will be defined by a point P0 and a direction D, so that a point on the line has an equation P = P0 + m * D
    /// 
    /// Explanation at: http://paulbourke.net/geometry/lineline3d/
    /// </summary>
    public struct Line3D
    {
        public Line3D(Vector3D direction, Vector3D point0)
        {
            Direction = direction;
            Point0 = point0;
        }



        /// <summary>
        /// Finds the shortest line segment between two infinite lines
        /// Algorithm from: http://paulbourke.net/geometry/pointlineplane/
        /// </summary>
        /// <param name="line">The line to check for the distance</param>
        /// <returns></returns>
        public LineSegment3D? ShortestLineBetweenTwoLines(Line3D line)
        {
            Vector3D p1 = Point0;
            Vector3D p3 = line.Point0;

            Vector3D p21 = Direction;
            Vector3D p43 = line.Direction;
            Vector3D p13 = p1 - p3;

            if (Direction.Length < float.Epsilon || line.Direction.Length < float.Epsilon)
                return null;

            double d1343 = p13.Dot(p43);
            double d4321 = p43.Dot(p21);
            double d1321 = p13.X * (double) p21.X + (double) p13.Y * p21.Y + (double) p13.Z * p21.Z;
            double d4343 = p43.X * (double) p43.X + (double) p43.Y * p43.Y + (double) p43.Z * p43.Z;
            double d2121 = p21.X * (double) p21.X + (double) p21.Y * p21.Y + (double) p21.Z * p21.Z;

            double denom = d2121 * d4343 - d4321 * d4321;
            if (Math.Abs(denom) < float.Epsilon) return null;
            double numer = d1343 * d4321 - d1321 * d4343;

            double mua = numer / denom;
            double mub = (d1343 + d4321 * mua) / d4343;

            return new LineSegment3D(this[(float) mua], line[(float) mub]);
        }



        /// <summary>
        /// Finds the shortest line segment between two infinite lines
        /// Algorithm from: http://paulbourke.net/geometry/pointlineplane/
        /// </summary>
        /// <param name="line">The line to check for the distance</param>
        /// <returns></returns>
        public float? ClosestRelativeHit(Line3D line)
        {
            Vector3D p1 = Point0;
            Vector3D p3 = line.Point0;

            Vector3D p21 = Direction;
            Vector3D p43 = line.Direction;
            Vector3D p13 = p1 - p3;

            if (Direction.Length < float.Epsilon || line.Direction.Length < float.Epsilon)
                return null;

            double d1343 = p13.Dot(p43);
            double d4321 = p43.Dot(p21);
            double d1321 = p13.X * (double) p21.X + (double) p13.Y * p21.Y + (double) p13.Z * p21.Z;
            double d4343 = p43.X * (double) p43.X + (double) p43.Y * p43.Y + (double) p43.Z * p43.Z;
            double d2121 = p21.X * (double) p21.X + (double) p21.Y * p21.Y + (double) p21.Z * p21.Z;

            double denom = d2121 * d4343 - d4321 * d4321;
            if (Math.Abs(denom) < float.Epsilon) return null;
            double numer = d1343 * d4321 - d1321 * d4343;

            double mua = numer / denom;
            double mub = (d1343 + d4321 * mua) / d4343;

            return (float) mua;
        }



        /// <summary>
        /// Determines the point at the location m on the line, according to the line equation P = P0 + m * D
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public Vector3D this[float m] => Direction * m + Point0;



        public enum LineIntersection
        {
            CollinearOverlapping,
            CollinearDisjoint
        }

        /*public float? Intersects(Line3D plane, bool includeEnd, bool include)
        {

    
    
        }*/



        private static bool LinesIntersect(Vector3D A, Vector3D B, Vector3D C, Vector3D D)
        {
            Vector3D CmP = new Vector3D(C.X - A.X, C.Y - A.Y);
            Vector3D r = new Vector3D(B.X - A.X, B.Y - A.Y);
            Vector3D s = new Vector3D(D.X - C.X, D.Y - C.Y);

            float CmPxr = CmP.X * r.Y - CmP.Y * r.X;
            float CmPxs = CmP.X * s.Y - CmP.Y * s.X;
            float rxs = r.X * s.Y - r.Y * s.X;

            if (CmPxr == 0f)
                // Lines are collinear, and so intersect if they have any overlap

                return C.X - A.X < 0f != C.X - B.X < 0f
                       || C.Y - A.Y < 0f != C.Y - B.Y < 0f;

            if (rxs == 0f)
                return false; // Lines are parallel.

            float rxsr = 1f / rxs;
            float t = CmPxs * rxsr;
            float u = CmPxr * rxsr;

            return t >= 0f && t <= 1f && u >= 0f && u <= 1f;
        }



        /// <summary>
        /// Determines if the line intersects with the plane.
        /// Algorithm based on: http://paulbourke.net/geometry/planeline/
        /// </summary>
        /// <param name="plane">Plane to calculate the intersection with.</param>
        /// <returns>A value corresponding to m, in P = P0 + m * D. So to get the point, just use the [] operator. If the line is parallel or contained in the plane, then the result is null.</returns>
        public float? IntersectsPlane(Plane3D plane)
        {
            Vector3D p1 = Point0;
            Vector3D p2 = Point0 + Direction;

            float nominator = plane.Normal.Dot(plane.Point0 - p1);
            float denominator = plane.Normal.Dot(p2 - p1);

            //if it's 0, then the line is parallel to the plane, so no intersection
            if (Math.Abs(denominator) < float.Epsilon) return null;

            return nominator / denominator;
        }



        /// <summary>
        /// Finds the intersection between this line and the given sphere.
        /// </summary>
        /// <param name="sphereCenter">The sphere center.</param>
        /// <param name="radius">The sphere radius.</param>
        /// <returns>The array of intersected locations. Could be empty (if there were no intersections), or have up to 2 elements/intersections.</returns>
        /// <remarks>See http://csharphelper.com/blog/2014/09/determine-where-a-line-intersects-a-circle-in-c/ for original code.</remarks>
        public Vector3D[] FindSphereIntersectionPoints(Vector3D sphereCenter, float radius)
        {
            var direction = Direction;
            var point0 = Point0;

            return FindSphereIntersectionValues(sphereCenter, radius).Select(t => point0 + direction * t).ToArray();
        }



        /// <summary>
        /// Finds the intersection between this line and the given sphere, returning values between 0 and 1 relative to the line's points.
        /// </summary>
        /// <param name="sphereCenter">The sphere center.</param>
        /// <param name="radius">The sphere radius.</param>
        /// <returns>The array of intersected t values (between 0 and 1). Could be empty (if there were no intersections), or have up to 2 elements/intersections.</returns>
        /// <remarks>See http://csharphelper.com/blog/2014/09/determine-where-a-line-intersects-a-circle-in-c/ for original code.</remarks>
        public float[] FindSphereIntersectionValues(Vector3D sphereCenter, float radius)
        {
            float dx = Point1.X - Point0.X;
            float dy = Point1.Y - Point0.Y;
            float dz = Point1.Z - Point0.Z;

            float a = dx * dx + dy * dy + dz * dz;
            float b = 2 * (dx * (Point0.X - sphereCenter.X) + dy * (Point0.Y - sphereCenter.Y) + dz * (Point0.Z - sphereCenter.Z));
            float c = (Point0.X - sphereCenter.X) * (Point0.X - sphereCenter.X) + (Point0.Y - sphereCenter.Y) * (Point0.Y - sphereCenter.Y) + (Point0.Z - sphereCenter.Z) * (Point0.Z - sphereCenter.Z) - radius * radius;

            float det = b * b - 4 * a * c;
            if (a <= 0.0000001 || det < 0)
                // No real solutions.
                return new float[0];

            if (Math.Abs(det) < float.Epsilon)
            {
                // One solution.
                float t = -b / (2 * a);

                return new[] {t};
            }

            // Two solutions.
            float t1 = (float) ((-b + Math.Sqrt(det)) / (2 * a));
            float t2 = (float) ((-b - Math.Sqrt(det)) / (2 * a));

            return new[] {t1, t2};
        }



        public Vector3D Direction
        {
            get;
        }


        public Vector3D Point0
        {
            get;
        }


        public Vector3D Point1 => Point0 + Direction;



        public static Line3D FromEndPoints(Vector3D pointBegin, Vector3D pointEnd)
        {
            return new Line3D(pointEnd - pointBegin, pointBegin);
        }



        public static Line3D FromPointAndDirection(Vector3D direction, Vector3D point0)
        {
            return new Line3D(direction, point0);
        }
    }
}