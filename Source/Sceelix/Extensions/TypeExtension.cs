using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sceelix.Loading;

namespace Sceelix.Extensions
{
    public static class TypeExtension
    {
        private static readonly Dictionary<Type, List<FieldInfo>> FieldInfoCache = new Dictionary<Type, List<FieldInfo>>();



        /// <summary>
        /// Gets the custom attribute (or null, if it does not exist).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static T GetCustomAttribute<T>(this Type type) where T : Attribute
        {
            try
            {
                object firstOrDefault = type.GetCustomAttributes(true).FirstOrDefault(val => val is T);
                if (firstOrDefault != null)
                    return (T) firstOrDefault;
            }
            catch (Exception ex)
            {
                SceelixDomain.Logger.Log(ex);
            }

            return null;
        }



        public static Attribute GetCustomAttribute(this Type type, Type attributeType)
        {
            try
            {
                object firstOrDefault = type.GetCustomAttributes(true).FirstOrDefault(val => attributeType.IsInstanceOfType(val));
                if (firstOrDefault != null)
                    return (Attribute) firstOrDefault;
            }
            catch (Exception ex)
            {
                SceelixDomain.Logger.Log(ex);
            }

            return null;
        }



        public static IEnumerable<FieldInfo> GetInstancePublicPrivateFields(this Type type)
        {
            List<FieldInfo> fieldInfos;

            //if (!FieldInfoCache.TryGetValue(type, out fieldInfos))
            {
                fieldInfos = new List<FieldInfo>();

                var currentType = type;
                while (currentType != null)
                {
                    fieldInfos.InsertRange(0, currentType.GetFields(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic));
                    currentType = currentType.BaseType;
                }

                //FieldInfoCache.Add(type, fieldInfos);
            }


            return fieldInfos;
        }



        public static IEnumerable<Type> GetInterfaces(this Type type, bool includeInherited)
        {
            if (includeInherited || type.BaseType == null)
                return type.GetInterfaces();

            return type.GetInterfaces().Except(type.BaseType.GetInterfaces());
        }



        //private readonly static Dictionary<Type, List<Func<,Object>>> FieldAccessorsCache = new Dictionary<Type, List<FieldInfo>>();



        /// <summary>
        /// Determines whether the given type has an attribute of the indicated type.
        /// </summary>
        /// <typeparam name="T">The attribute type to look for.</typeparam>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the given type has the specified type; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasCustomAttribute<T>(this Type type) where T : Attribute
        {
            return type.HasCustomAttribute(typeof(T));
        }



        /// <summary>
        /// Determines whether the given type has an attribute of the indicated type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="attributeType">Type of the attribute to look for.</param>
        /// <returns>
        ///   <c>True</c> if the given type has the specified attribute type; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasCustomAttribute(this Type type, Type attributeType)
        {
            try
            {
                return type.GetCustomAttributes(true).Any(x => attributeType.IsInstanceOfType(x));
            }
            catch (Exception ex)
            {
                SceelixDomain.Logger.Log(ex);
            }

            return false;
        }



        /// <summary>
        /// Performs a foreach-like iteration over the elements of the enumerable, performing the given action, and catching exceptions if they occur and 
        /// simply logging them to the defined domain logger.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">The enumerable to .</param>
        /// <param name="action">The action to apply to all the enumerable items.</param>
        public static void SafeForeach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T value in enumerable)
                try
                {
                    action(value);
                }
                catch (Exception ex)
                {
                    SceelixDomain.Logger.Log(ex);
                }
        }



        /*public static IEnumerable<T> GetValuesOfFields<T>()
        {
            Expression.Field()
            Expression.Field(Expression.Parameter(typeof(T)),)
        } */
    }
}