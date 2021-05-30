using System;
using System.Globalization;

namespace Sceelix.Mathematics.Data
{
    public struct Vector4D
    {
        private static int _precisionDigits;

        #region Constructors

        static Vector4D()
        {
            PrecisionDigits = 5;
        }



        public Vector4D(float x, float y)
        {
            X = x;
            Y = y;
            Z = 0;
            W = 0;
        }



        public Vector4D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
            W = 0;
        }



        public Vector4D(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }



        public Vector4D(float? x, float? y, float? z, float? w)
        {
            X = x ?? 0;
            Y = y ?? 0;
            Z = z ?? 0;
            W = w ?? 0;
        }



        public Vector4D(Vector2D vector2D, float z = 0, float w = 0)
        {
            X = vector2D.X;
            Y = vector2D.Y;
            Z = z;
            W = w;
        }



        public Vector4D(Vector3D vector3D, float w = 0)
        {
            X = vector3D.X;
            Y = vector3D.Y;
            Z = vector3D.Z;
            W = w;
        }

        #endregion

        #region Functions

        public float DistanceTo(Vector4D b)
        {
            float dx = X - b.X;
            float dy = Y - b.Y;
            float dz = Z - b.Z;
            float dw = Z - b.W;

            return (float) Math.Sqrt(dx * dx + dy * dy + dz * dz + dw * dw);
        }



        public static float Distance(Vector4D a, Vector4D b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            float dz = a.Z - b.Z;
            float dw = a.W - b.W;

            return (float) Math.Sqrt(dx * dx + dy * dy + dz * dz + dw * dw);
        }



        public float Dot(Vector4D b)
        {
            return X * b.X + Y * b.Y + Z * b.Z + W * b.W;
        }



        public static float Dot(Vector4D a, Vector4D b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
        }



        /// <summary>
        /// Determines if the given vector if within a certain radius (inclusive) of the current vector.
        /// </summary>
        /// <param name="b">Vector to compare to.</param>
        /// <param name="radius">Radius for comparison.</param>
        /// <returns>True if the vector b within the specified radius distance (inclusive) or false otherwise.</returns>
        public bool IsWithinRadius(Vector4D b, float radius)
        {
            float dx = X - b.X;
            float dy = Y - b.Y;
            float dz = Z - b.Z;
            float dw = W - b.W;

            return dx * dx + dy * dy + dz * dz + dw * dw <= radius * radius;
        }



        /// <summary>
        /// Determines if the given vector if within a certain radius (inclusive) of the current vector.
        /// </summary>
        /// <param name="b">Vector to compare to.</param>
        /// <param name="radius">Radius for comparison.</param>
        /// <returns>True if the vector b within the specified radius distance (inclusive) or false otherwise.</returns>
        public static bool WithinRadius(Vector4D a, Vector4D b, float radius)
        {
            return a.SquareLength - b.SquareLength <= radius * radius;
        }



/*public Vector4D Cross(Vector4D b)
                {
                    return new Vector4D(_y*b._z - _z*b._y, _z*b._x - _x*b._z, _x*b._y - _y*b._x);
                }
        
        
                public static Vector3D Cross(Vector3D a, Vector3D b)
                {
                    return new Vector3D(a._y*b._z - a._z*b._y, a._z*b._x - a._x*b._z, a._x*b._y - a._y*b._x);
                }*/



        /// <summary>
        /// Calculates the angle between this and the given vector.
        /// </summary>
        /// <param name="b">The vector against with the angle should be calculated.</param>
        /// <returns>The angle between the 2 vectors, in radians.</returns>
        public float AngleTo(Vector4D b)
        {
            float f = Dot(b) / (Length * b.Length);

            return (float) Math.Acos(Math.Round(f, PrecisionDigits));
        }



/*public float AngleAroundAxisTo(Vector3D axis, Vector3D b)
                {
                    float f = Dot(b) / (Length * b.Length);
        
                    return (float)Math.Acos(Math.Round(f, PrecisionDigits));
                }*/



