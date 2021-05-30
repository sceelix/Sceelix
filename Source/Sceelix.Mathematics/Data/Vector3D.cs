using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Sceelix.Extensions;

namespace Sceelix.Mathematics.Data
{
    public struct Vector3D
    {
        private static int _precisionDigits;

        #region Constructors

        static Vector3D()
        {
            PrecisionDigits = 5;
        }



        public Vector3D(float x, float y)
        {
            X = x;
            Y = y;
            Z = 0;
        }



        public Vector3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }



        public Vector3D(float? x, float? y, float? z)
        {
            X = x ?? 0;
            Y = y ?? 0;
            Z = z ?? 0;
        }



        public Vector3D(Vector2D vector2D, float z = 0)
        {
            X = vector2D.X;
            Y = vector2D.Y;
            Z = z;
        }



        public Vector3D(Vector3D vector3D)
        {
            X = vector3D.X;
            Y = vector3D.Y;
            Z = vector3D.Z;
        }

        #endregion

        #region Functions

        public float DistanceTo(Vector3D b)
        {
            float dx = X - b.X;
            float dy = Y - b.Y;
            float dz = Z - b.Z;

            return (float) Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }



        public static float Distance(Vector3D a, Vector3D b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            float dz = a.Z - b.Z;

            return (float) Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }



        /// <summary>
        /// Determines if the given vector b if within a certain radius (inclusive) of the current vector.
        /// </summary>
        /// <param name="b">Vector to compare to.</param>
        /// <param name="radius">Radius for comparison.</param>
        /// <returns>True if the vector b within the specified radius distance (inclusive) or false otherwise.</returns>
        public bool IsWithinRadius(Vector3D b, float radius)
        {
            float dx = X - b.X;
            float dy = Y - b.Y;
            float dz = Z - b.Z;

            return dx * dx + dy * dy + dz * dz <= radius * radius;
        }



        /// <summary>
        /// Determines if the given vector if within a certain radius (inclusive) of the current vector.
        /// </summary>
        /// <param name="b">Vector to compare to.</param>
        /// <param name="radius">Radius for comparison.</param>
        /// <returns>True if the vector b within the specified radius distance (inclusive) or false otherwise.</returns>
        /*public static bool WithinRadius(Vector3D a, Vector3D b, float radius)
        {
            return (a.SquareLength - b.SquareLength) <= radius*radius;
        }*/
        [Pure]
        public float Dot(Vector3D b)
        {
            return X * b.X + Y * b.Y + Z * b.Z;
        }



        public static float Dot(Vector3D a, Vector3D b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }



        [Pure]
        public Vector3D Cross(Vector3D b)
        {
            return new Vector3D(Y * b.Z - Z * b.Y, Z * b.X - X * b.Z, X * b.Y - Y * b.X);
        }



        public static Vector3D Cross(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
        }



        /// <summary>
        /// Calculates the angle, in radians, between this and the given vector.
        /// </summary>
        /// <param name="b">The vector against with the angle should be calculated.</param>
        /// <returns>The angle between the 2 vectors, in radians.</returns>
        public float AngleTo(Vector3D b)
        {
            float f = Dot(b) / (Length * b.Length);

            return (float) Math.Acos(Math.Round(f, PrecisionDigits));
        }



        public float AngleToOrdered(Vector3D b, Vector3D axis)
        {
            if (Cross(b).Dot(axis) > 0)
                return (float) (Math.PI + AngleTo(-b));

            return AngleTo(b);
        }



/*public float AngleAroundAxisTo(Vector3D axis, Vector3D b)
                {
                    float f = Dot(b) / (Length * b.Length);
        
                    return (float)Math.Acos(Math.Round(f, PrecisionDigits));
                }*/



