using NUnit.Framework;
using Sceelix.Helpers;

namespace Sceelix.Tests
{
    public class CompressionHelperTests
    {
        [Test]
        public void ZipAndUnzip()
        {
            string testString = "Hello, this is 1 test string with %&#%$$ strange chars.";

            var zippedData = CompressionHelper.ZipStr(testString);
            var unzippedData = CompressionHelper.UnZipStr(zippedData);

            Assert.AreNotEqual(zippedData, unzippedData);
            Assert.AreEqual(testString, unzippedData);
        }
    }
}