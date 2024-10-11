using System.Security.Cryptography;

namespace Drug.NetEase.Server.Utilities;

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
                byte[] encrypted = new byte[data.Length];
                encryptor.TransformBlock(data, 0, data.Length, encrypted, 0);
                return encrypted;
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
                byte[] decrypted = new byte[data.Length];
                decryptor.TransformBlock(data, 0, data.Length, decrypted, 0);
                return decrypted;
            }
        }
    }
    public static byte[] AES_CBC_Encrypt2(byte[] key, byte[] data, byte[] iv)
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

    public static byte[] AES_CBC_Decrypt2(byte[] key, byte[] data, byte[] iv)
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