        public Vector3D Rotate(Vector3D axis, float angle)
        {
            return (this - axis * (axis * this)) * (float) Math.Cos(angle)
                   + axis.Cross(this) * (float) Math.Sin(angle) +
                   axis * (axis * this);
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

                throw new Exception("No valid coordinate '" + coordinate + "' exists.");
            }
        }



        public Vector3D Normalize()
        {
            float len = Length;

            return this / len;
        }



        public Vector3D ProjectToPlane(Vector3D planeNormal)
        {
            var sideDirection = Cross(planeNormal);

            var planeDirection = planeNormal.Cross(sideDirection).Normalize();

            return planeDirection * Dot(this, planeDirection);
        }



        public override bool Equals(object obj)
        {
            if (obj is Vector3D)
            {
                var v = (Vector3D) obj;

                return Math.Abs(X - v.X) < Precision && Math.Abs(Y - v.Y) < Precision &&
                       Math.Abs(Z - v.Z) < Precision;
            }

            return false;
        }



        public bool AproxEquals(Vector3D v, float precision)
        {
            return Math.Abs(X - v.X) < precision && Math.Abs(Y - v.Y) < precision &&
                   Math.Abs(Z - v.Z) < precision;
        }



        public bool ExactlyEquals(Vector3D v)
        {
            return X == v.X && Y == v.Y && Z == v.Z;
        }



        public override int GetHashCode()
        {
            return base.GetHashCode();
        }



        public object Clone()
        {
            return new Vector3D(X, Y, Z);
        }



        public static bool operator ==(Vector3D a, Vector3D b)
        {
            return a.ExactlyEquals(b); //a.Length == b.Length
        }



        public static bool operator !=(Vector3D a, Vector3D b)
        {
            return !a.ExactlyEquals(b); //a.Length != b.Length
        }



        public static Vector3D Lerp(Vector3D v1, Vector3D v2, float value)
        {
            return v1 + (v2 - v1) * value;
        }



        public bool IsCollinear(Vector3D v1)
        {
            return IsCollinear(this, v1);
        }



