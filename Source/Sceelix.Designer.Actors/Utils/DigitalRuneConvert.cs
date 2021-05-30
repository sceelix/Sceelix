using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Sceelix.Designer.Utils;
using Vector2D = Sceelix.Mathematics.Data.Vector2D;
using Vector3D = Sceelix.Mathematics.Data.Vector3D;

namespace Sceelix.Designer.Actors.Utils
{
    public static class DigitalRuneConvert
    {
        /// <summary>
        /// Converts a Sceelix Vector3D to a DigitalRune Vector3F
        /// </summary>
        /// <param name="vector3D">Sceelix Vector3D</param>
        /// <param name="rotate"></param>
        /// <returns>XNA Vector3</returns>
        public static Vector3F ToVector3F(this Vector3D vector3D, bool rotate = true)
        {
            if(rotate)
                return DigitalRuneUtils.ZUpToYUpRotationMatrix * new Vector3F(vector3D.X, vector3D.Y, vector3D.Z);

            return new Vector3F(vector3D.X, vector3D.Y, vector3D.Z);
        }

        

        /// <summary>
        /// Converts a DigitalRune Vector3F to Sceelix Vector3D
        /// </summary>
        /// <param name="vector3">XNA Vector3</param>
        /// <returns>Sceelix Vector3D</returns>
        public static Vector3D ToSceelixVector3D(this Vector3 vector3)
        {
            return new Vector3D(vector3.X, vector3.Y, vector3.Z);
        }



        /// <summary>
        /// Converts a DigitalRune Vector3F to Sceelix Vector3D
        /// </summary>
        /// <param name="vector3">Digitalrune Vector3F</param>
        /// <returns>Sceelix Vector3D</returns>
        public static Vector3D ToSceelixVector3D(this Vector3F vector3)
        {
            vector3 = DigitalRuneUtils.YUpToZUpRotationMatrix*vector3;

            return new Vector3D(vector3.X, vector3.Y, vector3.Z);
        }



        /// <summary>
        /// Converts a Sceelix Vector2D to a XNA Vector2
        /// </summary>
        /// <param name="vectorUv">Sceelix Vector2D</param>
        /// <returns>XNA Vector2</returns>
        public static Vector2F ToVector2F(this Vector2D vectorUv)
        {
            return new Vector2F(vectorUv.X, vectorUv.Y);
        }
    }
}