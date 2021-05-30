using System;

namespace Sceelix.Mathematics.Data
{
    public struct Matrix
    {
        public Matrix(float m11, float m12, float m13, float m14,
            float m21, float m22, float m23, float m24,
            float m31, float m32, float m33, float m34,
            float m41, float m42, float m43, float m44)
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M14 = m14;
            M21 = m21;
            M22 = m22;
            M23 = m23;
            M24 = m24;
            M31 = m31;
            M32 = m32;
            M33 = m33;
            M34 = m34;
            M41 = m41;
            M42 = m42;
            M43 = m43;
            M44 = m44;
        }



        public override string ToString()
        {
            return string.Format("|{0}, {1}, {2}, {3}|\n", M11, M12, M13, M14)
                   + string.Format("|{0}, {1}, {2}, {3}|\n", M21, M22, M23, M24)
                   + string.Format("|{0}, {1}, {2}, {3}|\n", M31, M32, M33, M34)
                   + string.Format("|{0}, {1}, {2}, {3}|\n", M41, M42, M43, M44);
        }



        public float Trace()
        {
            return M11 + M22 + M33 + M44;
        }



        public Matrix Transpose =>
            new Matrix(M11, M21, M31, M41,
                M12, M22, M32, M42,
                M13, M23, M33, M43,
                M14, M24, M34, M44);



        public static Matrix operator +(Matrix a, Matrix b)
        {
            return new Matrix(
                a.M11 + b.M11, a.M12 + b.M12, a.M13 + b.M13, a.M14 + b.M14,
                a.M21 + b.M21, a.M22 + b.M22, a.M23 + b.M23, a.M24 + b.M24,
                a.M31 + b.M31, a.M32 + b.M32, a.M33 + b.M33, a.M34 + b.M34,
                a.M41 + b.M41, a.M42 + b.M42, a.M43 + b.M43, a.M44 + b.M44
            );
        }



        public static Matrix operator +(Matrix a, float s)
        {
            return new Matrix(
                a.M11 + s, a.M12 + s, a.M13 + s, a.M14 + s,
                a.M21 + s, a.M22 + s, a.M23 + s, a.M24 + s,
                a.M31 + s, a.M32 + s, a.M33 + s, a.M34 + s,
                a.M41 + s, a.M42 + s, a.M43 + s, a.M44 + s
            );
        }



        public static Matrix operator +(float s, Matrix a)
        {
            return a + s;
        }



        public static Matrix operator -(Matrix a, Matrix b)
        {
            return new Matrix(
                a.M11 - b.M11, a.M12 - b.M12, a.M13 - b.M13, a.M14 - b.M14,
                a.M21 - b.M21, a.M22 - b.M22, a.M23 - b.M23, a.M24 - b.M24,
                a.M31 - b.M31, a.M32 - b.M32, a.M33 - b.M33, a.M34 - b.M34,
                a.M41 - b.M41, a.M42 - b.M42, a.M43 - b.M43, a.M44 - b.M44
            );
        }



        public static Matrix operator -(Matrix a, float s)
        {
            return new Matrix(
                a.M11 - s, a.M12 - s, a.M13 - s, a.M14 - s,
                a.M21 - s, a.M22 - s, a.M23 - s, a.M24 - s,
                a.M31 - s, a.M32 - s, a.M33 - s, a.M34 - s,
                a.M41 - s, a.M42 - s, a.M43 - s, a.M44 - s
            );
        }



        public static Matrix operator *(Matrix a, Matrix b)
        {
            return new Matrix(a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41,
                a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42,
                a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43,
                a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44,
                a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41,
                a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42,
                a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43,
                a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44,
                a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41,
                a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42,
                a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43,
                a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44,
                a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41,
                a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42,
                a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43,
                a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44);
        }



        public Vector3D Transform(Vector3D vector)
        {
            return new Vector3D(
                M11 * vector.X + M12 * vector.Y + M13 * vector.Z + M14,
                M21 * vector.X + M22 * vector.Y + M23 * vector.Z + M24,
                M31 * vector.X + M32 * vector.Y + M33 * vector.Z + M34);
        }



/*public static Vector3D Transform(Matrix matrix, Vector3D vector)
                {
                    return new Vector3D(
                        (matrix.M11 * vector.X) + (matrix.M12 * vector.Y) + (matrix.M13 * vector.Z) + matrix.M14,
                        (matrix.M21 * vector.X) + (matrix.M22 * vector.Y) + (matrix.M23 * vector.Z) + matrix.M24,
                        (matrix.M31 * vector.X) + (matrix.M32 * vector.Y) + (matrix.M33 * vector.Z) + matrix.M34);
                }*/



