using System;
using Sceelix.Core.Annotations;

namespace Sceelix.Core.Graphs.Functions
{
    [ExpressionFunctions("Math")]
    public class MathFunctions
    {
        public static object Abs(dynamic number)
        {
            return Math.Abs(number);
        }



        public static object Acos(dynamic number)
        {
            return Math.Acos(number);
        }



        public static object Asin(dynamic number)
        {
            return Math.Asin(number);
        }



        public static object Atan(dynamic number)
        {
            return Math.Atan(number);
        }



        public static object Atan2(dynamic numberX, dynamic numberY)
        {
            return Math.Atan2(numberX, numberY);
        }



        public static object Ceiling(dynamic number)
        {
            return Math.Ceiling(number);
        }



        public static dynamic ClosestPowerOf2(dynamic x)
        {
            return Math.Pow(2, Math.Round(Math.Log(x) / Math.Log(2)));
        }



        public static object Cos(dynamic number)
        {
            return Math.Cos(number);
        }



        public static object Cosh(dynamic o1)
        {
            return Math.Cosh(o1);
        }



        public static object Deg(dynamic radians)
        {
            return radians * 180f / Math.PI;
        }



        public static object E()
        {
            return Math.E;
        }



        public static object Exp(dynamic o1)
        {
            return Math.Exp(o1);
        }



        public static object Floor(dynamic o1)
        {
            return Math.Floor(o1);
        }



        public static dynamic IsEven(dynamic x)
        {
            return Math.Round((decimal) x) % 2 == 0;
        }



        public static dynamic IsOdd(dynamic x)
        {
            return Math.Round((decimal) x) % 2 != 0;
        }



        public static object Log(dynamic o1, dynamic o2)
        {
            return Math.Log(o1);
        }



        public static object Log10(dynamic o1)
        {
            return Math.Log10(o1);
        }



        public static object Max(dynamic o1, dynamic o2)
        {
            return Math.Max(o1, o2);
        }



        public static object Min(dynamic o1, dynamic o2)
        {
            return Math.Min(o1, o2);
        }



        public static dynamic NextPowerOf2(dynamic x)
        {
            return Math.Pow(2, Math.Ceiling(Math.Log(x) / Math.Log(2)));
        }



        public static object Pi()
        {
            return Math.PI;
        }



        public static object PiOver2()
        {
            return Math.PI / 2f;
        }



        public static object PiOver4()
        {
            return Math.PI / 4f;
        }



        public static object Pow(dynamic o1, dynamic o2)
        {
            return Math.Pow(o1, o2);
        }



        public static dynamic PreviousPowerOf2(dynamic x)
        {
            return Math.Pow(2, Math.Floor(Math.Log(x) / Math.Log(2)));
        }



        public static object Rad(dynamic degrees)
        {
            return degrees * Math.PI / 180f;
        }



        public static object Round(dynamic o1, dynamic o2)
        {
            return Math.Round(o1, o2);
        }



        public static object Sign(dynamic o1)
        {
            return Math.Sign(o1);
        }



        public static object Sin(dynamic o1)
        {
            return Math.Sin(o1);
        }



        public static object Sinh(dynamic o1)
        {
            return Math.Sinh(o1);
        }



        public static object Sqrt(dynamic o1)
        {
            return Math.Sqrt(o1);
        }



        public static object Tan(dynamic o1)
        {
            return Math.Tan(o1);
        }



        public static object Tanh(dynamic o1)
        {
            return Math.Tanh(o1);
        }



        public static object Truncate(dynamic o1)
        {
            return Math.Truncate(o1);
        }
    }
}