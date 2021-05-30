using System.Collections.Generic;
using Sceelix.Collections;
using Sceelix.Conversion;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Conversions
{
    [ConversionFunctions]
    public class ColorConversions
    {
        public static SceeList ColorToSceeList(Color color)
        {
            return new SceeList(new KeyValuePair<string, object>("Red", color.R), new KeyValuePair<string, object>("Green", color.G), new KeyValuePair<string, object>("Blue", color.B), new KeyValuePair<string, object>("Alpha", color.A));
        }



        public static string ToString(Color color)
        {
            return string.Format("{0},{1},{2},{3}", ConvertHelper.Convert<string>(color.R), ConvertHelper.Convert<string>(color.G), ConvertHelper.Convert<string>(color.B), ConvertHelper.Convert<string>(color.A));
        }
    }
}