using System;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Geometry
{
    /// <summary>
    /// Defines an infinite Line in 3D Euclidean space.
    /// A line will be defined by a point P0 and a direction D, so that a point on the line has an equation P = P0 + m * D
    /// 
    /// Explanation at: http://paulbourke.net/geometry/lineline3d/
    /// </summary>
    public struct Line2D
    {
        private readonly Vector2D _direction;
        private readonly Vector2D _point0;



        public Line2D(Vector2D direction, Vector2D point0)
        {
            _direction = direction;
            _point0 = point0;
        }



        public float MinDistanceToPoint(Vector2D point)
        {
            return Math.Abs(_direction.Y * point.X - _direction.X * point.Y + Point1.X * _point0.Y - Point1.Y * _point0.X) / _direction.Length;
        }



        /// <summary>
        /// Determines the point at the location m on the line, according to the line equation P = P0 + m * D
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public Vector2D this[float m] => _direction * m + _point0;



        public Vector2D Direction => _direction;


        public Vector2D Point0 => _point0;


        public Vector2D Point1 => _point0 + _direction;



        public static Line2D FromEndPoints(Vector2D pointBegin, Vector2D pointEnd)
        {
            return new Line2D(pointEnd - pointBegin, pointBegin);
        }



        public static Line2D FromPointAndDirection(Vector2D direction, Vector2D point0)
        {
            return new Line2D(direction, point0);
        }



        /// <summary>
        /// Finds the intersection between this line and the given circle.
        /// </summary>
        /// <param name="circleCenter">The circle center.</param>
        /// <param name="radius">The circle radius.</param>
        /// <returns>The array of intersected locations. Could be empty (if there were no intersections), or have up to 2 elements/intersections.</returns>
        /// <remarks>See http://csharphelper.com/blog/2014/09/determine-where-a-line-intersects-a-circle-in-c/ for original code.</remarks>
        public Vector2D[] FindLineCircleIntersections(Vector2D circleCenter, float radius)
        {
            var dx = Point1.X - Point0.X;
            var dy = Point1.Y - Point0.Y;

            var a = dx * dx + dy * dy;
            var b = 2 * (dx * (Point0.X - circleCenter.X) + dy * (Point0.Y - circleCenter.Y));
            var c = (Point0.X - circleCenter.X) * (Point0.X - circleCenter.X) + (Point0.Y - circleCenter.Y) * (Point0.Y - circleCenter.Y) - radius * radius;

            var det = b * b - 4 * a * c;
            if (a <= 0.0000001 || det < 0)
            {
                // No real solutions.
                return new Vector2D[0];
            }

            if (Math.Abs(det) < float.Epsilon)
            {
                // One solution.
                float t = -b / (2 * a);

                return new[] {new Vector2D(Point0.X + t * dx, Point0.Y + t * dy)};
            }
            else
            {
                // Two solutions.
                float t = (float) ((-b + Math.Sqrt(det)) / (2 * a));
                var intersection1 = new Vector2D(Point0.X + t * dx, Point0.Y + t * dy);

                t = (float) ((-b - Math.Sqrt(det)) / (2 * a));
                var intersection2 = new Vector2D(Point0.X + t * dx, Point0.Y + t * dy);

                return new[] {intersection1, intersection2};
            }
        }
    }
}