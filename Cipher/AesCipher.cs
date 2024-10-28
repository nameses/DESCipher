using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Cipher
{
    public class AesCipher : ICipher
    {
        private string _cipherKey { get; }
        private string _ivKey { get; }

        public AesCipher(string cipherKey, string ivKey)
        {
            _cipherKey = cipherKey;
            _ivKey = ivKey;
        }

        public string EncryptText(string text)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_cipherKey.Substring(0, 32));
            var ivBytes = Encoding.UTF8.GetBytes(_ivKey.Substring(0, 16));

            using (Aes aes = Aes.Create()) 
            using (ICryptoTransform encryptor = aes.CreateEncryptor(keyBytes, ivBytes))
            using (MemoryStream mStream = new MemoryStream())
            using (CryptoStream cStream = new CryptoStream(mStream, encryptor, CryptoStreamMode.Write))
            {
                byte[] toEncrypt = Encoding.UTF8.GetBytes(text);

                cStream.Write(toEncrypt, 0, toEncrypt.Length);
                cStream.FlushFinalBlock();

                return Convert.ToBase64String(mStream.ToArray());
            }
        }

        public string DecryptText(string encryptedText)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            var keyBytes = Encoding.UTF8.GetBytes(_cipherKey.Substring(0, 32));
            var ivBytes = Encoding.UTF8.GetBytes(_ivKey.Substring(0, 16));

            using (Aes aes = Aes.Create())
            using (ICryptoTransform decryptor = aes.CreateDecryptor(keyBytes, ivBytes))
            using (MemoryStream mStream = new MemoryStream(encryptedBytes))
            using (CryptoStream cStream = new CryptoStream(mStream, decryptor, CryptoStreamMode.Read))
            using (StreamReader reader = new StreamReader(cStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
