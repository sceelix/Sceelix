using Sceelix.Conversion;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Conversions
{
    [ConversionFunctions]
    public class StringConversions
    {
        public static Color StringToColor(string str)
        {
            var components = str.Split(',');
            return new Color(ConvertHelper.Convert<byte>(components[0]), ConvertHelper.Convert<byte>(components[1]), ConvertHelper.Convert<byte>(components[2]), ConvertHelper.Convert<byte>(components[3]));
        }



        public static Vector2D StringToVector2DConversion(string str)
        {
            var components = str.Split(',');
            return new Vector2D(ConvertHelper.Convert<float>(components[0]), ConvertHelper.Convert<float>(components[1]));
        }



        public static Vector3D StringToVector3DConversion(string str)
        {
            var components = str.Split(',');
            return new Vector3D(ConvertHelper.Convert<float>(components[0]), ConvertHelper.Convert<float>(components[1]), ConvertHelper.Convert<float>(components[2]));
        }
    }
}