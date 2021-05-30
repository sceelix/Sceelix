using NUnit.Framework;
using Sceelix.Collections;

namespace Sceelix.Tests
{
    public class CircularListTests
    {
        [Test]
        public void AccessRandomIndices()
        {
            var circularList = new CircularList<int>();
            circularList.Add(2);
            circularList.Add(10);
            circularList.Add(8);
            circularList.Add(174);
            circularList.Add(10084);
            circularList.Add(45);

            Assert.IsNotEmpty(circularList);
            Assert.AreEqual(45, circularList[-1]);
            Assert.AreEqual(174, circularList[-3]);
            Assert.AreEqual(2, circularList[6]);
            Assert.AreEqual(10, circularList[25]);
        }
    }
}