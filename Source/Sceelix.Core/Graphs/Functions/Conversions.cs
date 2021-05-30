using Sceelix.Conversion;
using Sceelix.Core.Annotations;

namespace Sceelix.Core.Graphs.Functions
{
    [ExpressionFunctions("Conversions")]
    public class Conversions
    {
        public static dynamic Bool(dynamic val)
        {
            return ConvertHelper.Convert(val, typeof(bool));
        }



        public static dynamic Byte(dynamic val)
        {
            return ConvertHelper.Convert(val, typeof(byte));
        }



        public static dynamic Double(dynamic val)
        {
            return ConvertHelper.Convert(val, typeof(double));
        }



        public static dynamic Float(dynamic val)
        {
            return ConvertHelper.Convert(val, typeof(float));
        }



        public static dynamic Int(dynamic val)
        {
            return ConvertHelper.Convert(val, typeof(int));
        }



        public static dynamic Long(dynamic val)
        {
            return ConvertHelper.Convert(val, typeof(long));
        }



        public static dynamic Short(dynamic val)
        {
            return ConvertHelper.Convert(val, typeof(short));
        }



        public static dynamic String(dynamic val)
        {
            return ConvertHelper.Convert(val, typeof(string));
        }
    }
}