        /// <summary>
        /// Calculates the angle between two vectors.
        /// </summary>
        /// <param name="a">The first vector</param>
        /// <param name="b">The second vector</param>
        /// <returns>The angle between the two vectors, in radians.</returns>
        public static float Angle(Vector4D a, Vector4D b)
        {
            return (float) Math.Acos(Dot(a, b) / (a.Length * b.Length));
        }



        public float this[int index] => ToArray()[index];



        public float this[string coordinate]
        {
            get
            {
                var lowerCoordinate = coordinate.ToLower();

                if (lowerCoordinate == "x")
                    return X;
                if (lowerCoordinate == "y")
                    return Y;
                if (lowerCoordinate == "z")
                    return Z;
                if (lowerCoordinate == "w")
                    return W;

                throw new Exception("No valid coordinate '" + coordinate + "' exists.");
            }
        }



        public Vector4D Normalize()
        {
            float len = Length;

            return this / len;
        }



/*public Vector3D ProjectToPlane(Vector3D planeNormal)
                {
                    var sideDirection = Cross(planeNormal);
        
                    var planeDirection = planeNormal.Cross(sideDirection).Normalize();
        
                    return planeDirection * Dot(this, planeDirection);
                }*/



        public override bool Equals(object obj)
        {
            if (obj is Vector4D)
            {
                var v = (Vector4D) obj;

                return Math.Abs(X - v.X) < Precision && Math.Abs(Y - v.Y) < Precision &&
                       Math.Abs(Z - v.Z) < Precision && Math.Abs(W - v.W) < Precision;
            }

            return false;
        }



        public bool AproxEquals(Vector4D v, float precision)
        {
            return Math.Abs(X - v.X) < precision && Math.Abs(Y - v.Y) < precision &&
                   Math.Abs(Z - v.Z) < precision && Math.Abs(W - v.W) < precision;
        }



        public bool ExactlyEquals(Vector4D v)
        {
            return X == v.X && Y == v.Y && Z == v.Z && W == v.W;
        }



        public override int GetHashCode()
        {
            return base.GetHashCode();
        }



        public object Clone()
        {
            return new Vector4D(X, Y, Z, W);
        }



        public static bool operator ==(Vector4D a, Vector4D b)
        {
            return a.ExactlyEquals(b); //a.Length == b.Length
        }



        public static bool operator !=(Vector4D a, Vector4D b)
        {
            return !a.ExactlyEquals(b); //a.Length != b.Length
        }



        public static Vector4D Lerp(Vector4D v1, Vector4D v2, float value)
        {
            return v1 + (v2 - v1) * value;
        }



/*public bool IsCollinear(Vector4D v1)
                {
                    return IsCollinear(this, v1);
                }*/

        /*public static float Precision
        {
            get { return _precision; }
        }*/



        public Vector4D Round()
        {
            return new Vector4D((float) Math.Round(X, PrecisionDigits), (float) Math.Round(Y, PrecisionDigits), (float) Math.Round(Z, PrecisionDigits), (float) Math.Round(W, PrecisionDigits));
        }



        public Vector4D Round(int decimalCases)
        {
            return new Vector4D((float) Math.Round(X, decimalCases), (float) Math.Round(Y, decimalCases), (float) Math.Round(Z, decimalCases), (float) Math.Round(W, decimalCases));
        }



/*public bool IsCollinear(Vector4D v2, float tolerance)
                {
                    return Cross(this.Normalize(), v2.Normalize()).Length < tolerance;
                }*/

        /*public static bool IsCollinear(Vector3D v1, Vector3D v2)
        {
            v1 = v1.Round();
            v2 = v2.Round();

            Vector3D vector3D = Cross(v1, v2);

            return Cross(v1, v2).Equals(Zero);
        }*/



        public override string ToString()
        {
            //return string.Format("X: {0}, Y: {1}, Z: {2}", _x.ToString(CultureInfo.InvariantCulture), _y.ToString(CultureInfo.InvariantCulture), _z.ToString(CultureInfo.InvariantCulture));
            return string.Format("v({0},{1},{2},{3})", X.ToString(CultureInfo.InvariantCulture), Y.ToString(CultureInfo.InvariantCulture), Z.ToString(CultureInfo.InvariantCulture), W.ToString(CultureInfo.InvariantCulture));
        }



