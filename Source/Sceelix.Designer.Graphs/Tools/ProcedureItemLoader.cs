using System.Collections.Generic;
using System.Linq;
using Sceelix.Core;
using Sceelix.Core.Procedures;

namespace Sceelix.Designer.Graphs.Tools
{
    public static class ProcedureItemLoader
    {
        /// <summary>
        /// Data for system procedure instantiation
        /// </summary>
        private static readonly List<SystemProcedureItem> _systemProcedureItems = new List<SystemProcedureItem>();
        


        /// <summary>
        /// Guids of the procedure types, to allow easy procedure reference correction
        /// </summary>
        //private static readonly Dictionary<String, Type> _procedureTypeGuids = new Dictionary<String, Type>();
        public static void Initialize()
        {
            //first step, load the core engine and its libraries
            EngineManager.Initialize();

            //second step, load the 
            _systemProcedureItems.AddRange(SystemProcedureManager.SystemProcedureTypes.Select(x => new SystemProcedureItem(x)));
        }


        public static List<SystemProcedureItem> SystemProcedureItems
        {
            get { return _systemProcedureItems; }
        }
    }
}