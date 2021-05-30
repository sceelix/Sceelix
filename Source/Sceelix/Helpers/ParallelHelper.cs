using System;
using System.Threading.Tasks;

namespace Sceelix.Helpers
{
    public class ParallelHelper
    {
        private static readonly ParallelOptions _defaultParallelOptions = new ParallelOptions {MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2)};



        public static void For(int fromInclusive, int toExclusive, Action<int> body)
        {
            Parallel.For(fromInclusive, toExclusive, _defaultParallelOptions, body);
        }
    }
}