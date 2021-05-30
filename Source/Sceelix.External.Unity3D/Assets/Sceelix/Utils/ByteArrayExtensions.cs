using System;

namespace Assets.Sceelix.Utils
{
    public static class ByteArrayExtensions
    {
        public static T[,] ToTArray<T>(this byte[] byteArray, int sizeX, int sizeY) where T : struct
        {
            var otherArray = new T[sizeX,sizeY];
            Buffer.BlockCopy(byteArray, 0, otherArray, 0, byteArray.Length);

            return otherArray;
        }

        public static T[,,] ToTArray<T>(this byte[] byteArray, int sizeX, int sizeY, int sizeZ) where T : struct
        {
            var otherArray = new T[sizeX, sizeY, sizeZ];
            Buffer.BlockCopy(byteArray, 0, otherArray, 0, byteArray.Length);

            return otherArray;
        }
    }
}