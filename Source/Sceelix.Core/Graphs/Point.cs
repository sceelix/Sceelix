using System;
using System.Globalization;

namespace Sceelix.Core.Graphs
{
    public struct Point
    {
        public Point(float x, float y)
        {
            X = x;
            Y = y;
        }



        public static Point Parse(string pointString, CultureInfo cultureInfo)
        {
            string[] strings = pointString.Split(' ');

            return new Point(float.Parse(strings[0], cultureInfo), float.Parse(strings[1], cultureInfo));
        }



        public string ToString(CultureInfo cultureInfo)
        {
            return X.ToString(cultureInfo) + " " + Y.ToString(cultureInfo);
        }



        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }



        public static Point operator -(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }



        public float X
        {
            get;
            set;
        }


        public float Y
        {
            get;
            set;
        }



        public static Point Minimize(Point v1, Point v2)
        {
            float x = Math.Min(v1.X, v2.X);
            float y = Math.Min(v1.Y, v2.Y);

            return new Point(x, y);
        }
    }
}