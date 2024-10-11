using System.Security.Cryptography;

namespace Drug.NetEase.Client.Utilities;

public class AESHelper
{
    public static byte[] AES_CBC_Encrypt(byte[] key, byte[] data, byte[] iv)
    {
        using (var aes = Aes.Create())
        {                                                                                                                                            
            aes.KeySize = key.Length * 8;
            aes.BlockSize = 128;
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (var encryptor = aes.CreateEncryptor())
            {
                return encryptor.TransformFinalBlock(data, 0, data.Length);
            }
        }
    }

    public static byte[] AES_CBC_Decrypt(byte[] key, byte[] data, byte[] iv)
    {
        using (var aes = Aes.Create())
        {
            aes.KeySize = key.Length * 8;
            aes.BlockSize = 128;
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (var decryptor = aes.CreateDecryptor())
            {
                return decryptor.TransformFinalBlock(data, 0, data.Length);
            }
        }
    }
}