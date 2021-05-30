using System;
using System.Collections.Generic;

namespace Sceelix.Extensions
{
    public static class ListExtension
    {
        public static T GetCircular<T>(this List<T> list, int index)
        {
            return list[list.GetCircularIndex(index)];
        }



        public static int GetCircularIndex<T>(this List<T> list, int index)
        {
            if (index < 0)
                return list.Count + index % list.Count;

            //most cases will probably fit this case anyway, requiring only 2 simple conditions to reach it
            if (index < list.Count)
                return index;

            return index % list.Count;

            //this expression would be simpler, but requires the expensive modulo operation every time
            //return index < 0 ? Count + index % Count : index % Count;
        }



        public static List<T> GetCircularRangeAt<T>(this List<T> list, int indexBegin, int indexEnd)
        {
            indexBegin = list.GetCircularIndex(indexBegin);
            indexEnd = list.GetCircularIndex(indexEnd);

            if (indexBegin > indexEnd)
            {
                List<T> vTemp = new List<T>(list.GetRange(indexBegin, list.Count - indexBegin));
                vTemp.AddRange(list.GetRange(0, indexEnd + 1));
                return vTemp;
            }

            return new List<T>(list.GetRange(indexBegin, indexEnd - indexBegin + 1));
        }



        public static void Shuffle<T>(this IList<T> list, int seed)
        {
            Random rng = new Random(seed);
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}