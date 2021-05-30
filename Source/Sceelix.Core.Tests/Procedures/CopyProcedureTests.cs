using System.Linq;
using NUnit.Framework;
using Sceelix.Core.Data;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.Tests.Procedures
{
    public class CopyProcedureTests
    {
        [Test]
        public void TestCopyCount()
        {
            var entity = new Entity();

            CopyProcedure copyProcedure = new CopyProcedure();
            var copyProcedureParameter = copyProcedure.Parameters["Operation"].Parameters["Standard"];
            copyProcedureParameter.Parameters["Number of Copies"].Set(20);
            copyProcedureParameter.Inputs[0].Enqueue(entity);
            copyProcedure.Execute();

            Assert.AreEqual(20, copyProcedure.Outputs[0].DequeueAll().Count());
        }



        [Test]
        public void TestCopyType()
        {
            var entityGroup = new EntityGroup();

            CopyProcedure copyProcedure = new CopyProcedure();
            var copyProcedureParameter = copyProcedure.Parameters["Operation"].Parameters["Standard"];
            copyProcedureParameter.Parameters["Number of Copies"].Set(5);
            copyProcedureParameter.Inputs[0].Enqueue(entityGroup);
            copyProcedure.Execute();

            var entityGroups = copyProcedure.Outputs[0].DequeueAll().ToList();
            Assert.AreEqual(5, entityGroups.Count);
            CollectionAssert.AllItemsAreInstancesOfType(entityGroups, typeof(EntityGroup));
        }
    }
}