using System;
using System.Linq;
using Sceelix.Core;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;
using Sceelix.Loading;

namespace Sceelix.Documentation
{
    class Program
    {
        static void Main(string[] args)
        {
            String binFolder = @"";
            String destinationFolder = @"Node Reference";
            
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("/bin:"))
                    binFolder = args[i].SplitInTwo(':')[1];

                if (args[i].StartsWith("/destination:"))
                    destinationFolder = args[i].SplitInTwo(':')[1];
            }
            
            //load the assemblies
            SceelixDomain.LoadAssembliesFrom(binFolder);

            //initialize the engine
            EngineManager.Initialize();
            

            var procedureDocumenters = SystemProcedureManager.SystemProcedureTypes.Select(type => new ProcedureDocumenter(type)).Where(documenter => !documenter.IsObsolete);
            var groups = procedureDocumenters.GroupBy(x => x.Category).OrderBy(x => x.Key);
            foreach (IGrouping<string, ProcedureDocumenter> documenters in groups)
            {
                foreach (ProcedureDocumenter procedureDocumenter in documenters)
                {
                    procedureDocumenter.Generate(destinationFolder + "/" + documenters.Key);
                }
            }
        }
    }
}