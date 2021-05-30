using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using BoundingBox = Sceelix.Mathematics.Spatial.BoundingBox;
using Color = Microsoft.Xna.Framework.Color;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector2D = Sceelix.Mathematics.Data.Vector2D;
using Vector3D = Sceelix.Mathematics.Data.Vector3D;

namespace Sceelix.Designer.Actors.Utils
{
    public static class XnaConvert
    {
        public static Matrix ZUpToYUpRotationMatrix = Matrix.CreateRotationX(-MathHelper.PiOver2);
        public static Matrix YUpToZUpRotationMatrix = Matrix.CreateRotationX(MathHelper.PiOver2);



        /// <summary>
        /// Converts a Sceelix Vector3D to a XNA Vector3
        /// </summary>
        /// <param name="vector3D">Sceelix Vector3D</param>
        /// <param name="rotate">If true, a rotation will be performed to compensate for the difference in Z-Up (Sceelix) and Y-Up (XNA)</param>
        /// <returns>XNA Vector3</returns>
        public static Vector3 ToVector3(this Vector3D vector3D, bool rotate = true)
        {
            if(rotate)
                return Vector3.Transform(new Vector3(vector3D.X, vector3D.Y, vector3D.Z), ZUpToYUpRotationMatrix);

            return new Vector3(vector3D.X, vector3D.Y, vector3D.Z);
        }



        /// <summary>
        /// Converts a XNA Vector3 to Sceelix Vector3D
        /// </summary>
        /// <param name="vector3">XNA Vector3</param>
        /// <param name="rotate">If true, a rotation will be performed to compensate for the difference in Y-Up (XNA) and Z-Up (Sceelix)</param>
        /// <returns>Sceelix Vector3D</returns>
        public static Vector3D ToVector3D(this Vector3 vector3, bool rotate = true)
        {
            if (rotate)
                vector3 = Vector3.Transform(vector3, YUpToZUpRotationMatrix);


            return new Vector3D(vector3.X, vector3.Y, vector3.Z);
        }



        /// <summary>
        /// Converts a Sceelix Vector2D to a XNA Vector2
        /// </summary>
        /// <param name="vectorUv">Sceelix Vector2D</param>
        /// <returns>XNA Vector2</returns>
        public static Vector2 ToVector2(this Vector2D vectorUv)
        {
            return new Vector2(vectorUv.X, vectorUv.Y);
        }



        /// <summary>
        /// Converts a Sceelix Color to a XNA Color
        /// </summary>
        /// <param name="color">Sceelix Color</param>
        /// <returns>XNA Color</returns>
        public static Microsoft.Xna.Framework.Color ToXnaColor(this Mathematics.Data.Color color)
        {
            return new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
        }



        /// <summary>
        /// Converts a Sceelix Color to a XNA Vector
        /// </summary>
        /// <param name="color">Sceelix Color</param>
        /// <returns>XNA Vector</returns>
        public static Microsoft.Xna.Framework.Vector3 ToXnaVector(this Mathematics.Data.Color color)
        {
            return new Microsoft.Xna.Framework.Vector3(color.R, color.G, color.B);
        }



        /// <summary>
        /// Converts a XNA Color to a Sceelix Color 
        /// </summary>
        /// <param name="color">XNA Color</param>
        /// <returns>Sceelix Color</returns>
        public static Mathematics.Data.Color ToSceelixColor(this Color color)
        {
            return new Mathematics.Data.Color(color.R, color.G, color.B, color.A);
        }



        /// <summary>
        /// Converts a Sceelix Boundingbox into a Xna BoundingBox
        /// </summary>
        /// <param name="boundingBox"></param>
        /// <returns></returns>
        public static Microsoft.Xna.Framework.BoundingBox ToXnaBoundingBox(this BoundingBox boundingBox)
        {
            return new Microsoft.Xna.Framework.BoundingBox(boundingBox.Min.ToVector3(), boundingBox.Max.ToVector3());
        }
    }
}