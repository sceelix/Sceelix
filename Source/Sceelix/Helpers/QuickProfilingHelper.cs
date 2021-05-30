using System;
using System.Diagnostics;

namespace Sceelix.Helpers
{
    public class QuickProfilingHelper
    {
        public static void CheckMemory(string identifier, Action action)
        {
            long m1 = GC.GetTotalMemory(true);

            action();

            long m2 = GC.GetTotalMemory(false);

            Console.WriteLine(identifier + " takes " + (m2 - m1) + " bytes.");
        }



        public static void CheckTime(string identifier, Action action)
        {
            Stopwatch watch = Stopwatch.StartNew();

            action();

            Console.WriteLine(identifier + " took " + watch.ElapsedMilliseconds + " ms.");
        }



        public static void CheckTime(string identifier, int numberTimes, Action action)
        {
            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < numberTimes; i++)
                action();

            Console.WriteLine(identifier + " took " + watch.ElapsedMilliseconds + " ms.");
        }
    }
}