/*public static float Precision
                {
                    get { return _precision; }
                }*/



        public Vector3D Round()
        {
            return new Vector3D((float) Math.Round(X, PrecisionDigits), (float) Math.Round(Y, PrecisionDigits), (float) Math.Round(Z, PrecisionDigits));
        }



        public Vector3D Round(int decimalCases)
        {
            return new Vector3D((float) Math.Round(X, decimalCases), (float) Math.Round(Y, decimalCases), (float) Math.Round(Z, decimalCases));
        }



        public static Vector3D Average(IEnumerable<Vector3D> vectors)
        {
            var vector3Ds = vectors as Vector3D[] ?? vectors.ToArray();

            return vector3Ds.Aggregate((x, xresult) => x + xresult) / vector3Ds.Length;
        }



        public static Vector3D Average(params Vector3D[] vectors)
        {
            return Average((IEnumerable<Vector3D>) vectors);
        }



        public static Vector3D CalculateNormal(IEnumerable<Vector3D> vectorEnumerable)
        {
            List<Vector3D> vectors = vectorEnumerable.ToList();
            Vector3D normal = Zero;

            for (int i = 0; i < vectors.Count; i++)
            {
                Vector3D pn = vectors[i];
                Vector3D pn1 = vectors.GetCircular(i + 1);

                float x = (pn.Y - pn1.Y) * (pn.Z + pn1.Z);
                float y = (pn.Z - pn1.Z) * (pn.X + pn1.X);
                float z = (pn.X - pn1.X) * (pn.Y + pn1.Y);

                normal += new Vector3D(x, y, z);
            }

            normal = normal.Normalize();

            //since the wire direction is clockwise
            normal *= -1;

            return normal;
        }



        public bool IsCollinear(Vector3D v2, float tolerance)
        {
            return Cross(Normalize(), v2.Normalize()).Length < tolerance;
        }



        public static bool IsCollinear(Vector3D v1, Vector3D v2)
        {
            v1 = v1.Round();
            v2 = v2.Round();

            //float dot = v1.Dot(v2);
            //return dot < Precision;

            /*if (v1.Equals(Zero) || v2.Equals(Zero))
                return false;
            
            if((v1.X == 0 || v2.X == 0) && v1.X!= v2.X)
                return false;

            if((v1.Y == 0 || v2.Y == 0) && v1.Y!= v2.Y)
                return false;

            if((v1.Z == 0 || v2.Z == 0) && v1.Z!= v2.Z)
                return false;

            float valX = v2.X == 0 ? 0 : v1.X / v2.X;
            float valY = v2.Y == 0 ? 0 : v1.Y / v2.Y;
            float valZ = v2.Z == 0 ? 0 : v1.Z / v2.Z;

            return (valX == valY) && (valY == valZ);*/
            Vector3D vector3D = Cross(v1, v2);

            return Cross(v1, v2).Equals(Zero);
        }



        public override string ToString()
        {
            //return string.Format("X: {0}, Y: {1}, Z: {2}", _x.ToString(CultureInfo.InvariantCulture), _y.ToString(CultureInfo.InvariantCulture), _z.ToString(CultureInfo.InvariantCulture));
            return string.Format("v({0},{1},{2})", X.ToString(CultureInfo.InvariantCulture), Y.ToString(CultureInfo.InvariantCulture), Z.ToString(CultureInfo.InvariantCulture));
        }



        /// <summary>
        /// Multiplies the vector by the given scalar.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static Vector3D Scale(Vector3D v, float value)
        {
            return v * value;
        }



        public static Vector3D Maximize(Vector3D v1, Vector3D v2)
        {
            float x = Math.Max(v1.X, v2.X);
            float y = Math.Max(v1.Y, v2.Y);
            float z = Math.Max(v1.Z, v2.Z);

            return new Vector3D(x, y, z);
        }



        public static Vector3D Minimize(Vector3D v1, Vector3D v2)
        {
            float x = Math.Min(v1.X, v2.X);
            float y = Math.Min(v1.Y, v2.Y);
            float z = Math.Min(v1.Z, v2.Z);

            return new Vector3D(x, y, z);
        }



        public static float GetCommonMultiplier(Vector3D v1, Vector3D v2)
        {
            float a = 0;

            if (v2.X != 0)
                a = v1.X / v2.X;
            else if (v2.Y != 0)
                a = v1.Y / v2.Y;
            else if (v2.Z != 0)
                a = v1.Z / v2.Z;

            return a;
        }



        public static int LongAxis(ref Vector3D v)
        {
            int i = 0;
            if (Math.Abs(v.Y) > Math.Abs(v.X)) i = 1;
            if (Math.Abs(v.Z) > Math.Abs(i == 0 ? v.X : v.Y)) i = 2;
            return i;
        }



        public Vector2D ToVector2D()
        {
            return new Vector2D(X, Y);
        }



        public bool IsOrthogonal(Vector3D normal)
        {
            return Math.Abs(Dot(normal) - 0) < float.Epsilon;
        }



        public static bool IsOrthogonal(Vector3D v1, Vector3D v2)
        {
            return v1.IsOrthogonal(v2);
        }



        public static Vector3D New(float x, float y, float z)
        {
            return new Vector3D(x, y, z);
        }



        /// <summary>
        /// Replaces the components that might have Infinity or NaN as values and replaces them with the given value.
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public Vector3D MakeValid(float defaultValue = 0)
        {
            float x = float.IsNaN(X) || float.IsInfinity(X) ? defaultValue : X;
            float y = float.IsNaN(Y) || float.IsInfinity(Y) ? defaultValue : Y;
            float z = float.IsNaN(Z) || float.IsInfinity(Z) ? defaultValue : Z;

            return new Vector3D(x, y, z);
        }



        /// <summary>
        /// Replaces the components that might have values equal to the given value and replaces them with a second value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="replacementValue"></param>
        /// <returns></returns>
        public Vector3D ReplaceValue(float value, float replacementValue)
        {
            float x = Math.Abs(X - value) < float.Epsilon ? replacementValue : X;
            float y = Math.Abs(Y - value) < float.Epsilon ? replacementValue : Y;
            float z = Math.Abs(Z - value) < float.Epsilon ? replacementValue : Z;

            return new Vector3D(x, y, z);
        }



        public float[] ToArray()
        {
            return new[] {X, Y, Z};
        }



        public Vector3D FlipYZ()
        {
            return new Vector3D(X, Z, Y);
        }



        public Vector3D FlipXY()
        {
            return new Vector3D(Y, X, Z);
        }



        public Vector3D FlipXZ()
        {
            return new Vector3D(Z, Y, X);
        }



        /// <summary>
        /// x becomes y, y becomes z, z becomes x
        /// </summary>
        /// <returns></returns>
        public Vector3D FlipXZYRight()
        {
            return new Vector3D(Z, X, Y);
        }



        /// <summary>
        /// x becomes z, y becomes x, z becomes y
        /// </summary>
        /// <returns></returns>
        public Vector3D FlipXZYLeft()
        {
            return new Vector3D(Y, Z, X);
        }

        #endregion


        #region Operator Overloading

        public static Vector3D operator *(Vector3D a, float scalar)
        {
            return new Vector3D(a.X * scalar, a.Y * scalar, a.Z * scalar);
        }



        public static Vector3D operator *(float scalar, Vector3D a)
        {
            return new Vector3D(scalar * a.X, scalar * a.Y, scalar * a.Z);
        }



        public static Vector3D operator /(Vector3D a, float scalar)
        {
            return new Vector3D(a.X / scalar, a.Y / scalar, a.Z / scalar);
        }



        public static Vector3D operator %(Vector3D a, float scalar)
        {
            return new Vector3D(a.X % scalar, a.Y % scalar, a.Z % scalar);
        }



        public static Vector3D operator +(Vector3D a, float scalar)
        {
            return new Vector3D(a.X + scalar, a.Y + scalar, a.Z + scalar);
        }



        public static Vector3D operator -(Vector3D a, float scalar)
        {
            return new Vector3D(a.X - scalar, a.Y - scalar, a.Z - scalar);
        }



        public static Vector3D operator -(Vector3D a)
        {
            return new Vector3D(-a.X, -a.Y, -a.Z);
        }



        public static Vector3D operator *(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }



        public static Vector3D operator /(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        }



        public static Vector3D operator +(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }



        public static Vector3D operator -(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }



        public static Vector3D operator %(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X % b.X, a.Y % b.Y, a.Z % b.Z);
        }



        public static bool operator <=(Vector3D a, Vector3D b)
        {
            return a.Length <= b.Length;
        }



        public static bool operator >=(Vector3D a, Vector3D b)
        {
            return a.Length >= b.Length;
        }



        public static bool operator <(Vector3D a, Vector3D b)
        {
            return a.Length < b.Length;
        }



        public static bool operator >(Vector3D a, Vector3D b)
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


        public float Length => (float) Math.Sqrt(SquareLength);


        public float SquareLength => X * X + Y * Y + Z * Z;


        public bool IsNumericallyZero => Length < Precision;


        public bool IsNaN => float.IsNaN(X) || float.IsNaN(Y) || float.IsNaN(Z);



        public Vector3D PerpendicularVector
        {
            get
            {
                if (IsCollinear(ZVector))
                    return Cross(XVector);

                return Cross(ZVector);
            }
        }



        /// <summary>
        /// Indicates if any of the 3 Coordinates evaluates to positive/negative infinity.
        /// </summary>
        public bool IsInfinity => float.IsInfinity(X) || float.IsInfinity(Y) || float.IsInfinity(Z);


        /// <summary>
        /// Indicates if any of the 3 Coordinates evaluates to either positive/negative infinity or NaN.
        /// </summary>
        public bool IsInfinityOrNaN => IsInfinity || IsNaN;

        #endregion

        #region Static Properties

        /// <summary>
        /// A vector with the three Components X,Y,Z set to 1.
        /// </summary>
        public static Vector3D One => new Vector3D(1, 1, 1);


        /// <summary>
        /// A vector with the three Components X,Y,Z set to 0.
        /// </summary>
        public static Vector3D Zero => new Vector3D(0, 0, 0);


        /// <summary>
        /// A vector with the X Component as 1 and the others as 0 .
        /// </summary>
        public static Vector3D XVector => new Vector3D(1, 0, 0);


        /// <summary>
        /// A vector with the Y Component as 1 and the others as 0 .
        /// </summary>
        public static Vector3D YVector => new Vector3D(0, 1, 0);


        /// <summary>
        /// A vector with the Z Component as 1 and the others as 0 .
        /// </summary>
        public static Vector3D ZVector => new Vector3D(0, 0, 1);


        /// <summary>
        /// A vector with the three Components X,Y,Z set to Positive Infinity.
        /// Do not use this for comparison, instead use IsInfinity.
        /// </summary>
        public static Vector3D Infinity => new Vector3D(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);


        /// <summary>
        /// A vector with the three Components X,Y,Z set to NaN.
        /// /// Do not use this for comparison, instead use IsNaN.
        /// </summary>
        public static Vector3D NaN => new Vector3D(float.NaN, float.NaN, float.NaN);



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

        #region Comparers

        private sealed class XYZEqualityComparer : IEqualityComparer<Vector3D>
        {
            public bool Equals(Vector3D x, Vector3D y)
            {
                return x.X.Equals(y.X) && x.Y.Equals(y.Y) && x.Z.Equals(y.Z);
            }



            public int GetHashCode(Vector3D obj)
            {
                unchecked
                {
                    int hashCode = obj.X.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Y.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Z.GetHashCode();
                    return hashCode;
                }
            }
        }


        public static IEqualityComparer<Vector3D> XYZComparer
        {
            get;
        } = new XYZEqualityComparer();

        #endregion



        public string ToString(string format, IFormatProvider provider = null)
        {
            provider = provider ?? CultureInfo.InvariantCulture;

            return string.Format(format, X.ToString(provider), Y.ToString(provider), Z.ToString(provider));
        }
    }

    /*public class Vector3DConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Vector3D vector3D = (Vector3D)value;

            writer.WriteValue(vector3D.X.ToString(CultureInfo.InvariantCulture) + " " + vector3D.Y.ToString(CultureInfo.InvariantCulture) + " " + vector3D.Z.ToString(CultureInfo.InvariantCulture));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string readAsString = reader.ReadAsString();

            string[] strings = readAsString.Split(' ');


            return new Vector3D(Convert.ToSingle(strings[0], CultureInfo.InvariantCulture),
                                Convert.ToSingle(strings[1], CultureInfo.InvariantCulture),
                                Convert.ToSingle(strings[2], CultureInfo.InvariantCulture));
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }
    }*/
}