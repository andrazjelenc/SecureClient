using System;
using System.IO;
using System.Security.Cryptography;

namespace SecureClient
{
    class ClassAES
    {
        /// <summary>
        /// Encrypt string with AES
        /// </summary>
        /// <param name="plainText">Plain text to encrpyt</param>
        /// <param name="password">Password string</param>
        /// <returns>Cipher string</returns>
        public static string EncryptStringAes(string plainText, string password)
        {
            //convert string key to bytes
            byte[] Key = ConvertKey(password);

            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");

            string output = "";
            byte[] encrypted;

            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }

                //convert cipger to Base64
                string cipherText = Convert.ToBase64String(encrypted);
                string IV = Convert.ToBase64String(aesAlg.IV);
                output = IV + cipherText;
            }
            return output;
        }

        /// <summary>
        /// Decrypt cipher string with AES
        /// </summary>
        /// <param name="cipherTextString">Cipher text to decrpyt</param>
        /// <param name="password">Password string</param>
        /// <returns>Plaintext string</returns>
        public static string DecryptStringAes(string cipherTextString, string password)
        {
            //convert key to bytes
            byte[] Key = ConvertKey(password);

            //IV is in front of cipherTextString
            string IV_String = cipherTextString.Substring(0, 24);
            string cipher_String = cipherTextString.Substring(24);
            byte[] cipherText = Convert.FromBase64String(cipher_String);
            byte[] IV = Convert.FromBase64String(IV_String);

            // Check arguments
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            string plaintext = null;

            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;

        }

        /// <summary>
        /// Convert string to bytes
        /// </summary>
        /// <param name="password">String to convert</param>
        /// <param name="keyBytes">output bytes length</param>
        /// <returns>string in bytes</returns>
        private static byte[] ConvertKey(string password, int keyBytes = 32)
        {
            const int Iterations = 300;
            byte[] Salt = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80 };
            var keyGenerator = new Rfc2898DeriveBytes(password, Salt, Iterations);
            return keyGenerator.GetBytes(keyBytes);
        }
    }
}
