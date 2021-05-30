using System;

namespace Sceelix.Extensions
{
    public static class FloatExtension
    {
        /// <summary>
        /// Calculates if two float values are approximately equal to each other (withn float.epsilon).
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool ApproximatelyEqualTo(this float a, float b)
        {
            return Math.Abs(a - b) < float.Epsilon;
        }



        /// <summary>
        /// Indicates if a is multiple of b.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsMultipleOf(this float a, float b)
        {
            return Math.Abs(a % b) < float.Epsilon;
        }
    }
}