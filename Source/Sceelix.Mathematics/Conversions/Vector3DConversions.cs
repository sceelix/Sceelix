using System.Collections.Generic;
using Sceelix.Collections;
using Sceelix.Conversion;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Conversions
{
    [ConversionFunctions]
    public class Vector3DConversions
    {
        public static SceeList ToSceeList(Vector3D vector3D)
        {
            return new SceeList(new KeyValuePair<string, object>("X", vector3D.X), new KeyValuePair<string, object>("Y", vector3D.Y), new KeyValuePair<string, object>("Z", vector3D.Z));
        }



        public static string ToString(Vector3D vector3D)
        {
            return string.Format("{0},{1},{2}", ConvertHelper.Convert<string>(vector3D.X), ConvertHelper.Convert<string>(vector3D.Y), ConvertHelper.Convert<string>(vector3D.Z));
        }
    }
}