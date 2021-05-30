using System.Linq;
using NUnit.Framework;
using Sceelix.Core.Attributes;

namespace Sceelix.Core.Tests.Procedures
{
    public class EntityCreateProcedureTests
    {
        [Test]
        public void CreateStandard()
        {
            var entityCreateProcedure = new Sceelix.Core.Procedures.EntityCreateProcedure();
            entityCreateProcedure.Parameters["Method"].Set("Standard");
            {
                var standardParameter = entityCreateProcedure.Parameters["Method"].Parameters["Standard"];
                standardParameter.Parameters["Index"].Set("MyIndex");
                standardParameter.Parameters["Count"].Set(30);
            }
            entityCreateProcedure.Execute();

            var entities = entityCreateProcedure.Outputs["Entities"].DequeueAll().ToList();

            Assert.AreEqual(30, entities.Count);
            for (int i = 0; i < entities.Count; i++)
            {
                Assert.AreEqual(i,entities[i].Attributes[new GlobalAttributeKey("MyIndex")]);
            }
        }


        [Test]
        public void CreateFromList()
        {
            var entityCreateProcedure = new Sceelix.Core.Procedures.EntityCreateProcedure();
            entityCreateProcedure.Parameters["Method"].Set("List");
            {
                var listParameter = entityCreateProcedure.Parameters["Method"].Parameters["List"];
                listParameter.Parameters["Item"].Set("Item");
                listParameter.Parameters["List"].SetExpression("[\"Hello\",23,false]");
            }
            entityCreateProcedure.Execute();

            var entities = entityCreateProcedure.Outputs["Entities"].DequeueAll().ToList();
            Assert.AreEqual(3, entities.Count);

            Assert.AreEqual("Hello",entities[0].Attributes[new GlobalAttributeKey("Item")]);
            Assert.AreEqual(23,entities[1].Attributes[new GlobalAttributeKey("Item")]);
            Assert.AreEqual(false,entities[2].Attributes[new GlobalAttributeKey("Item")]);
        }
    }
}