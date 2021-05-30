using System;
using System.Collections.Generic;
using Sceelix.Conversion;

namespace Sceelix.Extensions
{
    public static class DictionaryExtension
    {
        public static TS GetAndConvert<T, TR, TS>(this Dictionary<T, TR> dictionary, T key, TS defaultConvertedValue)
        {
            TR value;
            if (dictionary.TryGetValue(key, out value))
                return ConvertHelper.Convert<TS>(value);

            return defaultConvertedValue;
        }



        public static T GetOrCompute<T, TR>(this Dictionary<TR, T> dictionary, TR key, Func<T> computeAction)
        {
            T value;
            if (!dictionary.TryGetValue(key, out value))
                dictionary.Add(key, value = computeAction());

            return value;
        }



        public static TR GetOrDefault<T, TR>(this Dictionary<T, TR> dictionary, T key)
        {
            TR value;
            if (dictionary.TryGetValue(key, out value))
                return value;

            return default(TR);
        }



        public static TR GetOrDefault<T, TR>(this Dictionary<T, TR> dictionary, T key, TR defaultValue)
        {
            TR value;
            if (dictionary.TryGetValue(key, out value))
                return value;

            return defaultValue;
        }
    }
}