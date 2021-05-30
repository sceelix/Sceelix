using Sceelix.Conversion;
using Sceelix.Core.Annotations;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Graphs.Functions
{
    [ExpressionFunctions("Vector")]
    public class VectorFunctions
    {
        /*public static Object Vector2(dynamic x, dynamic y)
        {
            return new SceeList(new KVPair("X", x), new KVPair("Y", y));
        }

        public static Object Vector3(dynamic x, dynamic y, dynamic z)
        {
            return new SceeList(new KVPair("X", x), new KVPair("Y", y), new KVPair("Z", z));
        }

        public static Object Vector4(dynamic x, dynamic y, dynamic z, dynamic w)
        {
            return new SceeList(new KVPair("X", x), new KVPair("Y", y), new KVPair("Z", z), new KVPair("W", w));
        }*/



        public static object Vector2(dynamic value)
        {
            return ConvertHelper.Convert<Vector2D>(value);
        }



        public static object Vector2(dynamic x, dynamic y)
        {
            return new Vector2D(ConvertHelper.Convert<float>(x), ConvertHelper.Convert<float>(y));
        }



        public static object Vector3(dynamic value)
        {
            return ConvertHelper.Convert<Vector3D>(value);
        }



        public static object Vector3(dynamic x, dynamic y, dynamic z)
        {
            return new Vector3D(ConvertHelper.Convert<float>(x), ConvertHelper.Convert<float>(y), ConvertHelper.Convert<float>(z));
        }



        public static object Vector3_Dot(dynamic v1, dynamic v2)
        {
            return Vector3D.Dot(v1, v2);
        }



        public static object Vector3_Length(dynamic v1)
        {
            return v1.Length;
        }
    }
}