using System;
using System.Security.Cryptography;
using System.Text;

namespace Sceelix.Helpers
{
    public class DataProtectionHelper
    {
        public static byte[] DecryptData(byte[] dataToDecrypt, string passphrase)
        {
            byte[] results;
            UTF8Encoding utf8 = new UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();
            byte[] tdesKey = hashProvider.ComputeHash(utf8.GetBytes(passphrase));

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            TripleDESCryptoServiceProvider tdesAlgorithm = new TripleDESCryptoServiceProvider();

            // Step 3. Setup the decoder
            tdesAlgorithm.Key = tdesKey;
            tdesAlgorithm.Mode = CipherMode.ECB;
            tdesAlgorithm.Padding = PaddingMode.PKCS7;

            // Step 5. Attempt to decrypt the string
            try
            {
                ICryptoTransform decryptor = tdesAlgorithm.CreateDecryptor();
                results = decryptor.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                tdesAlgorithm.Clear();
                hashProvider.Clear();
            }

            // Step 6. Return the decrypted string in UTF8 format
            return results;
        }



        public static string DecryptData(string message, string passphrase)
        {
            UTF8Encoding utf8 = new UTF8Encoding();

            var base64String = Convert.FromBase64String(message);
            var decryptData = DecryptData(base64String, passphrase);

            return utf8.GetString(decryptData);
        }



        public static byte[] EncryptData(byte[] dataToEncrypt, string passphrase)
        {
            byte[] results;

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();
            byte[] tdesKey = hashProvider.ComputeHash(new UTF8Encoding().GetBytes(passphrase));

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            TripleDESCryptoServiceProvider tdesAlgorithm = new TripleDESCryptoServiceProvider();

            // Step 3. Setup the encoder
            tdesAlgorithm.Key = tdesKey;
            tdesAlgorithm.Mode = CipherMode.ECB;
            tdesAlgorithm.Padding = PaddingMode.PKCS7;

            // Step 4. Attempt to encrypt the string
            try
            {
                ICryptoTransform encryptor = tdesAlgorithm.CreateEncryptor();
                results = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                tdesAlgorithm.Clear();
                hashProvider.Clear();
            }

            // Step 5. Return the encrypted string as a base64 encoded string
            return results;
        }



        public static string EncryptData(string message, string passphrase)
        {
            UTF8Encoding utf8 = new UTF8Encoding();

            return Convert.ToBase64String(EncryptData(utf8.GetBytes(message), passphrase));
        }
    }
}