        public string ToString(string format, IFormatProvider provider = null)
        {
            provider = provider ?? CultureInfo.InvariantCulture;

            return string.Format(format, X.ToString(provider), Y.ToString(provider), Z.ToString(provider), W.ToString(provider));
        }



        public static Vector4D Scale(Vector4D v, float value)
        {
            return v * value;
        }



        public static Vector4D Maximize(Vector4D v1, Vector4D v2)
        {
            float x = Math.Max(v1.X, v2.X);
            float y = Math.Max(v1.Y, v2.Y);
            float z = Math.Max(v1.Z, v2.Z);
            float w = Math.Max(v1.W, v2.W);

            return new Vector4D(x, y, z, w);
        }



        public static Vector4D Minimize(Vector4D v1, Vector4D v2)
        {
            float x = Math.Min(v1.X, v2.X);
            float y = Math.Min(v1.Y, v2.Y);
            float z = Math.Min(v1.Z, v2.Z);
            float w = Math.Min(v1.W, v2.W);

            return new Vector4D(x, y, z, w);
        }



/*public static float GetCommonMultiplier(Vector3D v1, Vector3D v2)
                {
                    float a = 0;
        
                    if (v2.X != 0)
                        a = v1.X/v2.X;
                    else if (v2.Y != 0)
                        a = v1.Y/v2.Y;
                    else if (v2.Z != 0)
                        a = v1.Z/v2.Z;
        
                    return a;
                }*/

        /*public static int LongAxis(ref Vector3D v)
        {
            int i = 0;
            if (Math.Abs(v.Y) > Math.Abs(v.X)) i = 1;
            if (Math.Abs(v.Z) > Math.Abs(i == 0 ? v.X : v.Y)) i = 2;
            return i;
        }*/



        public Vector2D ToVector2D()
        {
            return new Vector2D(X, Y);
        }



        public Vector3D ToVector3D()
        {
            return new Vector3D(X, Y, Z);
        }



        public bool IsOrthogonal(Vector4D normal)
        {
            return Math.Abs(Dot(normal) - 0) < float.Epsilon;
        }



        public static bool IsOrthogonal(Vector4D v1, Vector4D v2)
        {
            return v1.IsOrthogonal(v2);
        }



        public static Vector4D New(float x, float y, float z, float w)
        {
            return new Vector4D(x, y, z, w);
        }



        /// <summary>
        /// Replaces the components that might have Infinity or NaN as values and replaces them with the given value.
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public Vector4D MakeValid(float defaultValue = 0)
        {
            float x = float.IsNaN(X) || float.IsInfinity(X) ? defaultValue : X;
            float y = float.IsNaN(Y) || float.IsInfinity(Y) ? defaultValue : Y;
            float z = float.IsNaN(Z) || float.IsInfinity(Z) ? defaultValue : Z;
            float w = float.IsNaN(W) || float.IsInfinity(W) ? defaultValue : W;

            return new Vector4D(x, y, z, w);
        }



        public float[] ToArray()
        {
            return new[] {X, Y, Z, W};
        }

        #endregion


        #region Operator Overloading

        public static Vector4D operator *(Vector4D a, float scalar)
        {
            return new Vector4D(a.X * scalar, a.Y * scalar, a.Z * scalar, a.W * scalar);
        }



        public static Vector4D operator *(float scalar, Vector4D a)
        {
            return new Vector4D(scalar * a.X, scalar * a.Y, scalar * a.Z, scalar * a.W);
        }



        public static Vector4D operator /(Vector4D a, float scalar)
        {
            return new Vector4D(a.X / scalar, a.Y / scalar, a.Z / scalar, a.W / scalar);
        }



        public static Vector4D operator %(Vector4D a, float scalar)
        {
            return new Vector4D(a.X % scalar, a.Y % scalar, a.Z % scalar, a.W % scalar);
        }



        public static Vector4D operator +(Vector4D a, float scalar)
        {
            return new Vector4D(a.X + scalar, a.Y + scalar, a.Z + scalar, a.W + scalar);
        }



