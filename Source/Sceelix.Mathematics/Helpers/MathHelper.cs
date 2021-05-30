using System;

namespace Sceelix.Mathematics.Helpers
{
    /// <summary>
    /// A class with several static mathematical functions - an extension to the "Math" class.
    /// </summary>
    public class MathHelper
    {
        /// <summary>
        /// The value of Pi.
        /// </summary>
        public const double Pi = 3.14159265358979323846264338327950f;


        /// <summary>
        /// The value of Pi, divided by 2.
        /// </summary>
        public const double PiOver2 = Pi / 2d;


        /// <summary>
        /// The value of Pi, divided by 4.
        /// </summary>
        public const double PiOver4 = Pi / 4d;



        /// <summary>
        /// Barycentric Interpolation, applicable to quads, assuming a division from (xMax,yMin) to (xMin, yMax).
        /// </summary>
        /// <param name="fractionX">The fraction x.</param>
        /// <param name="fractionY">The fraction y.</param>
        /// <param name="topLeft">The top left.</param>
        /// <param name="topRight">The top right.</param>
        /// <param name="bottomLeft">The bottom left.</param>
        /// <param name="bottomRight">The bottom right.</param>
        /// <returns></returns>
        public static float BarycentricMaxX(float fractionX, float fractionY, float topLeft, float topRight, float bottomLeft, float bottomRight)
        {
            if (fractionX < 1 - fractionY) ////the upper left triangle
                return fractionX * topRight + fractionY * bottomLeft + (1 - fractionX - fractionY) * topLeft;

            //the lower triangle
            return (1 - fractionY) * topRight + (1 - fractionX) * bottomLeft + (fractionY - (1 - fractionX)) * bottomRight;
        }



        /// <summary>
        /// Barycentric Interpolation, applicable to quads, assuming a division from (xMin, yMin) to (xMax, yMax).
        /// </summary>
        /// <param name="fractionX">The fraction x.</param>
        /// <param name="fractionY">The fraction y.</param>
        /// <param name="topLeft">The top left.</param>
        /// <param name="topRight">The top right.</param>
        /// <param name="bottomLeft">The bottom left.</param>
        /// <param name="bottomRight">The bottom right.</param>
        /// <returns></returns>
        public static float BarycentricMinX(float fractionX, float fractionY, float topLeft, float topRight, float bottomLeft, float bottomRight)
        {
            if (fractionX < fractionY) //the bottom triangle
                return (1 - fractionY) * topLeft + fractionX * bottomRight + (fractionY - fractionX) * bottomLeft;

            //the upper triangle
            return (1 - fractionX) * topLeft + fractionY * bottomRight + (fractionX - fractionY) * topRight;
        }



        /// <summary>
        /// Performs bilinear interpolation between the indicated values.
        /// </summary>
        /// <param name="fractionX">The fraction x.</param>
        /// <param name="fractionY">The fraction y.</param>
        /// <param name="topLeft">The top left.</param>
        /// <param name="topRight">The top right.</param>
        /// <param name="bottomLeft">The bottom left.</param>
        /// <param name="bottomRight">The bottom right.</param>
        /// <returns></returns>
        public static float Billinear(float fractionX, float fractionY, float topLeft, float topRight, float bottomLeft, float bottomRight)
        {
            //http://stackoverflow.com/questions/22151994/2d-array-interpolation
            return (1 - fractionX) * ((1 - fractionY) * topLeft + fractionY * bottomLeft) + fractionX * ((1 - fractionY) * topRight + fractionY * bottomRight);
        }



        /// <summary>
        /// Performs bilinear interpolation between the indicated values.
        /// </summary>
        /// <param name="fractionX">The fraction x.</param>
        /// <param name="fractionY">The fraction y.</param>
        /// <param name="topLeft">The top left.</param>
        /// <param name="topRight">The top right.</param>
        /// <param name="bottomLeft">The bottom left.</param>
        /// <param name="bottomRight">The bottom right.</param>
        /// <returns></returns>
        public static double Billinear(double fractionX, double fractionY, double topLeft, double topRight, double bottomLeft, double bottomRight)
        {
            //http://stackoverflow.com/questions/22151994/2d-array-interpolation
            return (topLeft * (1 - fractionY) + bottomLeft * fractionY) * (1 - fractionX)
                   + (topRight * (1 - fractionY) + bottomRight * fractionY) * fractionX;
        }



