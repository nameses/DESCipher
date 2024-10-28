namespace Cipher
{
    public interface ICipher
    {
        public string EncryptText(string text);
        public string DecryptText(string text);
    }
}
