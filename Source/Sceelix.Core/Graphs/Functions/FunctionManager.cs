using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sceelix.Core.Annotations;
using Sceelix.Core.Extensions;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;
using Sceelix.Loading;

namespace Sceelix.Core.Graphs.Functions
{
    public static class FunctionManager
    {
        private static readonly List<StaticFunction> StaticFunctions = new List<StaticFunction>();



        /// <summary>
        ///Gets names of available, non-obsolete functions names, ordered by their class name (group).
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IGrouping<string, string>> GetRegisteredFunctionByGroup()
        {
            return StaticFunctions
                .Where(x => x.ObsoleteAttribute == null)
                .OrderBy(x => x.Name)
                .GroupBy(x => x.GroupName, x => x.ToString())
                .ToList();
        }



        /// <summary>
        ///Gets names of available, non-obsolete functions names.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetRegisteredFunctionNames()
        {
            return StaticFunctions
                .Where(x => x.ObsoleteAttribute == null)
                .Select(x => x.ToString());
        }



        /// <summary>
        /// Gets the static function object with the given name and number of arguments (whereas this number should exclude those that will need to be injected)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="argNumber"></param>
        /// <returns></returns>
        public static StaticFunction GetRegisteredStaticFunction(string name, int argNumber)
        {
            return StaticFunctions.FirstOrDefault(x => x.Name.ToLower() == name.ToLower() && x.ArgNumber == argNumber);
        }



        internal static void Initialize()
        {
            SceelixDomain.Types.Where(x => x.HasCustomAttribute<ExpressionFunctionsAttribute>()).SafeForeach(Register);
        }



        private static void Register(Type type)
        {
            var attribute = type.GetCustomAttribute<ExpressionFunctionsAttribute>();

            foreach (MethodInfo methodInfo in type.GetMethods().Where(x => x.IsStatic /*&& x.ReturnType == typeof(Object))*/))
            {
                var parameterInfos = methodInfo.GetParameters();

                //if (parameterInfos.All(x => x.ParameterType == typeof(Object)))
                {
                    StaticFunctions.Add(new StaticFunction(attribute.Name, methodInfo, methodInfo.Name, parameterInfos));
                }
            }
        }



        public class StaticFunction
        {
            public StaticFunction(string groupName, MethodInfo methodInfo, string name, ParameterInfo[] parameterInfos)
            {
                GroupName = groupName;
                MethodInfo = methodInfo;
                Name = name;
                ParameterInfos = parameterInfos;
                ArgNumber = ParameterInfos.Length;
                ObsoleteAttribute = methodInfo.GetCustomAttribute<ObsoleteAttribute>();

                //do not consider the first parameter, as it will be injected
                if (parameterInfos.Length > 0 && parameterInfos.First().ParameterType == typeof(Procedure))
                    ArgNumber--;
            }



            public int ArgNumber
            {
                get;
            }


            public string GroupName
            {
                get;
            }


            public MethodInfo MethodInfo
            {
                get;
            }


            public string Name
            {
                get;
            }


            public ObsoleteAttribute ObsoleteAttribute
            {
                get;
            }


            public ParameterInfo[] ParameterInfos
            {
                get;
            }



            public override string ToString()
            {
                return string.Format("{0}({1})", Name, string.Join(",", Enumerable.Range(0, ArgNumber).Select(x => "arg" + x)));
            }
        }
    }
}