using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Sceelix.Mathematics.Data;

namespace Sceelix.Paths.Tests.Procedures
{
    public class PathSeparateAndMergeTests
    {
        [Test]
        public void SeparateAndMergePath()
        {
            var pathCreateProcedure = new Sceelix.Paths.Procedures.PathCreateProcedure();
            pathCreateProcedure.Parameters["Type"].Set("Single");
            pathCreateProcedure.Parameters["Type"].Parameters["Single"].Parameters["Points"].Set(new object[]
            {
                new Vector3D(0,0,0),
                new Vector3D(1,0,0),
                new Vector3D(3,5,0),
                new Vector3D(6,3,0),
                new Vector3D(8,8,0)
            });
            pathCreateProcedure.Execute();
            var multiPointPath = pathCreateProcedure.Outputs["Output"].Dequeue();


            var pathDivideProcedure = new Sceelix.Paths.Procedures.PathDivideProcedure();
            pathDivideProcedure.Parameters["Separate"].Set("Separate");
            {
                var separateParameter = pathDivideProcedure.Parameters["Separate"].Parameters["Separate"];
                separateParameter.Parameters["Attributes"].Set("Parent and Edge");
                separateParameter.Parameters["Scope"].Set("Parent");
            }
            pathDivideProcedure.Inputs["Input"].Enqueue(multiPointPath);
            pathDivideProcedure.Execute();
            var separatedPaths = pathDivideProcedure.Outputs["Output"].DequeueAll().ToList();
            Assert.AreEqual(4, separatedPaths.Count());


            var pathMergeProcedure = new Sceelix.Paths.Procedures.PathMergeProcedure();
            pathMergeProcedure.Parameters["Input"].Set("Collective");
            {
                var collectiveParameter = pathMergeProcedure.Parameters["Input"].Parameters["Collective"];
                collectiveParameter.Inputs["Collective"].Enqueue(separatedPaths);
            }
            pathMergeProcedure.Parameters["Scope Selection"].Set("First");
            pathMergeProcedure.Execute();

            var outputData = pathMergeProcedure.Outputs["Output"].DequeueAll();
            Assert.AreEqual(1, outputData.Count());
        }
    }
}
