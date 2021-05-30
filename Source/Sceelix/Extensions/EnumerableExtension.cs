using System;
using System.Collections.Generic;
using System.Linq;

namespace Sceelix.Extensions
{
    public static class EnumerableExtension
    {
        public static bool ContainsAll<T>(this IEnumerable<T> enumeration, IEnumerable<T> secondEnumeration)
        {
            return secondEnumeration.Except(enumeration).Any();
        }



        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration) action(item);
        }



        /// <summary>
        /// Selects the maximum value of the collection.
        /// 
        /// Unlike IEnumerable.Aggregate, this function does not run the function in minselection twice, which may be important if that function is heavy.
        /// Calls 2 times ToList(), though.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TR"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="minselection"></param>
        /// <returns></returns>
        public static KeyValuePair<T, TR> SelectMax<T, TR>(this IEnumerable<T> enumerable, Func<T, TR> maxselection)
        {
            List<T> list = enumerable.ToList();

            List<TR> values = list.Select(maxselection).ToList();

            TR minValue = values.Max();

            return new KeyValuePair<T, TR>(list[values.IndexOf(minValue)], minValue);
        }



        /// <summary>
        /// Selects the minimum value of the collection.
        /// 
        /// Unlike IEnumerable.Aggregate, this function does not run the function in minselection twice, which may be important if that function is heavy.
        /// Calls 2 times ToList(), though.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TR"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="minselection"></param>
        /// <returns></returns>
        public static KeyValuePair<T, TR> SelectMin<T, TR>(this IEnumerable<T> enumerable, Func<T, TR> minselection)
        {
            List<T> list = enumerable.ToList();

            List<TR> values = list.Select(minselection).ToList();

            TR minValue = values.Min();

            return new KeyValuePair<T, TR>(list[values.IndexOf(minValue)], minValue);
        }
    }
}