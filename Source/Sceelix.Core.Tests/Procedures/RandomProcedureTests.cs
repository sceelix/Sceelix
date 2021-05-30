using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Sceelix.Core.Attributes;
using Sceelix.Core.Data;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.Tests.Procedures
{
    public class RandomProcedureTests
    {
        [Test]
        public void TestMany()
        {
            var entities = new [] {new Entity(), new Entity(),new Entity(), new Entity()};
            var textValues = new object[] {"This", "is", 1, "test"};

            var randomProcedure = new Sceelix.Core.Procedures.RandomProcedure();
            randomProcedure.Parameters["Inputs"].Set("Collective");
            {
                var collectiveParameter = randomProcedure.Parameters["Inputs"].Parameters["Collective"];
                collectiveParameter.Inputs["Collective"].Enqueue(entities);
            }
            randomProcedure.Parameters["Attributes"].Set("List");
            {
                var listParameter = randomProcedure.Parameters["Attributes"].Parameters[0];
                listParameter.Parameters["List"].Set("Item");
                listParameter.Parameters["List"].Set(textValues);
                listParameter.Parameters["Value"].Set("RandomListItem");
            }
            randomProcedure.Parameters["Attributes"].Set("Double");
            {
                var doubleParameter = randomProcedure.Parameters["Attributes"].Parameters[1];
                doubleParameter.Parameters["Minimum"].Set(30);
                doubleParameter.Parameters["Maximum"].Set(70);
                doubleParameter.Parameters["Value"].Set("RandomDouble");
            }
            randomProcedure.Parameters["Attributes"].Set("Integer");
            {
                var integerParameter = randomProcedure.Parameters["Attributes"].Parameters[2];
                integerParameter.Parameters["Minimum"].Set(3);
                integerParameter.Parameters["Maximum"].Set(10);
                integerParameter.Parameters["Value"].Set("RandomInteger");
            }
            randomProcedure.Parameters["Seed"].Set(1000);
            randomProcedure.Execute();

            var outputEntities = randomProcedure.Outputs["Output"].DequeueAll();
            foreach (var outputEntity in outputEntities)
            {
                var randomListItem = outputEntity.Attributes[new GlobalAttributeKey("RandomListItem")];   
                CollectionAssert.Contains(textValues, randomListItem);

                var randomDouble = (double)outputEntity.Attributes[new GlobalAttributeKey("RandomDouble")];
                Assert.GreaterOrEqual(randomDouble, 30);
                Assert.LessOrEqual(randomDouble, 70);

                var randomInteger = (int)outputEntity.Attributes[new GlobalAttributeKey("RandomInteger")];
                Assert.GreaterOrEqual(randomInteger, 3);
                Assert.LessOrEqual(randomInteger, 10);
            }
        }
    }
}