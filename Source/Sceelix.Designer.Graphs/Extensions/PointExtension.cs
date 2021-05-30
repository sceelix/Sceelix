using System;
using Microsoft.Xna.Framework;

namespace Sceelix.Designer.Graphs.Extensions
{
    public static class PointExtension
    {
        public static Point Maximize(Point v1, Point v2)
        {
            Point pResult = new Point();

            pResult.X = Math.Max(v1.X, v2.X);
            pResult.Y = Math.Max(v1.Y, v2.Y);

            return pResult;
        }



        public static Point Minimize(Point v1, Point v2)
        {
            Point pResult = new Point();

            pResult.X = Math.Min(v1.X, v2.X);
            pResult.Y = Math.Min(v1.Y, v2.Y);

            return pResult;
        }



        public static Point Add(this Point v1, Point v2)
        {
            return new Point(v1.X + v2.X, v1.Y + v2.Y);
        }



        public static Point Subtract(this Point v1, Point v2)
        {
            return new Point(v1.X - v2.X, v1.Y - v2.Y);
        }



        /*public static System.Drawing.Point ToPoint(this Vector2 vector2)
        {
            return new System.Drawing.Point((int)vector2.X, (int)vector2.Y);
        }*/
    }
}