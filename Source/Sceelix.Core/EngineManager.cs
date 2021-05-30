using System;
using System.Reflection;
using Sceelix.Conversion;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.Graphs.Functions;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;
using Sceelix.Loading;

namespace Sceelix.Core
{
    public class EngineManager
    {
        /// <summary>
        /// Controls the engine initialization, so that it happens only once.
        /// </summary>
        private static bool _isInitialized;



        /// <summary>
        /// Initializes procedure libraries, functions, entity types and parameters from all referenced assemblies on the application domain (except those dynamic and those in the global assembly cache). 
        /// Should be called once and only once, at the beginning of the application.
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
                return;

            try
            {
                ConvertHelper.Initialize();

                //initialize list of expression functions
                FunctionManager.Initialize();

                //initialize list of IEntity types
                EntityManager.Initialize();

                //initialize list of parameter types
                ParameterManager.Initialize();

                //initialize the list of system procedures
                SystemProcedureManager.Initialize();

                _isInitialized = true;
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (Exception inner in ex.LoaderExceptions) // write details of "inner", in particular inner.Message
                    SceelixDomain.Logger.Log(inner);
            }
        }



        /// <summary>
        /// Verifies if the given assembly is a valid engine library.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static bool IsEngineLibrary(Assembly assembly)
        {
            return assembly.HasCustomAttribute<EngineLibraryAttribute>();
        }
    }
}