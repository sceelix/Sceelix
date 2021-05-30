using System.Collections.Generic;
using System.Linq;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Spatial
{
    public class BoundingRectangle
    {
        private Vector2D _max;
        private Vector2D _min;



        public BoundingRectangle()
        {
            _min = Vector2D.Infinity;
            _max = -Vector2D.Infinity;
        }



        public BoundingRectangle(Rectangle rectangle)
        {
            _min = rectangle.Min;
            _max = rectangle.Max;
        }



        public BoundingRectangle(Vector2D min, Vector2D max)
        {
            _min = min;
            _max = max;
        }



        public BoundingRectangle(float x, float y, float width, float height)
        {
            _min = new Vector2D(x, y);
            _max = new Vector2D(x + width, y + height);
        }



        public BoundingRectangle(params Vector2D[] points)
            : this((IEnumerable<Vector2D>) points)
        {
        }



        public BoundingRectangle(IEnumerable<Vector2D> points) : this()
        {
            foreach (Vector2D vector2D in points) AddPoint(vector2D);
        }



        public Vector2D Center => _min + (_max - _min) / 2f;


        public float Height => _max.Y - _min.Y;


        public bool IsEmpty => Width == 0 || Height == 0;



        public Vector2D Max
        {
            get { return _max; }
            set { _max = value; }
        }



        public Vector2D Min
        {
            get { return _min; }
            set { _min = value; }
        }



        public Vector2D Size => new Vector2D(Width, Height);


        public float Width => _max.X - _min.X;


        public static BoundingRectangle Zero => new BoundingRectangle(new Vector2D(0, 0), new Vector2D(0, 0));



        public void AddPoint(Vector2D point)
        {
            if (!Contains(point))
            {
                _min = Vector2D.Minimize(point, _min);
                _max = Vector2D.Maximize(point, _max);
            }
        }



        /// <summary>
        /// Checks whether a point is inside the box
        /// </summary>
        /// <param name="point">The point to check</param>
        /// <returns></returns>
        public bool Contains(Vector2D point)
        {
            return point.X >= _min.X && point.X <= _max.X && point.Y >= _min.Y && point.Y <= _max.Y;
        }



        public bool Contains(BoundingRectangle rectangle)
        {
            return Contains(rectangle.Min) && Contains(rectangle.Max);
        }



        public bool Equals(BoundingRectangle other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other._min.Equals(_min) && other._max.Equals(_max);
        }



        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(BoundingRectangle)) return false;
            return Equals((BoundingRectangle) obj);
        }



        public void Expand(float value)
        {
            _min -= new Vector2D(value, value);
            _max += new Vector2D(value, value);
        }



        public BoundingRectangle GetExpanded(float value)
        {
            return new BoundingRectangle(_min - new Vector2D(value, value), _max + new Vector2D(value, value));
        }



        public override int GetHashCode()
        {
            unchecked
            {
                return (_min.GetHashCode() * 397) ^ _max.GetHashCode();
            }
        }



        public BoundingRectangle Intersection(BoundingRectangle boundingRectangle)
        {
            var newMin = Vector2D.Maximize(boundingRectangle.Min, Min);
            var newMax = Vector2D.Minimize(boundingRectangle.Max, Max);

            if (newMin.X < newMax.X && newMin.Y < newMax.Y)
                return new BoundingRectangle(newMin, newMax);

            return null;
        }



        public static BoundingRectangle Intersection(IEnumerable<BoundingRectangle> boundingRectangles)
        {
            //start the aggregation with an infinite boundingrectangle, keep on going if null is returned (meaning no intersection exists)
            return boundingRectangles.Aggregate(new BoundingRectangle(), (valResult, val) => valResult == null ? null : valResult.Intersection(val));
        }



        public bool Intersects(BoundingRectangle target)
        {
            //combine
            Vector2D combinedMin = Vector2D.Minimize(_min, target._min);
            Vector2D combinedMax = Vector2D.Maximize(_max, target._max);

            if (
                combinedMax.X - combinedMin.X > Width + target.Width ||
                combinedMax.Y - combinedMin.Y > Height + target.Height
            ) return false;

            return true;
        }



        public BoundingRectangle Union(BoundingRectangle boundingBox)
        {
            return new BoundingRectangle(Vector2D.Minimize(boundingBox.Min, Min), Vector2D.Maximize(boundingBox.Max, Max));
        }



        public static BoundingRectangle Union(IEnumerable<BoundingRectangle> boundingRectangles)
        {
            return boundingRectangles.Aggregate(new BoundingRectangle(), (sum, val) => sum.Union(val));
        }
    }
}