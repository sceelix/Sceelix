using System.Collections.Generic;
using Sceelix.Collections;
using Sceelix.Conversion;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Conversions
{
    [ConversionFunctions]
    public class Vector2DConversions
    {
        public static SceeList ToSceeList(Vector2D vector2D)
        {
            return new SceeList(new KeyValuePair<string, object>("X", vector2D.X), new KeyValuePair<string, object>("Y", vector2D.Y));
        }



        public static string ToString(Vector2D vector2D)
        {
            return string.Format("{0},{1}", ConvertHelper.Convert<string>(vector2D.X), ConvertHelper.Convert<string>(vector2D.Y));
        }
    }
}