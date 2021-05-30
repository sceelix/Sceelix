using System;
using System.Drawing;
using System.Globalization;

namespace Sceelix.Mathematics.Data
{
    public struct Vector2D
    {
        private static int _precisionDigits;

        #region Constructors

        static Vector2D()
        {
            PrecisionDigits = 5;
        }



        public Vector2D(float x, float y)
        {
            X = x;
            Y = y;
        }



        public Vector2D(Vector3D vector3D)
        {
            X = vector3D.X;
            Y = vector3D.Y;
        }

        #endregion

        #region Operator Overloading

        public static Vector2D operator *(Vector2D a, float scalar)
        {
            return new Vector2D(a.X * scalar, a.Y * scalar);
        }



        public static Vector2D operator *(float scalar, Vector2D a)
        {
            return new Vector2D(scalar * a.X, scalar * a.Y);
        }



        public static Vector2D operator /(Vector2D a, float scalar)
        {
            return new Vector2D(a.X / scalar, a.Y / scalar);
        }



        public static Vector2D operator *(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.X * b.X, a.Y * b.Y);
        }



        public static Vector2D operator /(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.X / b.X, a.Y / b.Y);
        }



        public static Vector2D operator +(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.X + b.X, a.Y + b.Y);
        }



        public static Vector2D operator -(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.X - b.X, a.Y - b.Y);
        }



        public static Vector2D operator -(Vector2D a)
        {
            return new Vector2D(-a.X, -a.Y);
        }

        #endregion

        #region Functions

        public static Vector2D Maximize(Vector2D v1, Vector2D v2)
        {
            float x = Math.Max(v1.X, v2.X);
            float y = Math.Max(v1.Y, v2.Y);

            return new Vector2D(x, y);
        }



        public static Vector2D Minimize(Vector2D v1, Vector2D v2)
        {
            float x = Math.Min(v1.X, v2.X);
            float y = Math.Min(v1.Y, v2.Y);

            return new Vector2D(x, y);
        }



        /// <summary>
        /// Determines if the given vector if within a certain radius (inclusive) of the current vector.
        /// </summary>
        /// <param name="b">Vector to compare to.</param>
        /// <param name="radius">Radius for comparison.</param>
        /// <returns>True if the vector b within the specified radius distance (inclusive) or false otherwise.</returns>
        public bool IsWithinRadius(Vector2D b, float radius)
        {
            float dx = X - b.X;
            float dy = Y - b.Y;

            return dx * dx + dy * dy <= radius * radius;
        }



        /// <summary>
        /// Determines if the given vector if within a certain radius (inclusive) of the current vector.
        /// </summary>
        /// <param name="b">Vector to compare to.</param>
        /// <param name="radius">Radius for comparison.</param>
        /// <returns>True if the vector b within the specified radius distance (inclusive) or false otherwise.</returns>
        public static bool WithinRadius(Vector2D a, Vector2D b, float radius)
        {
            return a.SquareLength - b.SquareLength <= radius * radius;
        }



        public Vector3D ToVector3D(float z = 0)
        {
            return new Vector3D(X, Y, z);
        }



        public Vector2D Normalize()
        {
            return this / Length;
        }



        public float[] ToArray()
        {
            return new[] {X, Y};
        }



        public static Vector2D Infinity => new Vector2D(float.PositiveInfinity, float.PositiveInfinity);



        public override string ToString()
        {
            return string.Format("X: {0}, Y: {1}", X.ToString(CultureInfo.InvariantCulture), Y.ToString(CultureInfo.InvariantCulture));
        }



        public float this[string coordinate]
        {
            get
            {
                var lowerCoordinate = coordinate.ToLower();

                if (lowerCoordinate == "x")
                    return X;
                if (lowerCoordinate == "y")
                    return Y;

                throw new Exception("No valid coordinate '" + coordinate + "' exists.");
            }
        }



        public float this[int index] => ToArray()[index];



        public float DistanceTo(Vector2D b)
        {
            float dx = X - b.X;
            float dy = Y - b.Y;

            return (float) Math.Sqrt(dx * dx + dy * dy);
        }



        public static float Distance(Vector2D a, Vector2D b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;

            return (float) Math.Sqrt(dx * dx + dy * dy);
        }



        public float Dot(Vector2D b)
        {
            return X * b.X + Y * b.Y;
        }



        public static float Dot(Vector2D a, Vector2D b)
        {
            return a.X * b.X + a.Y * b.Y;
        }



        /// <summary>
        /// Calculates the angle between this and the given vector.
        /// </summary>
        /// <param name="b">The vector against with the angle should be calculated.</param>
        /// <returns>The angle between the 2 vectors, in radians.</returns>
        public float AngleTo(Vector2D b)
        {
            float f = Dot(b) / (Length * b.Length);

            return (float) Math.Acos(Math.Round(f, PrecisionDigits));
        }



        /// <summary>
        /// Calculates the angle between two vectors.
        /// </summary>
        /// <param name="a">The first vector</param>
        /// <param name="b">The second vector</param>
        /// <returns>The angle between the two vectors, in radians.</returns>
        public static float Angle(Vector2D a, Vector2D b)
        {
            return (float) Math.Acos(Dot(a, b) / (a.Length * b.Length));
        }



        public Vector2D FlipXY()
        {
            return new Vector2D(Y, X);
        }



        public static Vector2D New(float x, float y)
        {
            return new Vector2D(x, y);
        }



        public bool IsOrthogonal(Vector2D normal)
        {
            return Math.Abs(Dot(normal) - 0) < float.Epsilon;
        }



        public static bool IsOrthogonal(Vector2D v1, Vector2D v2)
        {
            return v1.IsOrthogonal(v2);
        }



        public static Vector2D Scale(Vector2D v, float value)
        {
            return v * value;
        }



        public static Vector2D operator +(Vector2D a, float scalar)
        {
            return new Vector2D(a.X + scalar, a.Y + scalar);
        }



        public static Vector2D operator -(Vector2D a, float scalar)
        {
            return new Vector2D(a.X - scalar, a.Y - scalar);
        }



        //  User-defined conversion from double to Digit
        public static implicit operator Point(Vector2D vector)
        {
            return new Point((int) vector.X, (int) vector.Y);
        }



        public static implicit operator Size(Vector2D vector)
        {
            return new Size((int) vector.X, (int) vector.Y);
        }



        //  User-defined conversion from double to Digit
        public static implicit operator Vector2D(Point point)
        {
            return new Vector2D(point.X, point.Y);
        }



        public static implicit operator Vector2D(Size size)
        {
            return new Vector2D(size.Width, size.Height);
        }



        public string ToString(string format, IFormatProvider provider = null)
        {
            provider = provider ?? CultureInfo.InvariantCulture;

            return string.Format(format, X.ToString(provider), Y.ToString(provider));
        }

        #endregion

        #region Constants

        /// <summary>
        /// 2-Dimensional single-precision floating point zero vector.
        /// </summary>
        public static readonly Vector2D Zero = new Vector2D(0.0f, 0.0f);

        /// <summary>
        /// 2-Dimensional single-precision floating point 1 vector (X = 1,Y = 1).
        /// </summary>
        public static readonly Vector2D One = new Vector2D(1f, 1f);


        /// <summary>
        /// 2-Dimensional single-precision floating point X-Axis vector.
        /// </summary>
        public static readonly Vector2D XAxis = new Vector2D(1.0f, 0.0f);


        /// <summary>
        /// 2-Dimensional single-precision floating point Y-Axis vector.
        /// </summary>
        public static readonly Vector2D YAxis = new Vector2D(0.0f, 1.0f);

        #endregion

        #region Properties

        /// <summary>
        /// The X-Coordinate of the Vector.
        /// </summary>
        public float X
        {
            get;
        }


        /// <summary>
        /// The Y-Coordinate of the Vector
        /// </summary>
        public float Y
        {
            get;
        }



        /// <summary>
        /// Precision Digits for error margin in calculations (for instance, 4 corresponds to 0.0001 margin).
        /// </summary>
        public static int PrecisionDigits
        {
            get { return _precisionDigits; }
            set
            {
                _precisionDigits = value;
                Precision = (float) Math.Pow(0.1, _precisionDigits);
            }
        }



        /// <summary>
        /// Precision/error margin in calculations
        /// </summary>
        public static float Precision
        {
            get;
            private set;
        }


        public float SquareLength => X * X + Y * Y;


        public float Length => (float) Math.Sqrt(X * X + Y * Y);

        #endregion
    }
}