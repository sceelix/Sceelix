using System;

namespace Sceelix.Extensions
{
    public static class RandomExtension
    {
        public static bool Bool(this Random random)
        {
            return random.NextDouble() > 0.5;
        }



        public static byte Byte(this Random random)
        {
            return (byte) random.Next(256);
        }



        public static byte Byte(this Random random, int max)
        {
            return (byte) random.Next(max);
        }



        public static byte Byte(this Random random, int min, int max)
        {
            return (byte) random.Next(min, max);
        }



        /*public double Double()
        {
            return _randomGenerator.NextDouble();
        }*/



        public static double Double(this Random random, double max)
        {
            return random.NextDouble() * max;
        }



        public static double Double(this Random random, double min, double max)
        {
            return min + random.NextDouble() * (max - min);
        }



        /*public float Float()
        {
            return Convert.ToSingle(_randomGenerator.NextDouble());
        }*/



        public static float Float(this Random random, float min, float max)
        {
            return Convert.ToSingle(min + random.NextDouble() * (max - min));
        }



        public static int Int(this Random random)
        {
            return random.Next();
        }



        public static int Int(this Random random, int max)
        {
            return random.Next(max);
        }



        public static int Int(this Random random, int min, int max)
        {
            return random.Next(min, max);
        }
    }
}