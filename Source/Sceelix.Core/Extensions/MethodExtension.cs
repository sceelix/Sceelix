using System;
using System.Linq;
using System.Reflection;

namespace Sceelix.Core.Extensions
{
    internal static class MethodExtension
    {
        public static T GetCustomAttribute<T>(this MemberInfo type) where T : Attribute
        {
            object firstOrDefault = type.GetCustomAttributes(true).FirstOrDefault(val => val is T);
            if (firstOrDefault != null)
                return (T) firstOrDefault;

            return null;
        }



        public static string GetMethodSignature(this MethodInfo methodInfo)
        {
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();

            return methodInfo.Name + "(" + string.Join(",", parameterInfos.Select(val => val.ParameterType.Name).ToArray()) + ")";
        }



        public static bool HasCustomAttribute<T>(this MemberInfo type) where T : Attribute
        {
            return type.GetCustomAttributes(true).Any(val => val is T);
        }
    }
}