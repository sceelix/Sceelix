using System.Linq;
using NUnit.Framework;
using Sceelix.Core.Attributes;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Helpers;

namespace Sceelix.Meshes.Tests.Procedures
{

    public class MeshSplitProcedureTests
    {
        [Test]
        public void SplitCube()
        {
            var meshSplitProcedure = new Sceelix.Meshes.Procedures.MeshSplitProcedure();
            meshSplitProcedure.Parameters["Split Axis"].Set("Scope Axis");
            meshSplitProcedure.Parameters["Split Axis"].Parameters["Scope Axis"].Parameters["Axis"].Set("X");
            meshSplitProcedure.Parameters["Slices"].Set("Repetitive Slice");
            meshSplitProcedure.Parameters["Slices"].Parameters[0].Set("Slice");
            {
                var sliceParameter = meshSplitProcedure.Parameters["Slices"].Parameters[0].Parameters[0];
                sliceParameter.Parameters["Size"].Set(1f);
                sliceParameter.Parameters["Sizing"].Set("Absolute");
                sliceParameter.Parameters["Flexible"].Set(true);
                sliceParameter.Parameters["Cap"].Set(false);
            }
            meshSplitProcedure.Parameters["Index"].Set("SliceIndex");
            meshSplitProcedure.Inputs["Input"].Enqueue(MeshEntityHelper.CreateCube(5));
            meshSplitProcedure.Execute();


            var meshSlices = meshSplitProcedure
                .Parameters["Slices"]
                .Parameters[0].Parameters[0]
                .Outputs["Output"]
                .DequeueAll<MeshEntity>().ToList();

            Assert.AreEqual(5,meshSlices.Count);
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(i, meshSlices[i].Attributes[new GlobalAttributeKey("SliceIndex")]);
            }
        }
    }
}
