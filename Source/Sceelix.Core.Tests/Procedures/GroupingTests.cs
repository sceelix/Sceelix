using System.Linq;
using NUnit.Framework;
using Sceelix.Core.Data;

namespace Sceelix.Core.Tests.Procedures
{
    public class GroupingTests
    {
        [Test]
        public void GroupEntities()
        {
            var entities = new Entity[] {new Entity(), new Entity(), new Entity(), new Entity()};

            var groupProcedure = new Sceelix.Core.Procedures.GroupProcedure();
            groupProcedure.Parameters["Operation"].Set("Group");
            {
                var groupParameter = groupProcedure.Parameters["Operation"].Parameters["Group"];
                groupParameter.Parameters["Merge Attributes"].Set(true);
                groupParameter.Inputs["Input"].Enqueue(entities);
            }
            groupProcedure.Execute();


            var entityGroup = groupProcedure.Parameters["Operation"].Parameters["Group"].Outputs["Output"].DequeueAll<EntityGroup>();
            Assert.AreEqual(4, entityGroup.First().Entities.Count);
        }
    }
}
