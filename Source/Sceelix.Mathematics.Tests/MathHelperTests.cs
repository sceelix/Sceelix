using NUnit.Framework;
using Sceelix.Mathematics.Helpers;

namespace Sceelix.Mathematics.Tests
{
    public class MathHelperTests
    {
        [Test]
        public void Clamp()
        {
            Assert.AreEqual(3, MathHelper.Clamp(3, 0, 5));
            Assert.AreEqual(3, MathHelper.Clamp(43, 0, 3));
            Assert.AreEqual(-1, MathHelper.Clamp(-4, -1, 2));
        }



        [Test]
        public void Mirror()
        {
            Assert.AreEqual(3, MathHelper.Mirror(3, 0, 3));
            Assert.AreEqual(0, MathHelper.Mirror(0, 0, 3));
            Assert.AreEqual(1, MathHelper.Mirror(1, 0, 3));
            Assert.AreEqual(1, MathHelper.Mirror(-1, 0, 3));
            Assert.AreEqual(1, MathHelper.Mirror(5, 0, 3), 1);
            Assert.AreEqual(2, MathHelper.Mirror(-4, -1, 2), 2);
        }



        [Test]
        public void MirrorFloat()
        {
            Assert.AreEqual(0, MathHelper.Mirror(0, 0, 5f));
            Assert.AreEqual(4.9f, MathHelper.Mirror(5.1f, 0, 5f), 0.01f);
            Assert.AreEqual(0.1f, MathHelper.Mirror(10.1f, 0, 5f), 0.01f);
        }



        [Test]
        public void RepeatFloat()
        {
            Assert.AreEqual(0, MathHelper.Repeat(0, 0, 5f), 0);
            Assert.AreEqual(0, MathHelper.Repeat(5, 0, 5f), 0);
            Assert.AreEqual(0.1f, MathHelper.Repeat(5.1f, 0, 5f), 0.01f);
        }



        [Test]
        public void RepeatInt()
        {
            Assert.AreEqual(0, MathHelper.Repeat(5, 0, 5));
            Assert.AreEqual(1, MathHelper.Repeat(6, 0, 5));
            Assert.AreEqual(0, MathHelper.Repeat(0, 0, 5));
            Assert.AreEqual(4, MathHelper.Repeat(-1, 0, 5));
        }
    }
}