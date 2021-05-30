using NUnit.Framework;
using Sceelix.Actors.Data;
using Sceelix.Actors.Procedures;
using Sceelix.Mathematics.Data;
using Sceelix.Meshes.Helpers;

namespace Sceelix.Actors.Tests.Procedures
{
    public class ActorScaleTesting
    {
        [Test]
        public void Scale()
        {
            var actorScaleProcedure = new ActorScaleProcedure();
            actorScaleProcedure.Parameters["Amount"].Set(new Vector3D(3, 3, 6));
            actorScaleProcedure.Parameters["Mode"].Set("Relative");
            actorScaleProcedure.Parameters["Relative To"].Set("Scope");
            {
                var pivotParameter = actorScaleProcedure.Parameters["Pivot"];
                pivotParameter.Parameters["Position"].Set(new Vector3D(0, 0, 0));
                pivotParameter.Parameters["Offset"].Set("Relative");
                pivotParameter.Parameters["Relative To"].Set("Scope");
            }
            actorScaleProcedure.Inputs["Input"].Enqueue(MeshEntityHelper.CreateBox(1, 5, 6));
            actorScaleProcedure.Execute();

            var actor = actorScaleProcedure.Outputs["Output"].Dequeue<IActor>();

            Assert.AreEqual(new Vector3D(3, 15, 36), actor.BoxScope.Sizes);
        }
    }
}