        /// <summary>
        /// Performs a Catmull-Rom interpolation.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <param name="value3">The third value.</param>
        /// <param name="value4">The fourth value.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>Interpolated value.</returns>
        public static float CatmullRom(float value1, float value2, float value3, float value4, float amount)
        {
            // Obtained from the Monogame library: https://github.com/ManojLakshan/monogame/blob/master/MonoGame.Framework/MathHelper.cs
            // Using formula from http://www.mvps.org/directx/articles/catmull/
            // Internally using doubles not to lose precision
            double amountSquared = amount * amount;
            double amountCubed = amountSquared * amount;

            return (float) (0.5 * (2.0 * value2 + (value3 - value1) * amount + (2.0 * value1 - 5.0 * value2 + 4.0 * value3 - value4) * amountSquared + (3.0 * value2 - value1 - 3.0 * value3 + value4) * amountCubed));
        }



        public static float CeilingToFactor(float value, float factor)
        {
            return (float) Math.Ceiling(value / (double) factor) * factor;
        }



        /// <summary>
        /// Restricts a value to be within a specified range, clamping the value if outside that range.
        /// </summary>
        /// <param name="value">The value to be checked against the limits.</param>
        /// <param name="min">The lower limit.</param>
        /// <param name="max">The upper limit.</param>
        /// <returns>Min, if value is lower than min. Max, if it is higher. Otherwise, returns back the value.</returns>
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }



