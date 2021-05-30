using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Spatial
{
    /// <summary>
    /// This is a bounding box structure
    /// </summary>
    public class BoundingBox
    {
        private Vector3D _max;
        private Vector3D _min;



        /// <summary>
        /// Constructor for a boundingbox with infinite inverted sizes (minimum is +Inf and maximum is -Inf).
        /// </summary>
        public BoundingBox()
        {
            _min = Vector3D.Infinity;
            _max = -Vector3D.Infinity;
        }



        public BoundingBox(float left, float top, float front, float right, float bottom, float back)
        {
            _min = new Vector3D(left, top, front);
            _max = new Vector3D(right, bottom, back);
        }



        public BoundingBox(IEnumerable<Vector3D> points) : this()
        {
            foreach (Vector3D vector3D in points) AddPoint(vector3D);
        }



        public BoundingBox(Vector3D min, Vector3D max)
        {
            _min = min;
            _max = max;

            if (min.X > max.X || min.Y > max.Y || min.Z > max.Z)
                throw new ArgumentException("Coordinates in 'min' cannot be greater than 'max'.");
        }



        public BoundingBox(float width, float height, float depth)
        {
            _min = new Vector3D(0, 0, 0);
            _max = new Vector3D(width, height, depth);
        }



        public BoundingRectangle BoundingRectangle => new BoundingRectangle(new Vector2D(_min), new Vector2D(_max));


        /// <summary>
        /// Gets a bounding sphere that fits inside this bounding box.
        /// </summary>
        /// <value>
        /// The bounding sphere.
        /// </value>
        public BoundingSphere BoundingSphere => new BoundingSphere(_min + Vector3D.Scale(_max - _min, 0.5f), Vector3D.Scale(_max - _min, 0.5f).Length);



        /// <summary>
        /// This property will translate the bounding box to the given center
        /// </summary>
        public Vector3D Center
        {
            get
            {
                return _min + (_max - _min) / 2f; // boundingsphere.Center;
            }
            set
            {
                Vector3D halfDiagonally = Vector3D.Scale(_max - _min, 0.5f);
                _min = value - halfDiagonally;
                _max = value + halfDiagonally;
                //_boundingsphere.Center = value;
            }
        }



        /// <summary>
        /// This will return the 8 corners
        /// </summary>
        public Vector3D[] Corners
        {
            get
            {
                //vectors
                Vector3D diagonally = _max - _min;
                Vector3D width = new Vector3D(diagonally.X, 0, 0);
                Vector3D height = new Vector3D(0, diagonally.Y, 0);
                Vector3D depth = new Vector3D(0, 0, diagonally.Z);
                Vector3D[] ret = new Vector3D[8];

                for (int index = 0; index < 8; index++)
                {
                    //coordinates
                    int x = index & 1;
                    int y = (index & 2) / 2;
                    int z = (index & 4) / 4;

                    //result
                    ret[index] = _min + width * x + height * y + depth * z;
                }

                return ret;
            }
        }



        /// <summary>
        /// Gets the height (size in Z) of the boundingbox.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public float Height => _max.Z - _min.Z;


        public bool IsInfinity => _min.IsInfinity || _max.IsInfinity;


        /// <summary>
        /// Gets the length (size in Y) of the bounding box.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public float Length => _max.Y - _min.Y;


        /// <summary>
        /// Gets the maximum of the bounding box.
        /// </summary>
        /// <value>
        /// The maximum.
        /// </value>
        public Vector3D Max => _max;


        /// <summary>
        /// Gets the minimum of the bounding box.
        /// </summary>
        /// <value>
        /// The minimum.
        /// </value>
        public Vector3D Min => _min;


        /// <summary>
        /// Gets the size of the box in the 3 dimensions.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public Vector3D Size => _max - _min;


        /// <summary>
        /// Gets the volume of the box.
        /// </summary>
        /// <value>
        /// The volume.
        /// </value>
        public float Volume => Height * Width * Length;


        /// <summary>
        /// Gets the width (size in X) of the bounding box.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public float Width => _max.X - _min.X;



        /// <summary>
        /// Expands the bounding box to as to contain the given point.
        /// </summary>
        /// <param name="point">The point which must be contained in the box after the expansion.</param>
        public void AddPoint(Vector3D point)
        {
            if (!Contains(point))
            {
                _min = Vector3D.Minimize(point, _min);
                _max = Vector3D.Maximize(point, _max);
            }
        }



        /// <summary>
        /// Checks whether a point is inside the box.
        /// </summary>
        /// <param name="point">The point to check</param>
        /// <returns></returns>
        public bool Contains(Vector3D point)
        {
            return point.X >= _min.X && point.X <= _max.X
                                     && point.Y >= _min.Y && point.Y <= _max.Y
                                     && point.Z >= _min.Z && point.Z <= _max.Z;
        }



        /// <summary>
        /// Determines whether a given target bounding box is contained inside the current one.
        /// </summary>
        /// <param name="target">The target bounding box.</param>
        /// <returns>
        ///   <c>true</c> if this bounding box contains the specified target; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(BoundingBox target)
        {
            if (target._min.X >= _min.X &&
                target._min.Y >= _min.Y &&
                target._min.Z >= _min.Z &&
                target._max.X <= _max.X &&
                target._max.Y <= _max.Y &&
                target._max.Z <= _max.Z)
                return true;

            return false;
        }



        protected bool Equals(BoundingBox other)
        {
            return _min.Equals(other._min) && _max.Equals(other._max);
        }



        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((BoundingBox) obj);
        }



        public void Expand(float size)
        {
            _min -= new Vector3D(size, size);
            _max += new Vector3D(size, size);
        }



        public void Expand(Vector3D size)
        {
            _min -= size;
            _max += size;
        }



        public static BoundingBox FromPoints(IEnumerable<Vector3D> points)
        {
            BoundingBox boundingBox = new BoundingBox();

            foreach (Vector3D point in points)
                boundingBox.AddPoint(point);

            return boundingBox;
        }



        /// <summary>
        /// Checks whether the box fully contains the target box
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [Obsolete("Replaced with contains method.")]
        public bool FullyContains(BoundingBox target)
        {
            if (target._min.X >= _min.X &&
                target._min.Y >= _min.Y &&
                target._min.Z >= _min.Z &&
                target._max.X <= _max.X &&
                target._max.Y <= _max.Y &&
                target._max.Z <= _max.Z)
                return true;

            return false;
        }



        public override int GetHashCode()
        {
            unchecked
            {
                return (_min.GetHashCode() * 397) ^ _max.GetHashCode();
            }
        }



        /// <summary>
        /// This function will return one of the 8 sub boxes
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public BoundingBox GetSubBox(int index)
        {
            //vectors
            Vector3D halfDiagonally = Vector3D.Scale(_max - _min, 0.5f);
            Vector3D halfWidth = new Vector3D(halfDiagonally.X, 0, 0);
            Vector3D halfHeight = new Vector3D(0, halfDiagonally.Y, 0);
            Vector3D halfDepth = new Vector3D(0, 0, halfDiagonally.Z);

            //coordinates
            int x = index & 1;
            int y = (index & 2) / 2;
            int z = (index & 4) / 4;

            //result
            Vector3D newLeftTopFront = _min + halfWidth * x + halfHeight * y + halfDepth * z;
            return new BoundingBox(newLeftTopFront, newLeftTopFront + halfDiagonally);
        }



        /// <summary>
        /// this will return the index of the containing sub box
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public int GetSubBoxIndex(Vector3D point)
        {
            //vectors
            Vector3D halfDiagonally = Vector3D.Scale(_max - _min, 0.5f);
            double halfWidth = halfDiagonally.X;
            double halfHeight = halfDiagonally.Y;
            double halfDepth = halfDiagonally.Z;
            point -= _min;

            int x = 0;
            int y = 0;
            int z = 0;
            if (point.X >= halfWidth) x = 1;
            if (point.Y >= halfHeight) y = 2;
            if (point.Z >= halfDepth) z = 4;

            return x | y | z;
        }



        /// <summary>
        /// this function will return the indexes touched by the target box
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public int[] GetSubBoxIndexes(BoundingBox box)
        {
            throw new Exception("Sadly this is not implemented yet");
        }



        public BoundingBox Intersection(BoundingBox boundingBox)
        {
            var newMin = Vector3D.Maximize(boundingBox.Min, Min);
            var newMax = Vector3D.Minimize(boundingBox.Max, Max);

            if (newMin.X < newMax.X && newMin.Y < newMax.Y && newMin.Z < newMax.Z)
                return new BoundingBox(newMin, newMax);

            return null;
        }



        public static BoundingBox Intersection(IEnumerable<BoundingBox> boundingRectangles)
        {
            //start the aggregation with an infinite boundingrectangle, keep on going if null is returned (meaning no intersection exists)
            return boundingRectangles.Aggregate(new BoundingBox(-Vector3D.Infinity, Vector3D.Infinity), (valResult, val) => valResult == null ? null : valResult.Intersection(val));
        }



        /// <summary>
        /// Checks wether the box intersects the target. The boxes must be axis aligned
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool Intersects(BoundingBox target)
        {
            //combine
            Vector3D combinedMin = Vector3D.Minimize(_min, target._min);
            Vector3D combinedMax = Vector3D.Maximize(_max, target._max);

            if (
                combinedMax.X - combinedMin.X > Width + target.Width ||
                combinedMax.Y - combinedMin.Y > Length + target.Length ||
                combinedMax.Z - combinedMin.Z > Height + target.Height
            ) return false;

            return true;
        }



        /// <summary>
        /// Always override this or the class will be boxed
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Bounding box";
        }



        /// <summary>
        /// Merges the current boundingBox with the indicated one and returns a merged copy.
        /// </summary>
        /// <param name="boundingBox"></param>
        public BoundingBox Union(BoundingBox boundingBox)
        {
            return new BoundingBox(Vector3D.Minimize(boundingBox.Min, Min), Vector3D.Maximize(boundingBox.Max, Max));
        }



        public static BoundingBox Union(IEnumerable<BoundingBox> boundingBoxes)
        {
            return boundingBoxes.Aggregate(new BoundingBox(), (sum, val) => sum.Union(val));
        }
    }
}