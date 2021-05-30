using System;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Geometry
{
    /// <summary>
    /// Defines a line segment in 2D space.
    /// </summary>
    public class LineSegment2D
    {
        private Vector2D _start;
        private Vector2D _end;



        public LineSegment2D(Vector2D start, Vector2D end)
        {
            _start = start;
            _end = end;
        }



        /// <summary> 
        /// Gets the center of this line segment
        /// </summary>
        public Vector2D Center => new Vector2D(0.5f * (_start.X + _end.X), 0.5f * (_start.Y + _end.Y));


        public Vector2D Direction => _end - _start;


        public Vector2D End => _end;


        /// <summary>
        /// Gets the length of this <see cref="LineSegment2D"/>.
        /// </summary>
        public float Length => Direction.Length;


        /// <summary>
        /// Gets the squared length of this <see cref="LineSegment2D"/>.
        /// </summary>
        public float LengthSquared => Direction.SquareLength;


        /// <summary> 
        /// Gets the normal of the line Segment. 
        /// </summary>
        public Vector2D Normal => new Vector2D(_start.Y - _end.Y, _end.X - _start.X).Normalize();


        public Vector2D Start => _start;



        /// <summary>
        /// Find the closest point between <see cref="_start"/> and <see cref="_end"/>.
        /// </summary>
        public Vector2D ClosestPointOnLine(Vector2D point)
        {
            var lineLength = Length;
            var lineDir = Direction / lineLength;
            var distance = Vector2D.Dot(point - _start, lineDir);

            distance = Math.Min(Math.Max(0, distance), lineLength);
            /*if (distance <= 0)
                return _start;

            if (distance >= lineLength)
                return _end;*/

            return _start + lineDir * distance;
        }



        public Vector2D? Intersection(LineSegment2D value, bool includeEnds)
        {
            float x1 = _end.X - _start.X;
            float y1 = _end.Y - _start.Y;
            float x2 = value._end.X - value._start.X;
            float y2 = value._end.Y - value._start.Y;
            float d = x1 * y2 - y1 * x2;

            if (Math.Abs(d) < float.Epsilon)
                return null;

            float x3 = value._start.X - _start.X;
            float y3 = value._start.Y - _start.Y;
            float t = (x3 * y2 - y3 * x2) / d;
            float u = (x3 * y1 - y3 * x1) / d;

            if (includeEnds)
            {
                if (t < 0 || t > 1 || u < 0 || u > 1)
                    return null;
            }
            else
            {
                if (t <= 0 || t >= 1 || u <= 0 || u >= 1)
                    return null;
            }

            return new Vector2D(_start.X + t * x1, _start.Y + t * y1);
        }



        public bool Intersects(LineSegment2D value, bool includeEnds)
        {
            return Intersection(value, includeEnds).HasValue;
        }



        /// <summary>
        /// Find the minimum distance from this line segment to the given point.
        /// </summary>
        public float MinDistanceTo(Vector2D point)
        {
            return (ClosestPointOnLine(point) - point).Length;
        }



        /// <summary>
        /// Moves this <see cref="LineSegment2D"/> along its normal for the specified length.
        /// </summary>
        public void Offset(float length)
        {
            var normal = Normal;

            _start += normal * length;
            _end += normal * length;
        }
    }
}