        /// <summary>
        /// Restricts a value to be within a specified range.
        /// </summary>
        /// <param name="value">The value to be checked against the limits.</param>
        /// <param name="min">The lower limit.</param>
        /// <param name="max">The upper limit.</param>
        /// <returns>Min, if value is lower than min. Max, if it is higher. Otherwise, returns back the value.</returns>
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }



        /// <summary>
        /// Restricts a value to be within a specified range.
        /// </summary>
        /// <param name="value">The value to be checked against the limits.</param>
        /// <param name="min">The lower limit.</param>
        /// <param name="max">The upper limit.</param>
        /// <returns>Min, if value is lower than min. Max, if it is higher. Otherwise, returns back the value.</returns>
        public static double Clamp(double value, double min, double max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }



        public static float FloorToFactor(float value, float factor)
        {
            return (float) Math.Floor(value / (double) factor) * factor;
        }



        /// <summary>
        /// Performs a Hermite spline interpolation.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="tangent1">Tangent for the first value.</param>
        /// <param name="value2">The second value.</param>
        /// <param name="tangent2">Tangent for the second value.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>Interpolated value.</returns>
        public static float Hermite(float value1, float tangent1, float value2, float tangent2, float amount)
        {
            // All transformed to double not to lose precision
            // Otherwise, for high numbers of param:amount the result is NaN instead of Infinity
            double v1 = value1, v2 = value2, t1 = tangent1, t2 = tangent2, s = amount, result;
            double sCubed = s * s * s;
            double sSquared = s * s;

            if (Math.Abs(amount) < float.Epsilon)
                result = value1;
            else if (Math.Abs(amount - 1f) < float.Epsilon)
                result = value2;
            else
                result = (2 * v1 - 2 * v2 + t2 + t1) * sCubed +
                         (3 * v2 - 3 * v1 - 2 * t1 - t2) * sSquared +
                         t1 * s +
                         v1;
            return (float) result;
        }



        /// <summary>
        /// Indicates if the given value is a power of two (2,4,8,16...).
        /// </summary>
        /// <param name="value">The value to be checked.</param>
        /// <returns>True the value is a power of two, false otherwise.</returns>
        public static bool IsPowerOfTwo(short value)
        {
            return value != 0 && (value & (value - 1)) == 0;
        }



        /// <summary>
        /// Indicates if the given value is a power of two (2,4,8,16...).
        /// </summary>
        /// <param name="value">The value to be checked.</param>
        /// <returns>True the value is a power of two, false otherwise.</returns>
        public static bool IsPowerOfTwo(int value)
        {
            return value != 0 && (value & (value - 1)) == 0;
        }



        /// <summary>
        /// Indicates if the given value is a power of two (2,4,8,16...).
        /// </summary>
        /// <param name="value">The value to be checked.</param>
        /// <returns>True the value is a power of two, false otherwise.</returns>
        public static bool IsPowerOfTwo(long value)
        {
            return value != 0 && (value & (value - 1)) == 0;
        }



        /// <summary>
        /// Performs a linear interpolation between two values.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>Interpolated value.</returns>
        public static float Lerp(float value1, float value2, float amount)
        {
            return value1 + (value2 - value1) * amount;
        }



        /// <summary>
        /// Restricts a value to be within a specified range, mirroring the value around the indicated min and max, if outside that range.
        /// </summary>
        /// <param name="value">The value to be checked against the limits.</param>
        /// <param name="min">The lower limit.</param>
        /// <param name="max">The upper limit.</param>
        /// <returns>The mirrored value if outside the bounds. Otherwise, returns back the value.</returns>
        public static int Mirror(int value, int min, int max)
        {
            if (value < min)
            {
                var difference = min - value;
                var size = max - min;
                var rest = difference % size;
                var division = difference / size;
                if (division % 2 == 0)
                    return max - (size - rest);
                return min + (size - rest);
            }


            if (value > max)
            {
                var difference = value - max;
                var size = max - min;
                var rest = difference % size;
                var division = difference / size;
                if (division % 2 == 0)
                    return max - rest;
                return min + rest;
            }

            return value;
        }



        /// <summary>
        /// Restricts a value to be within a specified range, mirroring the value around the indicated min and max, if outside that range.
        /// </summary>
        /// <param name="value">The value to be checked against the limits.</param>
        /// <param name="min">The lower limit.</param>
        /// <param name="max">The upper limit.</param>
        /// <returns>The mirrored value if outside the bounds. Otherwise, returns back the value.</returns>
        public static float Mirror(float value, float min, float max)
        {
            if (value < min)
            {
                var difference = min - value;
                var size = max - min;
                var rest = difference % size;
                var division = difference / size;
                if (division % 2 == 0)
                    return max - (size - rest);
                return min + (size - rest);
            }


            if (value > max)
            {
                var difference = value - max;
                var size = max - min;
                var rest = difference % size;
                var division = (int) Math.Truncate(difference / size);
                if (division % 2 == 0)
                    return max - rest;
                return min + rest;
            }

            return value;
        }



        /// <summary>
        /// Restricts a value to be within a specified range, repeating the value within the indicated min (inclusive) and max (exclusive), if outside that range.
        /// </summary>
        /// <param name="value">The value to be checked against the limits.</param>
        /// <param name="min">The lower limit.</param>
        /// <param name="max">The upper limit.</param>
        /// <returns>The repeated value if outside the bounds. Otherwise, returns back the value.</returns>
        public static int Repeat(int value, int min, int max)
        {
            if (value < min)
                return max + (value - min) % (max - min);
            if (value < max) //most cases will probably fit this case anyway, requiring only 2 simple conditions to reach it
                return value;

            return (value - min) % (max - min);
        }



        /// <summary>
        /// Restricts a value to be within a specified range, repeating the value within the indicated min and max, if outside that range.
        /// </summary>
        /// <param name="value">The value to be checked against the limits.</param>
        /// <param name="min">The lower limit.</param>
        /// <param name="max">The upper limit.</param>
        /// <returns>The repeated value if outside the bounds. Otherwise, returns back the value.</returns>
        public static float Repeat(float value, float min, float max)
        {
            if (value < min)
                return max + (value - min) % (max - min);
            if (value < max) //most cases will probably fit this case anyway, requiring only 2 simple conditions to reach it
                return value;

            return (value - min) % (max - min);
        }



        public static float RoundToFactor(float value, float factor)
        {
            return (float) Math.Round(value / (double) factor, MidpointRounding.AwayFromZero) * factor;
        }



        /// <summary>
        /// Converts a value from radians to degrees.
        /// </summary>
        /// <param name="radians">Value in radians.</param>
        /// <returns>Value in degrees</returns>
        public static float ToDegrees(float radians)
        {
            return radians * 180f / (float) Pi;
        }



        /// <summary>
        /// <summary>
        /// Converts a value from radians to degrees.
        /// </summary>
        /// <param name="radians">Value in radians.</param>
        /// <returns>Value in degrees</returns>
        public static double ToDegrees(double radians)
        {
            return radians * 180d / Pi;
        }



        /// <summary>
        /// Converts a value from degrees to radians.
        /// </summary>
        /// <param name="degrees">Value in degrees.</param>
        /// <returns>Value in radians</returns>
        public static float ToRadians(float degrees)
        {
            return degrees * (float) Pi / 180f;
        }



        /// <summary>
        /// Converts a value from degrees to radians.
        /// </summary>
        /// <param name="degrees">Value in degrees.</param>
        /// <returns>Value in radians</returns>
        public static double ToRadians(double degrees)
        {
            return degrees * Pi / 180d;
        }
    }
}