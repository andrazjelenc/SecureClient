using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace SecureClient
{
    class ClassRSA
    {
        /// <summary>
        /// Generate new RSA pair
        /// </summary>
        /// <returns>string[0]: private key in base64</returns>
        /// <returns>string[1]: public key in base64</returns>
        public static string[] GenerateRSAKey()
        {
            //generate new RSA key pair
            var ServiceProvider = new RSACryptoServiceProvider(2048);

            //ger private and public key
            var privateKey = ServiceProvider.ExportParameters(true);
            var publicKey = ServiceProvider.ExportParameters(false);

            //write private key to file
            var stringWriter = new System.IO.StringWriter();
            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            xmlSerializer.Serialize(stringWriter, privateKey);
            string privateKeyString = stringWriter.ToString();
            
            //write public key to file
            stringWriter = new System.IO.StringWriter();
            xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            xmlSerializer.Serialize(stringWriter, publicKey);
            string publicKeyString = stringWriter.ToString();

            return new string[] { Base64Encode(privateKeyString), Base64Encode(publicKeyString) };
        }

        /// <summary>
        /// Encrypt string with RSA
        /// </summary>
        /// <param name="keyString">RSA key to encrpyt string</param>
        /// <param name="message">String to encrpyt</param>
        /// <returns>Cipher string in base64</returns>
        public static string encrypt(string keyString, string message)
        {
            //read plaintext message from file
            string text = message;

            //from string to object
            var stringReader = new System.IO.StringReader(Base64Decode(keyString));
            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            var pubicKey = (RSAParameters)xmlSerializer.Deserialize(stringReader);

            var serviceProvider = new RSACryptoServiceProvider();
            serviceProvider.ImportParameters(pubicKey);

            //encrypt plain text
            var bytesPlainText = System.Text.Encoding.Unicode.GetBytes(text);
            var bytesCypher = serviceProvider.Encrypt(bytesPlainText, false);
            var cypher = Convert.ToBase64String(bytesCypher);
            //write encrypted text back to output file

            cypher = Base64Encode(cypher);
            return cypher;
        }

        /// <summary>
        /// Get string checksum and sign it
        /// </summary>
        /// <param name="keyString">RSA key to encrpyt checksum</param>
        /// <param name="message">String to sign</param>
        /// <returns>Cipher signature in base64</returns>
        public static string getSignature(string keyString, string message)
        {
            //from string to object
            var stringReader = new System.IO.StringReader(Base64Decode(keyString));
            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            var privateKey = (RSAParameters)xmlSerializer.Deserialize(stringReader);

            var serviceProvider = new RSACryptoServiceProvider();
            serviceProvider.ImportParameters(privateKey);

            //get signature
            RSAPKCS1SignatureFormatter formatter = new RSAPKCS1SignatureFormatter(serviceProvider);
            formatter.SetHashAlgorithm("SHA256");
            SHA256Managed SHhash = new SHA256Managed();
            byte[] signedValue = formatter.CreateSignature(SHhash.ComputeHash(new UnicodeEncoding().GetBytes(message)));

            //write string to output file
            string signature = System.Convert.ToBase64String(signedValue);
            signature = Base64Encode(signature);
            return signature;
        }

        /// <summary>
        /// Compare string and encrypted signature
        /// </summary>
        /// <param name="message">Plain text string</param>
        /// <param name="signatureInBase64">Encrypted signature in base64</param>
        /// <param name="publicKeyInBase64">RSA Key in base64 to check signature</param>
        /// <returns>True/False</returns>
        public static bool checkSignature(string message, string signatureInBase64, string publicKeyInBase64)
        {
            var stringReader = new System.IO.StringReader(Base64Decode(publicKeyInBase64));
            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            var pubicKey = (RSAParameters)xmlSerializer.Deserialize(stringReader);

            var serviceProvider = new RSACryptoServiceProvider();
            serviceProvider.ImportParameters(pubicKey);

            //get signature
            RSAPKCS1SignatureDeformatter RSADeformatter = new RSAPKCS1SignatureDeformatter(serviceProvider);
            RSADeformatter.SetHashAlgorithm("SHA256");
            SHA256Managed SHhash = new SHA256Managed();

            //checking
            if (RSADeformatter.VerifySignature(SHhash.ComputeHash(new UnicodeEncoding().GetBytes(message)), System.Convert.FromBase64String(Base64Decode(signatureInBase64))))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Decrypt cipher string with RSA
        /// </summary>
        /// <param name="privateKeyString">RSA key in base64 to decrypt string</param>
        /// <param name="cypherText">Cipher string to decrpyt</param>
        /// <returns>Plain text string</returns>
        public static string decrypt(string privateKeyString, string cypherText)
        {
            cypherText = Base64Decode(cypherText);
            //get object from string
            var stringReader = new System.IO.StringReader(Base64Decode(privateKeyString));
            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            var privateKey = (RSAParameters)xmlSerializer.Deserialize(stringReader);

            var ServiceProvider = new RSACryptoServiceProvider();
            ServiceProvider.ImportParameters(privateKey);

            byte[] bytesText = Convert.FromBase64String(cypherText);
            
            var bytesPlain = ServiceProvider.Decrypt(bytesText, false);

            //get string to output file
            var decrypted = System.Text.Encoding.Unicode.GetString(bytesPlain);
            return decrypted;
        }

        /// <summary>
        /// Convert string in base64 to string
        /// </summary>
        /// <param name="base64EncodedData">String in base64 to decode</param>
        /// <returns>plain text string</returns>
        private static string Base64Decode(string base64EncodedData)
        {
            byte[] base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        /// <summary>
        /// Convert string in base64 to string
        /// </summary>
        /// <param name="plainText">String to convert to base64</param>
        /// <returns>string in base64</returns>
        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
