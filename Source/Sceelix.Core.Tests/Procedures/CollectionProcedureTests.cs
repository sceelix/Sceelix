using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Sceelix.Core.Attributes;
using Sceelix.Core.Data;

namespace Sceelix.Core.Tests.Procedures
{
    
    public class CollectionProcedureTests
    {
        [Test]
        public void TestCount()
        {

            var entities = new []{new Entity(), new Entity(), new Entity()};

            var collectionProcedure = new Sceelix.Core.Procedures.CollectionProcedure();
            collectionProcedure.Parameters["Operation"].Set("Count");
            {
                var countParameter = collectionProcedure.Parameters["Operation"].Parameters["Count"];
                countParameter.Parameters["Index"].Set("Index");
                countParameter.Parameters["Count"].Set("Count");
            }
            collectionProcedure.Inputs["Input"].Enqueue(entities);
            collectionProcedure.Execute();

            var changedEntities = collectionProcedure.Outputs["Output"].DequeueAll().ToList();
            for (int i = 0; i < entities.Length; i++)
            {
                Assert.AreEqual(i,changedEntities[i].Attributes[new GlobalAttributeKey("Index")]);
                Assert.AreEqual(entities.Length,changedEntities[i].Attributes[new GlobalAttributeKey("Count")]);
            }

        }


        [Test]
        public void TestTake()
        {
            var entities = new []{new Entity(), new Entity(), new Entity(), new Entity(), new Entity()};

            var collectionProcedure = new Sceelix.Core.Procedures.CollectionProcedure();
            collectionProcedure.Parameters["Operation"].Set("Take");
            {
                var takeParameter = collectionProcedure.Parameters["Operation"].Parameters["Take"];
                takeParameter.Parameters["Starting Index"].Set(0);
                takeParameter.Parameters["Amount"].Set(2);
                takeParameter.Parameters["Loop"].Set(false);
            }
            collectionProcedure.Inputs["Input"].Enqueue(entities);
            collectionProcedure.Execute();

            var mainEntities = collectionProcedure.Outputs["Output"].DequeueAll();
            var elseEntities = collectionProcedure.Parameters["Operation"].Parameters["Take"].Outputs["Else"].DequeueAll();

            Assert.AreEqual(2, mainEntities.Count());
            Assert.AreEqual(3, elseEntities.Count());
        }
    }
}
