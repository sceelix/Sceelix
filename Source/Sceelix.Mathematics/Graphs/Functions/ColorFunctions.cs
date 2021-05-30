using Sceelix.Collections;
using Sceelix.Conversion;
using Sceelix.Core.Annotations;
using Sceelix.Mathematics.Data;
using KVPair = System.Collections.Generic.KeyValuePair<string, object>;

namespace Sceelix.Mathematics.Graphs.Functions
{
    [ExpressionFunctions("Color")]
    public class ColorFunctions
    {
        public static object Color(dynamic red, dynamic green, dynamic blue)
        {
            return new Color(ConvertHelper.Convert<byte>(red), ConvertHelper.Convert<byte>(green), ConvertHelper.Convert<byte>(blue));
        }



        public static object Color(dynamic red, dynamic green, dynamic blue, dynamic alpha)
        {
            return new Color(ConvertHelper.Convert<byte>(red), ConvertHelper.Convert<byte>(green), ConvertHelper.Convert<byte>(blue), ConvertHelper.Convert<byte>(alpha));
        }



        /*public static Object Rgba(dynamic red, dynamic green, dynamic blue)
        {
            return new SceeList(new KVPair("Red", red), new KVPair("Green", green), new KVPair("Blue", blue), new KVPair("Alpha", (byte)255));
        }


        public static Object Rgba(dynamic red, dynamic green, dynamic blue, dynamic alpha)
        {
            return new SceeList(new KVPair("Red", red), new KVPair("Green", green), new KVPair("Blue", blue), new KVPair("Alpha", alpha));
        }*/



        public static object Hsva(dynamic hue, dynamic saturation, dynamic value)
        {
            Color color = Data.Color.HsvToRgb(hue, saturation, value);

            return new SceeList(new KVPair("Red", color.R), new KVPair("Green", color.G), new KVPair("Blue", color.B), new KVPair("Alpha", (byte) 255));
        }



        public static object Hsva(dynamic hue, dynamic saturation, dynamic value, dynamic alpha)
        {
            Color color = Data.Color.HsvToRgb(hue, saturation, value);

            return new SceeList(new KVPair("Red", color.R), new KVPair("Green", color.G), new KVPair("Blue", color.B), new KVPair("Alpha", alpha));
        }
    }
}