using System.Linq;
using NUnit.Framework;
using Sceelix.Actors.Procedures;
using Sceelix.Mathematics.Data;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Helpers;

namespace Sceelix.Actors.Tests.Procedures
{
    public class ActorRotateTesting
    {
        [Test]
        public void Rotate()
        {
            var actorRotateProcedure = new ActorRotateProcedure();
            actorRotateProcedure.Parameters["Operation"].Set("Rotate");
            {
                var rotateParameter = actorRotateProcedure.Parameters["Operation"].Parameters["Rotate"];
                rotateParameter.Parameters["Angle"].Set(90f);
                {
                    var axisParameter = rotateParameter.Parameters["Axis"];
                    axisParameter.Parameters["Direction"].Set(new Vector3D(1, 0, 0));
                    axisParameter.Parameters["Relative To"].Set("Scope");
                }
                {
                    var pivotParameter = rotateParameter.Parameters["Pivot"];
                    pivotParameter.Parameters["Position"].Set(new Vector3D(0, 0, 0));
                    pivotParameter.Parameters["Offset"].Set("Relative");
                    pivotParameter.Parameters["Relative To"].Set("Scope");
                }
            }
            actorRotateProcedure.Inputs["Input"].Enqueue(MeshEntityHelper.CreateRectangle(10, 20));
            actorRotateProcedure.Execute();

            var meshEntity = actorRotateProcedure.Outputs["Output"].Dequeue<MeshEntity>();

            Assert.AreEqual(-Vector3D.YVector, meshEntity.BoxScope.ToWorldDirection(Vector3D.ZVector));
            Assert.That(meshEntity.Faces.Select(x => x.Normal.Y), Has.All.Negative);
        }
    }
}