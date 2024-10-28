using System;

namespace Cipher
{
    public static class CipherFactory
    {
        public static ICipher CreateCipher(CipherType cipherType, string cipherKey, string ivKey)
        {
            return cipherType switch
            {
                CipherType.AES => new AesCipher(cipherKey, ivKey),
                CipherType.DES => new DesCipher(cipherKey, ivKey),
                CipherType.TripleDES => new TripleDesCipher(cipherKey, ivKey),
                _ => throw new ArgumentException("Invalid cipher type", nameof(cipherType)),
            };
        }
    }
}
