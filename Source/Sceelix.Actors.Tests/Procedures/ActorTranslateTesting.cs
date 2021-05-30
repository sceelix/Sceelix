using System.Linq;
using NUnit.Framework;
using Sceelix.Actors.Data;
using Sceelix.Actors.Procedures;
using Sceelix.Mathematics.Data;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Helpers;

namespace Sceelix.Actors.Tests.Procedures
{
    public class ActorTranslateTesting
    {
        [Test]
        public void Reset()
        {
            var actorTranslateProcedure = new ActorTranslateProcedure();
            actorTranslateProcedure.Parameters["Operation"].Set("Reset");
            {
                var resetParameter = actorTranslateProcedure.Parameters["Operation"].Parameters["Reset"];
                resetParameter.Parameters["X"].Set("Center");
                resetParameter.Parameters["Y"].Set("Center");
                resetParameter.Parameters["Z"].Set("Minimum");
            }
            actorTranslateProcedure.Inputs["Input"].Enqueue(MeshEntityHelper.CreateBox(2, 3, 4));
            actorTranslateProcedure.Execute();

            var meshEntity = actorTranslateProcedure.Outputs["Output"].Dequeue<MeshEntity>();

            Assert.AreEqual(new Vector3D(-1f, -1.5f, 0f), meshEntity.BoxScope.Translation);
            Assert.That(meshEntity.FaceVertices.Select(x => x.Position.Z), Has.All.GreaterThanOrEqualTo(0));
        }



        [Test]
        public void Translate()
        {
            var actorTranslateProcedure = new ActorTranslateProcedure();
            actorTranslateProcedure.Parameters["Operation"].Set("Translate");
            {
                var translateParameter = actorTranslateProcedure.Parameters["Operation"].Parameters["Translate"];
                translateParameter.Parameters["Amount"].Set(new Vector3D(2, 5, 8));
                translateParameter.Parameters["Relative To"].Set("Scope");
            }
            actorTranslateProcedure.Inputs["Input"].Enqueue(MeshEntityHelper.CreateBox(1, 2, 5));
            actorTranslateProcedure.Execute();

            var actor = actorTranslateProcedure.Outputs["Output"].Dequeue<IActor>();

            Assert.AreEqual(new Vector3D(2, 5, 8), actor.BoxScope.Translation);
        }
    }
}