using System.Collections.Generic;
using Sceelix.Collections;
using Sceelix.Core.Data;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.Graphs.Expressions
{
    internal static class ExpressionHelper
    {
        public static object Add(object o1, object o2)
        {
            return (dynamic) o1 + (dynamic) o2;
        }



        public static object And(object o1, object o2)
        {
            return (dynamic) o1 && (dynamic) o2;
        }



        public static object CreateKeyValue(dynamic key, dynamic value)
        {
            return new KeyValuePair<string, object>(key, value);
        }



        public static object CreateList(params object[] objs)
        {
            SceeList list = new SceeList();
            for (int i = 0; i < objs.Length; i++)
                if (objs[i] is KeyValuePair<string, object>)
                {
                    var keyValue = (KeyValuePair<string, object>) objs[i];
                    list.Add(keyValue.Key, keyValue.Value);
                }
                else
                {
                    list.Add(objs[i]);
                }

            return list;
        }



        public static object Divide(object o1, object o2)
        {
            return (dynamic) o1 / (dynamic) o2;
        }



        public static object Equal(object o1, object o2)
        {
            return (dynamic) o1 == (dynamic) o2;
        }



        public static object GreaterThan(object o1, object o2)
        {
            return (dynamic) o1 > (dynamic) o2;
        }



        public static object GreaterThanOrEqual(object o1, object o2)
        {
            return (dynamic) o1 >= (dynamic) o2;
        }



        public static object Index(object o1, object o2, object masterProcedure)
        {
            if (o1 == null)
                return null;
            if (o1 is IEntity && o2 is string)
            {
                var entity = (IEntity) o1;
                var key = (string) o2;

                var actualKey = AttributeParameter.GetKey(key, (Procedure) masterProcedure);
                //var actualKey = key.StartsWithLowerCase() ? (AttributeKey)new LocalAttributeKey(key, (Procedure) masterProcedure) : new GlobalAttributeKey(key);

                return entity.Attributes.TryGet(actualKey);
            }

            return ((dynamic) o1)[(dynamic) o2];
        }



        public static object LessThan(object o1, object o2)
        {
            return (dynamic) o1 < (dynamic) o2;
        }



        public static object LessThanOrEqual(object o1, object o2)
        {
            return (dynamic) o1 <= (dynamic) o2;
        }



        public static object Modulo(object o1, object o2)
        {
            return (dynamic) o1 % (dynamic) o2;
        }



        public static object Multiply(object o1, object o2)
        {
            return (dynamic) o1 * (dynamic) o2;
        }



        public static object Negate(object o1)
        {
            return -(dynamic) o1;
        }



        public static object Not(object o1)
        {
            /*if(o1 is bool)
                return !(dynamic)o1;

            //do like javascript does
            return o1 == null;*/

            return !(dynamic) o1;
        }



        public static object NotEqual(object o1, object o2)
        {
            return (dynamic) o1 != (dynamic) o2;
        }



        public static object Or(object o1, object o2)
        {
            return (dynamic) o1 || (dynamic) o2;
        }



        public static object Subtract(object o1, object o2)
        {
            return (dynamic) o1 - (dynamic) o2;
        }
    }
}