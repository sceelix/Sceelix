using System;
using System.Collections;
using System.Collections.Generic;

namespace Sceelix.Core.Attributes
{
    [Obsolete("Check if still makes sense")]
    public class AttributeMetaKeyManager
    {
        private static readonly List<IMetaParser> _managers = new List<IMetaParser>();



        public static IEnumerable<int> GetCodesOfMetaManager<T>(BitArray array) where T : IMetaParser
        {
            for (int i = 0; i < array.Count; i++)
                if (array[i] && _managers[i].GetType() == typeof(T))
                    yield return i;
        }



        public static IMetaParser GetManager(int code)
        {
            return _managers[code];
        }



        public static IEnumerable<IMetaParser> GetManagers(BitArray array)
        {
            for (int i = 0; i < array.Count; i++)
                if (array[i])
                    yield return _managers[i];
        }



        public static int GetNewCode(IMetaParser parser)
        {
            _managers.Add(parser);

            return _managers.Count - 1;
        }



        public static bool HasKey(BitArray array, int code)
        {
            return array.Count > code && array[code];
        }



        /*public static BitArray ToBinary(this int numeral)
        {
            return new BitArray(new[] { numeral });
        }

        public static int ToNumeral(this BitArray binary)
        {
            if (binary == null)
                throw new ArgumentNullException("binary");
            if (binary.Length > 32)
                throw new ArgumentException("must be at most 32 bits long");

            var result = new int[1];
            binary.CopyTo(result, 0);
            return result[0];
        }*/
    }
}