using NUnit.Framework;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Helpers;

namespace Sceelix.Meshes.Tests.Procedures
{
    public class MeshExtrudeTests
    {
        [Test]
        public void ExtrudeCube()
        {
            var meshModifyProcedure = new Sceelix.Meshes.Procedures.MeshModifyProcedure();
            meshModifyProcedure.Parameters["Operation"].Set("Extrude");
            {
                var extrudeParameter = meshModifyProcedure.Parameters["Operation"].Parameters["Extrude"];
                extrudeParameter.Parameters["Amount"].Set(1f);
                extrudeParameter.Parameters["Method"].Set("Normal");
                extrudeParameter.Parameters["Method"].Parameters["Normal"].Parameters["Offset"].Set(0f);
                extrudeParameter.Parameters["Cap"].Set(false);
                extrudeParameter.Parameters["Section"].Set("");
            }
            meshModifyProcedure.Inputs["Input"].Enqueue(MeshEntityHelper.CreateCube());
            meshModifyProcedure.Execute();

            var meshEntity = meshModifyProcedure.Outputs["Output"].Dequeue<MeshEntity>();

            Assert.AreEqual(6*4 + 6, meshEntity.Faces.Count);
            Assert.AreEqual(6*4 + 6, meshEntity.Area);
        }
    }
}
