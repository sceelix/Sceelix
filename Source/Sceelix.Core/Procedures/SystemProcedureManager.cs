using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Sceelix.Core.Annotations;
using Sceelix.Extensions;
using Sceelix.Loading;

namespace Sceelix.Core.Procedures
{
    public class SystemProcedureManager
    {
        /// <summary>
        /// Data for system procedure instantiation
        /// </summary>
        private static readonly Dictionary<string, Type> _systemProcedureTypes = new Dictionary<string, Type>();

        private static readonly Dictionary<Type, Func<SystemProcedure>> _systemProcedureCreators = new Dictionary<Type, Func<SystemProcedure>>();



        public static Dictionary<string, Type> SystemProcedureGuids
        {
            //delivers a copy
            get { return _systemProcedureTypes.ToDictionary(x => x.Key, y => y.Value); }
        }



        public static IEnumerable<Type> SystemProcedureTypes => _systemProcedureTypes.Values;



        public static T FromGuid<T>(string guid) where T : SystemProcedure
        {
            SystemProcedure proc = FromGuid(guid);
            if (proc != null)
                return (T) proc;

            return null;
        }



        public static SystemProcedure FromGuid(string guid)
        {
            Type type = TryGetType(guid);
            if (type != null)
                return _systemProcedureCreators[type]();

            return null;
        }



        internal static void Initialize()
        {
            SceelixDomain.Types.Where(x => typeof(SystemProcedure).IsAssignableFrom(x)).SafeForeach(procedureType =>
            {
                if (!procedureType.IsAbstract)
                {
                    //Console.WriteLine(assemblyType.Name);
                    ProcedureAttribute attributeForProcedure = ProcedureAttribute.GetAttributeForProcedure(procedureType);
                    if (attributeForProcedure != null)
                    {
                        if (_systemProcedureTypes.ContainsKey(attributeForProcedure.Guid))
                            throw new Exception(string.Format("The guid for the type '{0}' had already been assigned to type '{1}'.", procedureType, _systemProcedureTypes[attributeForProcedure.Guid]));

                        Guid guid;
                        if (!Guid.TryParse(attributeForProcedure.Guid, out guid))
                            throw new Exception(string.Format("The guid for the type '{0}' is invalid.", procedureType));

                        var creationFunction = Expression.Lambda<Func<SystemProcedure>>(Expression.New(procedureType)).Compile();
                        _systemProcedureCreators.Add(procedureType, creationFunction);

                        //var result = creationFunction();

                        _systemProcedureTypes.Add(attributeForProcedure.Guid, procedureType);
                    }
                }
            });
        }



        internal static Type TryGetType(string guid)
        {
            Type type;
            if (_systemProcedureTypes.TryGetValue(guid, out type))
            {
                return type;
            }

            return null;
        }
    }
}