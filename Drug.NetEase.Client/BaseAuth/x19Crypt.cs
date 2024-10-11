using System;
using System.Linq;
using System.Text;
using Drug.NetEase.Client.Extensions;
using Drug.NetEase.Client.Utilities;

namespace Drug.NetEase.Client.BaseAuth;

public class x19Crypt
{
    private static readonly string[] _keys =
    {
        "MK6mipwmOUedplb6",
        "OtEylfId6dyhrfdn"
    };
    public static byte[] PickKey(byte query)
    {
        return Encoding.UTF8.GetBytes(_keys[0]);
    }
    public static byte[] ParseLoginResponse(byte[] body)
    {
        return AESHelper.AES_CBC_Decrypt(PickKey(new byte()), body, Encoding.ASCII.GetBytes(_keys[1]));
    }

    public static byte[] HttpEncrypt(byte[] bodyIn)
    {
        return AESHelper.AES_CBC_Encrypt(PickKey(new byte()), bodyIn, Encoding.ASCII.GetBytes(_keys[1]));
    }
    public static string ComputeDynamicToken(string path, byte[] body, string token)
    {
        return null;
    }
}