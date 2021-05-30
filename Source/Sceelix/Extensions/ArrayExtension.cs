using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Sceelix.Extensions
{
    public static class ArrayExtension
    {
        public static T GetNormalized<T>(this T[] array, int index)
        {
            return array[array.GetNormalizedIndex(index)];
        }



        public static int GetNormalizedIndex<T>(this T[] array, int index)
        {
            int count = array.Length;

            if (index < 0)
                return count + index % count;
            if (index < count) //most cases will probably fit this case anyway, requiring only 2 simple conditions to reach it
                return index;

            return index % count;
        }



        public static byte[] ToByteArray<T>(this T[] otherArray) where T : struct
        {
            var size = Marshal.SizeOf(default(T));

            var byteArray = new byte[otherArray.Length * size];
            Buffer.BlockCopy(otherArray, 0, byteArray, 0, byteArray.Length);

            return byteArray;
        }



        public static byte[] ToByteArray<T>(this T[,] otherArray) where T : struct
        {
            var size = Marshal.SizeOf(default(T));

            var byteArray = new byte[otherArray.GetLength(0) * otherArray.GetLength(1) * size];
            Buffer.BlockCopy(otherArray, 0, byteArray, 0, byteArray.Length);

            return byteArray;
        }



        public static byte[] ToByteArray<T>(this T[,,] otherArray) where T : struct
        {
            var size = Marshal.SizeOf(default(T));

            var byteArray = new byte[otherArray.GetLength(0) * otherArray.GetLength(1) * otherArray.GetLength(2) * size];
            Buffer.BlockCopy(otherArray, 0, byteArray, 0, byteArray.Length);

            return byteArray;
        }



        public static IEnumerable<T> ToEnumerable<T>(this Array array)
        {
            foreach (var item in array)
                yield return (T) item;
        }



        public static IEnumerable<T> ToEnumerable<T>(this T[,] array)
        {
            int sizeI = array.GetLength(0);
            int sizeJ = array.GetLength(1);

            for (int i = 0; i < sizeI; i++)
            for (int j = 0; j < sizeJ; j++)
                yield return array[i, j];
        }



        public static T[] ToTArray<T>(this byte[] byteArray) where T : struct
        {
            var size = Marshal.SizeOf(default(T));

            var otherArray = new T[byteArray.Length / size];
            Buffer.BlockCopy(byteArray, 0, otherArray, 0, byteArray.Length);

            return otherArray;
        }



        public static T[,] ToTArray<T>(this byte[] byteArray, int sizeX, int sizeY) where T : struct
        {
            var otherArray = new T[sizeX, sizeY];
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