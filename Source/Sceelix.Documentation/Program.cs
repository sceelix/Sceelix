using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sceelix.Core;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;
//using Sceelix.Licensing;
using Sceelix.Loading;

namespace Sceelix.Documentation
{
    class Program
    {
        static void Main(string[] args)
        {
            String binFolder = @"E:\Sceelix\Development\Build\Windows64\Release\Bin";
            String destinationFolder = @"E:\Sceelix\Support\Docs\Node Reference";
            
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("/bin:"))
                    binFolder = args[i].SplitInTwo(':')[1];

                if (args[i].StartsWith("/destination:"))
                    destinationFolder = args[i].SplitInTwo(':')[1];
            }
            

            //dummy load - not needed because we are referencing a non-licensed version.
            //LicenseManager.LoadLicenseFromFile("");

            //load the assemblies
            SceelixDomain.LoadAssembliesFrom(binFolder);

            //initialize the engine
            EngineManager.Initialize();

            //we can now access the system procedures
            StringBuilder stringBuilder = new StringBuilder();

            var procedureDocumenters = SystemProcedureManager.SystemProcedureTypes.Select(type => new ProcedureDocumenter(type)).Where(documenter => !documenter.IsObsolete);
            var groups = procedureDocumenters.GroupBy(x => x.Category).OrderBy(x => x.Key);
            foreach (IGrouping<string, ProcedureDocumenter> documenters in groups)
            {

                stringBuilder.AppendLine("# [" + documenters.Key + "]()");
                foreach (ProcedureDocumenter procedureDocumenter in documenters)
                {
                    var tocEntry = procedureDocumenter.Generate(destinationFolder);
                    stringBuilder.AppendLine("## " + tocEntry);
                }
            }

            /*foreach (Type systemProcedureType in SystemProcedureManager.SystemProcedureTypes)
            {
                var procedureDocumenter = new ProcedureDocumenter(systemProcedureType);
                if (!procedureDocumenter.IsObsolete)
                {
                    var tocEntry = procedureDocumenter.Generate(destinationFolder);
                    stringBuilder.AppendLine(tocEntry);
                }
            }*/

            File.WriteAllText(Path.Combine(destinationFolder, "toc.md"), stringBuilder.ToString());
        }
    }
}