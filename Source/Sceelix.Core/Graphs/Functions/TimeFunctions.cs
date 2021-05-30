using System;
using Sceelix.Core.Annotations;

namespace Sceelix.Core.Graphs.Functions
{
    [ExpressionFunctions("Utils")]
    public class TimeFunctions
    {
        public static dynamic Time()
        {
//Environment.TickCount
            return Guid.NewGuid().GetHashCode(); // / Int32.MaxValue;
        }
    }
}