        public static Vector4D operator -(Vector4D a, float scalar)
        {
            return new Vector4D(a.X - scalar, a.Y - scalar, a.Z - scalar, a.W - scalar);
        }



        public static Vector4D operator -(Vector4D a)
        {
            return new Vector4D(-a.X, -a.Y, -a.Z, -a.W);
        }



        public static Vector4D operator *(Vector4D a, Vector4D b)
        {
            return new Vector4D(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
        }



        public static Vector4D operator /(Vector4D a, Vector4D b)
        {
            return new Vector4D(a.X / b.X, a.Y / b.Y, a.Z / b.Z, a.W / b.W);
        }



        public static Vector4D operator +(Vector4D a, Vector4D b)
        {
            return new Vector4D(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        }



        public static Vector4D operator -(Vector4D a, Vector4D b)
        {
            return new Vector4D(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
        }



        public static Vector4D operator %(Vector4D a, Vector4D b)
        {
            return new Vector4D(a.X % b.X, a.Y % b.Y, a.Z % b.Z, a.W % b.W);
        }



        public static bool operator <=(Vector4D a, Vector4D b)
        {
            return a.Length <= b.Length;
        }



        public static bool operator >=(Vector4D a, Vector4D b)
        {
            return a.Length >= b.Length;
        }



        public static bool operator <(Vector4D a, Vector4D b)
        {
            return a.Length < b.Length;
        }



        public static bool operator >(Vector4D a, Vector4D b)
        {
            return a.Length > b.Length;
        }

        #endregion

        #region Properties

        public float X
        {
            get;
        }


        public float Y
        {
            get;
        }


        public float Z
        {
            get;
        }


        public float W
        {
            get;
        }


        public float Length => (float) Math.Sqrt(X * X + Y * Y + Z * Z + W * W);


        public float SquareLength => X * X + Y * Y + Z * Z + W * W;


        public bool IsNumericallyZero => Length < Precision;


        public bool IsNaN => float.IsNaN(X) || float.IsNaN(Y) || float.IsNaN(Z) || float.IsNaN(W);


        /// <summary>
        /// Indicates if any of the 3 Coordinates evaluates to positive/negative infinity.
        /// </summary>
        public bool IsInfinity => float.IsInfinity(X) || float.IsInfinity(Y) || float.IsInfinity(Z) || float.IsInfinity(W);


        /// <summary>
        /// Indicates if any of the 3 Coordinates evaluates to either positive/negative infinity or NaN.
        /// </summary>
        public bool IsInfinityOrNaN => IsInfinity || IsNaN;

        #endregion

        #region Static Properties

        /// <summary>
        /// A vector with the three Components X,Y,Z set to 1.
        /// </summary>
        public static Vector4D One => new Vector4D(1, 1, 1);


        /// <summary>
        /// A vector with the three Components X,Y,Z set to 0.
        /// </summary>
        public static Vector4D Zero => new Vector4D(0, 0, 0);


        /// <summary>
        /// A vector with the X Component as 1 and the others as 0 .
        /// </summary>
        public static Vector4D XVector => new Vector4D(1, 0, 0);


        /// <summary>
        /// A vector with the Y Component as 1 and the others as 0 .
        /// </summary>
        public static Vector4D YVector => new Vector4D(0, 1, 0);


        /// <summary>
        /// A vector with the Z Component as 1 and the others as 0 .
        /// </summary>
        public static Vector4D ZVector => new Vector4D(0, 0, 1);


        /// <summary>
        /// A vector with the Z Component as 1 and the others as 0 .
        /// </summary>
        public static Vector4D WVector => new Vector4D(0, 0, 0, 1);


        /// <summary>
        /// A vector with the three Components X,Y,Z set to Positive Infinity.
        /// Do not use this for comparison, instead use IsInfinity.
        /// </summary>
        public static Vector4D Infinity => new Vector4D(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);


        /// <summary>
        /// A vector with the three Components X,Y,Z set to NaN.
        /// /// Do not use this for comparison, instead use IsNaN.
        /// </summary>
        public static Vector4D NaN => new Vector4D(float.NaN, float.NaN, float.NaN, float.NaN);



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

        #endregion
    }
}