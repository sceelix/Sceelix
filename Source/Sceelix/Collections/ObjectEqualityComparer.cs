using System.Collections.Generic;
using System.Linq;

namespace Sceelix.Collections
{
    public class ObjectArrayEqualityComparer : IEqualityComparer<object[]>
    {
        public bool Equals(object[] array1, object[] array2)
        {
            return array1.SequenceEqual(array2);
        }



        public int GetHashCode(object[] array)
        {
            // Implement some hash - this isn't great, but it'll work
            return array.Aggregate(array.Length, (current, i) => current ^ i.GetHashCode());
        }
    }
}