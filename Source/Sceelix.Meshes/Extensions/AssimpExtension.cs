using Assimp;
using Sceelix.Mathematics.Data;
using Vector2D = Sceelix.Mathematics.Data.Vector2D;
using Vector3D = Sceelix.Mathematics.Data.Vector3D;

namespace Sceelix.Meshes.Extensions
{
    public static class AssimpExtension
    {
        public static Color ToSceelixColor(this Color3D color)
        {
            return new Color((byte) (color.R * 255), (byte) (color.B * 255), (byte) (color.G * 255));
        }



        public static Color ToSceelixColor(this Color4D color)
        {
            return new Color((byte) (color.R * 255), (byte) (color.B * 255), (byte) (color.G * 255), (byte) (color.A * 255));
        }



        public static Matrix ToSceelixMatrix(this Matrix4x4 matrix)
        {
            return new Matrix(matrix.A1, matrix.A2, matrix.A3, matrix.A4,
                matrix.B1, matrix.B2, matrix.B3, matrix.B4,
                matrix.C1, matrix.C2, matrix.C3, matrix.C4,
                matrix.D1, matrix.D2, matrix.D3, matrix.D4);
        }



        public static Vector2D ToSceelixVector2D(this Assimp.Vector2D vector)
        {
            return new Vector2D(vector.X, vector.Y);
        }



        public static Vector3D ToSceelixVector3D(this Assimp.Vector3D vector)
        {
            return new Vector3D(vector.X, vector.Y, vector.Z);
        }
    }
}