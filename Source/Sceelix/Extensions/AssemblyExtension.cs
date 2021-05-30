using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sceelix.Extensions
{
    public static class AssemblyExtension
    {
        /*public static T GetCustomAttribute<T>(this Assembly assembly) where T : Attribute
        {
            object firstOrDefault = assembly.GetCustomAttributes(true).FirstOrDefault(val => val is T);
            if (firstOrDefault != null)
                return (T) firstOrDefault;

            return null;
        }



        public static IEnumerable<T> GetCustomAttributes<T>(this Assembly assembly) where T : Attribute
        {
            return assembly.GetCustomAttributes(true).Where(val => val is T).Cast<T>();
        }*/



        public static IEnumerable<Type> GetSubclassesOf(this Assembly assembly, Type interfaceOrSuperclass)
        {
            return assembly.GetTypes().Where(interfaceOrSuperclass.IsAssignableFrom);
        }



        public static bool HasCustomAttribute<T>(this Assembly assembly) where T : Attribute
        {
            //return assembly.GetCustomAttributesData().Any(x => typeof(T).IsAssignableFrom(x.Constructor.DeclaringType));

            return assembly.GetCustomAttributes(true).Any(val => typeof(T).IsInstanceOfType(val));
        }



        public static bool HasCustomAttribute(this Assembly assembly, Type attribute)
        {
            //return assembly.GetCustomAttributesData().Any(attribute.IsInstanceOfType());

            //return assembly.GetCustomAttributes(true).Any(val => val is T);
            return assembly.GetCustomAttributes(true).Any(val => attribute.IsInstanceOfType(val));
        }



        public static bool HasCustomAttribute(this Assembly assembly, string attributeName)
        {
            return assembly.GetCustomAttributes(true).Any(val => val.GetType().Name == attributeName);
        }
    }
}