using Sceelix.Core.Annotations;

namespace Sceelix.Core.Graphs.Functions
{
    [ExpressionFunctions("Conditional")]
    public class BasicFunctions
    {
        public static dynamic IfBool(dynamic condition, dynamic value1, dynamic value2)
        {
            if (condition)
                return value1;

            return value2;
        }



        public static dynamic IfNull(dynamic value1, dynamic value2)
        {
            return value1 ?? value2;
        }



        public static dynamic IsNull(dynamic value1)
        {
            return value1 == null;
        }



        public static dynamic Null()
        {
            return null;
        }
    }
}