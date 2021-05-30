using System;
using Sceelix.Collections;
using Sceelix.Conversion;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Conversions
{
    [ConversionFunctions]
    public class SceeListConversions
    {
        public static BoxScope SceelistToBoxScope(SceeList sceelist)
        {
            if (sceelist.IsAssociative)
                return new BoxScope(
                    ConvertHelper.Convert<Vector3D>(sceelist["XAxis"]),
                    ConvertHelper.Convert<Vector3D>(sceelist["YAxis"]),
                    ConvertHelper.Convert<Vector3D>(sceelist["ZAxis"]),
                    ConvertHelper.Convert<Vector3D>(sceelist["Translation"]),
                    ConvertHelper.Convert<Vector3D>(sceelist["Sizes"]));
            return new BoxScope(ConvertHelper.Convert<Vector3D>(sceelist[0]),
                ConvertHelper.Convert<Vector3D>(sceelist[1]),
                ConvertHelper.Convert<Vector3D>(sceelist[2]),
                ConvertHelper.Convert<Vector3D>(sceelist[3]),
                ConvertHelper.Convert<Vector3D>(sceelist[4]));
        }



        /// <summary>
        /// Converts a List to a struct of type Color.
        /// </summary>
        /// <param name="sceelist"></param>
        /// <returns></returns>
        public static Color SceelistToColorConversion(SceeList sceelist)
        {
            if (sceelist.IsAssociative)
            {
                var red = sceelist["Red"] ?? 255;
                var green = sceelist["Green"] ?? 255;
                var blue = sceelist["Blue"] ?? 255;
                var alpha = sceelist["Alpha"] ?? 255;

                return new Color(ConvertHelper.Convert<byte>(red), ConvertHelper.Convert<byte>(green), ConvertHelper.Convert<byte>(blue), ConvertHelper.Convert<byte>(alpha));
            }

            if (sceelist.Count >= 3)
                return new Color(ConvertHelper.Convert<byte>(sceelist[0]), ConvertHelper.Convert<byte>(sceelist[1]), ConvertHelper.Convert<byte>(sceelist[2]));
            if (sceelist.Count >= 4) return new Color(ConvertHelper.Convert<byte>(sceelist[0]), ConvertHelper.Convert<byte>(sceelist[1]), ConvertHelper.Convert<byte>(sceelist[2]), ConvertHelper.Convert<byte>(sceelist[3]));

            throw new Exception("Could not convert 'Sceelist' to 'Color'.");
        }



        public static Vector2D SceelistToVector2DConversion(SceeList sceelist)
        {
            if (sceelist.IsAssociative)
                return new Vector2D(
                    ConvertHelper.Convert<float>(sceelist["X"]),
                    ConvertHelper.Convert<float>(sceelist["Y"]));

            return new Vector2D(
                ConvertHelper.Convert<float>(sceelist[0]),
                ConvertHelper.Convert<float>(sceelist[1]));
        }



        public static Vector3D SceelistToVectorConversion(SceeList sceelist)
        {
            if (sceelist.IsAssociative)
                return new Vector3D(
                    ConvertHelper.Convert<float>(sceelist["X"]),
                    ConvertHelper.Convert<float>(sceelist["Y"]),
                    ConvertHelper.Convert<float>(sceelist["Z"]));

            return new Vector3D(
                ConvertHelper.Convert<float>(sceelist[0]),
                ConvertHelper.Convert<float>(sceelist[1]),
                ConvertHelper.Convert<float>(sceelist[2]));
        }
    }
}