        public static Matrix CreateAxisAngle(Vector3D rotationAxis, float angle)
        {
            float sin = (float) Math.Sin(angle);
            float cos = (float) Math.Cos(angle);
            float cosInv = 1.0f - cos;

            float squareAxisX = rotationAxis.X * rotationAxis.X;
            float squareAxisY = rotationAxis.Y * rotationAxis.Y;
            float squareAxisZ = rotationAxis.Z * rotationAxis.Z;

            float xy = rotationAxis.X * rotationAxis.Y;
            float xz = rotationAxis.X * rotationAxis.Z;
            float yz = rotationAxis.Y * rotationAxis.Z;

            return new Matrix(cos + squareAxisX * cosInv, xy * cosInv - rotationAxis.Z * sin, xz * cosInv + rotationAxis.Y * sin, 0.0f,
                xy * cosInv + rotationAxis.Z * sin, cos + squareAxisY * cosInv, yz * cosInv - rotationAxis.X * sin, 0.0f,
                xz * cosInv - rotationAxis.Y * sin, yz * cosInv + rotationAxis.X * sin, cos + squareAxisZ * cosInv, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f);
        }



        public float[] ToAngles()
        {
            float[] angles = new float[3];

            double sinPitch, cosPitch, sinRoll, cosRoll, sinYaw, cosYaw;

            sinPitch = -M31;
            cosPitch = Math.Sqrt(1 - sinPitch * sinPitch);

            if (Math.Abs(cosPitch) > float.Epsilon)
            {
                sinRoll = M32 / cosPitch;
                cosRoll = M33 / cosPitch;
                sinYaw = M21 / cosPitch;
                cosYaw = M11 / cosPitch;
            }
            else
            {
                sinRoll = -M23;
                cosRoll = M22;
                sinYaw = 0;
                cosYaw = 1;
            }


            //roll - x
            angles[0] = (float) (Math.Atan2(sinRoll, cosRoll) * 180 / Math.PI);

            //pitch - y
            angles[1] = (float) (Math.Atan2(sinPitch, cosPitch) * 180 / Math.PI);

            //yaw - z
            angles[2] = (float) (Math.Atan2(sinYaw, cosYaw) * 180 / Math.PI);

            return angles;
        }



        public Matrix Inverse
        {
            get
            {
                ///
                // Use Laplace expansion theorem to calculate the inverse of a 4x4 matrix
                // 
                // 1. Calculate the 2x2 determinants needed the 4x4 determinant based on the 2x2 determinants 
                // 3. Create the adjugate matrix, which satisfies: A * adj(A) = det(A) * I
                // 4. Divide adjugate matrix with the determinant to find the inverse

                float det1, det2, det3, det4, det5, det6, det7, det8, det9, det10, det11, det12;
                float detMatrix;
                FindDeterminants(ref this, out detMatrix, out det1, out det2, out det3, out det4, out det5, out det6,
                    out det7, out det8, out det9, out det10, out det11, out det12);

                float invDetMatrix = 1f / detMatrix;

                var m11 = (M22 * det12 - M23 * det11 + M24 * det10) * invDetMatrix;
                var m12 = (-M12 * det12 + M13 * det11 - M14 * det10) * invDetMatrix;
                var m13 = (M42 * det6 - M43 * det5 + M44 * det4) * invDetMatrix;
                var m14 = (-M32 * det6 + M33 * det5 - M34 * det4) * invDetMatrix;
                var m21 = (-M21 * det12 + M23 * det9 - M24 * det8) * invDetMatrix;
                var m22 = (M11 * det12 - M13 * det9 + M14 * det8) * invDetMatrix;
                var m23 = (-M41 * det6 + M43 * det3 - M44 * det2) * invDetMatrix;
                var m24 = (M31 * det6 - M33 * det3 + M34 * det2) * invDetMatrix;
                var m31 = (M21 * det11 - M22 * det9 + M24 * det7) * invDetMatrix;
                var m32 = (-M11 * det11 + M12 * det9 - M14 * det7) * invDetMatrix;
                var m33 = (M41 * det5 - M42 * det3 + M44 * det1) * invDetMatrix;
                var m34 = (-M31 * det5 + M32 * det3 - M34 * det1) * invDetMatrix;
                var m41 = (-M21 * det10 + M22 * det8 - M23 * det7) * invDetMatrix;
                var m42 = (M11 * det10 - M12 * det8 + M13 * det7) * invDetMatrix;
                var m43 = (-M41 * det4 + M42 * det2 - M43 * det1) * invDetMatrix;
                var m44 = (M31 * det4 - M32 * det2 + M33 * det1) * invDetMatrix;

                return new Matrix(m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44);
            }
        }



