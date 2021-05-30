using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Collections;
using Sceelix.Conversion;

namespace Sceelix.Extensions
{
    public static class ObjectExtension
    {
        public static T CastTo<T>(this object obj)
        {
            return (T) obj;
        }



        public static T Clone<T>(this object obj)
        {
            return (T) obj.Clone();
        }



        public static object Clone(this object obj)
        {
            if (obj is ICloneable)
                return ((ICloneable) obj).Clone();

            if (obj is ValueType)
                return obj;

            return obj;
        }



        public static T ConvertTo<T>(this object obj)
        {
            return (T) ConvertHelper.Convert(obj, typeof(T));
        }



        public static bool IsIn<T>(this T obj, IEnumerable<T> list)
        {
            return list.Contains(obj);
        }



        public static object MergeWith(this object obj, object otherObj)
        {
            if (obj is IMergeable)
                return ((IMergeable) obj).MergeWith(otherObj);

            return obj;
        }



        public static string SafeToString(this object obj)
        {
            if (obj != null)
                return obj.ToString();

            return string.Empty;
        }
    }
}