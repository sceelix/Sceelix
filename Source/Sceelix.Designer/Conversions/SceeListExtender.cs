using DigitalRune.Mathematics.Algebra;
using Sceelix.Collections;
using Sceelix.Conversion;
using Sceelix.Extensions;

namespace Sceelix.Designer.Conversions
{
    [ConversionFunctions]
    public static class SceeListExtender
    {
        public static Vector2F ToVector2F(this SceeList sceeList)
        {
            return new Vector2F(sceeList[0].ConvertTo<float>(), sceeList[1].ConvertTo<float>());
        }



        public static Vector3F ToVector3F(this SceeList sceeList)
        {
            return new Vector3F(sceeList[0].ConvertTo<float>(), sceeList[1].ConvertTo<float>(), sceeList[2].ConvertTo<float>());
        }



        public static Vector4F ToVector4F(this SceeList sceeList)
        {
            return new Vector4F(sceeList[0].ConvertTo<float>(), sceeList[1].ConvertTo<float>(), sceeList[2].ConvertTo<float>(), sceeList[3].ConvertTo<float>());
        }
    }
}