        /// <summary>
        /// Helper method for using the Laplace expansion theorem using two rows expansions to calculate major and 
        /// minor determinants of a 4x4 matrix. This method is used for inverting a matrix.
        /// </summary>
        private static void FindDeterminants(ref Matrix matrix, out float major,
            out float minor1, out float minor2, out float minor3, out float minor4, out float minor5, out float minor6,
            out float minor7, out float minor8, out float minor9, out float minor10, out float minor11, out float minor12)
        {
            double det1 = matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21;
            double det2 = matrix.M11 * matrix.M23 - matrix.M13 * matrix.M21;
            double det3 = matrix.M11 * matrix.M24 - matrix.M14 * matrix.M21;
            double det4 = matrix.M12 * matrix.M23 - matrix.M13 * matrix.M22;
            double det5 = matrix.M12 * matrix.M24 - matrix.M14 * matrix.M22;
            double det6 = matrix.M13 * matrix.M24 - matrix.M14 * matrix.M23;
            double det7 = matrix.M31 * matrix.M42 - matrix.M32 * matrix.M41;
            double det8 = matrix.M31 * matrix.M43 - matrix.M33 * matrix.M41;
            double det9 = matrix.M31 * matrix.M44 - matrix.M34 * matrix.M41;
            double det10 = matrix.M32 * matrix.M43 - matrix.M33 * matrix.M42;
            double det11 = matrix.M32 * matrix.M44 - matrix.M34 * matrix.M42;
            double det12 = matrix.M33 * matrix.M44 - matrix.M34 * matrix.M43;

            major = (float) (det1 * det12 - det2 * det11 + det3 * det10 + det4 * det9 - det5 * det8 + det6 * det7);
            minor1 = (float) det1;
            minor2 = (float) det2;
            minor3 = (float) det3;
            minor4 = (float) det4;
            minor5 = (float) det5;
            minor6 = (float) det6;
            minor7 = (float) det7;
            minor8 = (float) det8;
            minor9 = (float) det9;
            minor10 = (float) det10;
            minor11 = (float) det11;
            minor12 = (float) det12;
        }



        /// <summary>
        /// Creates a matrix with 
        /// </summary>
        /// <param name="angle">Angle, in radians.</param>
        /// <returns></returns>
        public static Matrix CreateRotateX(float angle)
        {
            float sin = (float) Math.Sin(angle);
            float cos = (float) Math.Cos(angle);


            return new Matrix(1.0f, 0.0f, 0.0f, 0.0f,
                0.0f, cos, -sin, 0.0f,
                0.0f, sin, cos, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f);
        }



        public static Matrix CreateRotateY(float angle)
        {
            float sin = (float) Math.Sin(angle);
            float cos = (float) Math.Cos(angle);

            return new Matrix(cos, 0.0f, sin, 0.0f,
                0.0f, 1.0f, 0.0f, 0.0f,
                -sin, 0.0f, cos, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f);
        }



        public static Matrix CreateRotateZ(float angle)
        {
            float sin = (float) Math.Sin(angle);
            float cos = (float) Math.Cos(angle);

            return new Matrix(cos, -sin, 0.0f, 0.0f,
                sin, cos, 0.0f, 0.0f,
                0.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f);
        }



        public static Matrix CreateScale(Vector3D vector)
        {
            return new Matrix(vector.X, 0.0f, 0.0f, 0.0f,
                0.0f, vector.Y, 0.0f, 0.0f,
                0.0f, 0.0f, vector.Z, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f);
        }



        public static Matrix CreateScale(float x, float y, float z)
        {
            return new Matrix(x, 0.0f, 0.0f, 0.0f,
                0.0f, y, 0.0f, 0.0f,
                0.0f, 0.0f, z, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f);
        }



        public static Matrix CreateTranslation(float x, float y, float z)
        {
            return new Matrix(1.0f, 0.0f, 0.0f, x,
                0.0f, 1.0f, 0.0f, y,
                0.0f, 0.0f, 1.0f, z,
                0.0f, 0.0f, 0.0f, 1.0f);
        }



        public static Matrix CreateTranslation(Vector3D vector)
        {
            return new Matrix(1.0f, 0.0f, 0.0f, vector.X,
                0.0f, 1.0f, 0.0f, vector.Y,
                0.0f, 0.0f, 1.0f, vector.Z,
                0.0f, 0.0f, 0.0f, 1.0f);
        }



        public float M11
        {
            get;
        }


        public float M12
        {
            get;
        }


        public float M13
        {
            get;
        }


        public float M14
        {
            get;
        }


        public float M21
        {
            get;
        }


        public float M22
        {
            get;
        }


        public float M23
        {
            get;
        }


        public float M24
        {
            get;
        }


        public float M31
        {
            get;
        }


        public float M32
        {
            get;
        }


        public float M33
        {
            get;
        }


        public float M34
        {
            get;
        }


        public float M41
        {
            get;
        }


        public float M42
        {
            get;
        }


        public float M43
        {
            get;
        }


        public float M44
        {
            get;
        }


        public static Matrix Zero => new Matrix(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);


        public static Matrix Identity => new Matrix(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
    }
}