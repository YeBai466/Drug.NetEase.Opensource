using System.IO;
using System.Security.Cryptography;
using Drug.NetEase.Client.Extensions;

namespace Drug.NetEase.Client.Utilities;

public static class HashHelper
{
    public static string CompleteMD5FromFile(string fileName)
    {
        var file = new FileStream(fileName, System.IO.FileMode.Open);
        var md5 = new MD5CryptoServiceProvider();
        var retVal = md5.ComputeHash(file);
        file.Close();
        return retVal.ToHex().ToLower();
    }
}