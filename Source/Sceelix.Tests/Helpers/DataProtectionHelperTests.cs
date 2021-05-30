using NUnit.Framework;
using Sceelix.Helpers;

namespace Sceelix.Tests
{
    internal class DataProtectionHelperTests
    {
        [Test]
        public void EncryptAndDecryptBinary()
        {
            string testString = "Hello, this is 1 test string with %&#%$$ strange chars.";
            string passphrase = "Just a very secure passphrase";

            byte[] zippedData = CompressionHelper.ZipStr(testString);

            byte[] encryptedData = DataProtectionHelper.EncryptData(zippedData, passphrase);
            byte[] decryptedData = DataProtectionHelper.DecryptData(encryptedData, passphrase);

            string unzippedString = CompressionHelper.UnZipStr(decryptedData);

            Assert.AreEqual(zippedData, decryptedData);
            Assert.AreNotEqual(zippedData, encryptedData);
            Assert.AreNotEqual(encryptedData, decryptedData);
            Assert.AreEqual(testString, unzippedString);
        }



        [Test]
        public void EncryptAndDecryptText()
        {
            string testString = "Hello, this is 1 test string with %&#%$$ strange chars.";
            string passphrase = "Just a very secure passphrase";

            string encryptedString = DataProtectionHelper.EncryptData(testString, passphrase);
            string decryptedString = DataProtectionHelper.DecryptData(encryptedString, passphrase);

            Assert.AreNotEqual(encryptedString, decryptedString);
            Assert.AreEqual(testString, decryptedString);
        }
    }
}