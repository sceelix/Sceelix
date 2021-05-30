using NUnit.Framework;
using Sceelix.Collections;

namespace Sceelix.Tests.Data
{
    public class SceeListTests
    {
        [Test]
        public void AccessByKeyAndIndex()
        {
            var sceeList = new SceeList(new[] {"A", "B", "X"}, new object[] {23, "Hello", "Test"});

            Assert.AreEqual("Hello", sceeList["B"]);
            Assert.AreEqual("Test", sceeList["X"]);
            Assert.AreEqual(23, sceeList[0]);
            Assert.AreEqual("Test", sceeList[2]);
        }
    }
}