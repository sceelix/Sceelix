using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Sceelix.Designer.Graphs.GUI.Basic
{
    public struct RectangleF
    {
        private Vector2 _min;
        private Vector2 _max;



        public RectangleF(Vector2 min, Vector2 max)
        {
            _min = min;
            _max = max;
        }



        public RectangleF(float x, float y, float width, float height)
        {
            _min = new Vector2(x, y);
            _max = new Vector2(x + width, y + height);
        }



        public RectangleF(params Vector2[] points) : this((IEnumerable<Vector2>) points)
        {
        }



        public RectangleF(IEnumerable<Vector2> points) : this()
        {
            foreach (var vector2D in points)
            {
                AddPoint(vector2D);
            }
        }



        public RectangleF(Rectangle rectangle)
            : this(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height)
        {
        }



        public static RectangleF Zero
        {
            get { return new RectangleF(new Vector2(0, 0), new Vector2(0, 0)); }
        }



        public static RectangleF MinBounds
        {
            get
            {
                return new RectangleF(new Vector2(Single.MaxValue, Single.MaxValue),
                    new Vector2(-Single.MaxValue, -Single.MaxValue));
            }
        }



        public Vector2 Min
        {
            get { return _min; }
            set { _min = value; }
        }



        public Vector2 Max
        {
            get { return _max; }
            set { _max = value; }
        }



        public float Width
        {
            get { return _max.X - _min.X; }
        }



        public float Height
        {
            get { return _max.Y - _min.Y; }
        }



        public Vector2 Center
        {
            get { return _min + (_max - _min)/2f; }
        }



        public void AddPoint(Vector2 point)
        {
            if (!ContainsPoint(point))
            {
                _min = Vector2.Min(point, _min);
                _max = Vector2.Max(point, _max);
            }
        }



        /// <summary>
        ///     Checks whether a point is inside the box
        /// </summary>
        /// <param name="point">The point to check</param>
        /// <returns></returns>
        public bool ContainsPoint(Vector2 point)
        {
            return point.X >= _min.X && point.X <= _max.X && point.Y >= _min.Y && point.Y <= _max.Y;
        }



        public void Expand(float amount)
        {
            _min = new Vector2(_min.X - amount, _min.Y - amount);
            _max = new Vector2(_max.X + amount, _max.Y + amount);
        }



        public Rectangle ToXnaRectangle()
        {
            return new Rectangle((int) _min.X, (int) _min.Y, (int) Width, (int) Height);
        }



        public bool ContainsRectangle(RectangleF innerRectangle)
        {
            return Left < innerRectangle.Left &&
                   Right > innerRectangle.Right &&
                   Top > innerRectangle.Top &&
                   Bottom < innerRectangle.Bottom;
        }



        public float Left
        {
            get { return _min.X; }
        }



        public float Right
        {
            get { return _max.X; }
        }



        public float Top
        {
            get { return _min.Y; }
        }



        public float Bottom
        {
            get { return _max.Y; }
        }



        public bool Intersects(RectangleF innerRectangle)
        {
            return !(innerRectangle.Left > Right ||
                     innerRectangle.Right < Left ||
                     innerRectangle.Top > Bottom ||
                     innerRectangle.Bottom < Top);
        }



        public void Translate(int x, int y)
        {
            _min.X += x;
            _max.X += x;

            _min.Y += y;
            _max.Y += y;
        }



        public RectangleF Merge(RectangleF rect)
        {
            return new RectangleF(Vector2.Min(rect.Min, Min), Vector2.Max(rect.Max, Max));
        }



        public static RectangleF Merge(IEnumerable<RectangleF> boundingRectangles)
        {
            return boundingRectangles.Aggregate((sum, val) => sum.Merge(val));
        }
    }
}