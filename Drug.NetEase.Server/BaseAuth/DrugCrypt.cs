using System.Text;
using Drug.NetEase.Server.Utilities;

namespace Drug.NetEase.Server.BaseAuth;
public class DrugCrypt
{
    public static byte[] Decrypt(byte[] body)
    {
        return AESHelper.AES_CBC_Decrypt2(Encoding.UTF8.GetBytes("MK6mipwmOUedplb6"), body, Encoding.ASCII.GetBytes("OtEylfId6dyhrfdn"));
    }

    public static byte[] Encrypt(byte[] bodyIn)
    {
        return AESHelper.AES_CBC_Encrypt2(Encoding.UTF8.GetBytes("MK6mipwmOUedplb6"), bodyIn, Encoding.ASCII.GetBytes("OtEylfId6dyhrfdn"));
    }
}