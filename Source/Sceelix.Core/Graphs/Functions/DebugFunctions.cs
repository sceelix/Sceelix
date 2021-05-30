using Sceelix.Core.Annotations;
using Sceelix.Core.Procedures;
using Sceelix.Logging;

namespace Sceelix.Core.Graphs.Functions
{
    [ExpressionFunctions("Debug")]
    public class DebugFunctions
    {
        public static object Print(Procedure procedure, dynamic data)
        {
            procedure.Environment.GetService<ILogger>().Log(data.ToString());
            return data;
        }
    }
}