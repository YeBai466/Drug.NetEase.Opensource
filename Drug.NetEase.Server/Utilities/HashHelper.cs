using System.Security.Cryptography;
using System.Text;

namespace Drug.NetEase.Server.Utilities;

public static class HashHelper
{
    public static string CompleteMD5Hex(byte[] bytes)
    {
        var md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(bytes);
        StringBuilder sb = new StringBuilder();
        foreach (byte b in hash)
        {
            sb.Append(b.ToString("x2")); 
        }
        return sb.ToString();
    }
}