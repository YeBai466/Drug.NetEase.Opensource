using System;
using System.Linq;
using System.Text;
using Drug.NetEase.Server.Extensions;
using Drug.NetEase.Server.Utilities;

namespace Drug.NetEase.Server.BaseAuth;

public static class x19Crypt
{
    private static readonly string[] _keys =
    {
        "MK6mipwmOUedplb6",
        "OtEylfId6dyhrfdn",
        "VNbhn5mvUaQaeOo9",
        "bIEoQGQYjKd02U0J",
        "fuaJrPwaH2cfXXLP",
        "LEkdyiroouKQ4XN1",
        "jM1h27H4UROu427W",
        "DhReQada7gZybTDk",
        "ZGXfpSTYUvcdKqdY",
        "AZwKf7MWZrJpGR5W",
        "amuvbcHw38TcSyPU",
        "SI4QotspbjhyFdT0",
        "VP4dhjKnDGlSJtbB",
        "UXDZx4KhZywQ2tcn",
        "NIK73ZNvNqzva4kd",
        "WeiW7qU766Q1YQZI"
    };

    public static byte[] PickKey(byte query)
    {
        return Encoding.UTF8.GetBytes(_keys[(query >> 4) & 0xf]);
    }

    public static string ParseLoginResponse(byte[] body)
    {
        if (body.Length < 0x12) throw new ArgumentException("Input body too short");
        var result = AESHelper.AES_CBC_Decrypt(PickKey(body[body.Length - 1]),
            body.Skip(16).Take(body.Length - 17).ToArray(), body.Take(16).ToArray());
        var scissor = 0;
        var scissorPos = result.Length - 1;
        while (scissor < 16)
        {
            if (result[scissorPos] != 0x00) scissor++;
            scissorPos--;
        }

        return Encoding.UTF8.GetString(result.Take(scissorPos + 1).ToArray());
    }

    public static byte[] HttpEncrypt(byte[] bodyIn)
    {
        try
        {
            var body = new byte[(int)Math.Ceiling((double)(bodyIn.Length + 16) / 16) * 16];
            Array.Copy(bodyIn, body, bodyIn.Length);
            var randFill = Encoding.ASCII.GetBytes(StringExtensions.RandStringRunes(0x10));
            for (var i = 0; i < randFill.Length; i++) body[i + bodyIn.Length] = randFill[i];

            var keyQuery = (byte)((new Random().Next(0, 15) << 4) | 2);
            var initVector = Encoding.ASCII.GetBytes(StringExtensions.RandStringRunes(0x10));
            var encrypted = AESHelper.AES_CBC_Encrypt(PickKey(keyQuery), body, initVector);
            var result = new byte[16 /* iv */ + encrypted.Length /* encrypted (body + scissor) */ + 1 /* key query */];
            Array.Copy(initVector, 0, result, 0, 16);
            Array.Copy(encrypted, 0, result, 16, encrypted.Length);
            result[result.Length - 1] = keyQuery;

            return result;
        }
        catch
        {
            return new byte[0];
        }
    }

    public static string ComputeDynamicToken(string path, byte[] body, string token)
    {
        var payload = new StringBuilder();
        payload.Append(HashHelper.CompleteMD5Hex(Encoding.UTF8.GetBytes(token)));
        payload.Append(Encoding.UTF8.GetString(body));
        payload.Append("0eGsBkhl");
        payload.Append(path.TrimEnd('?'));
        var sum = Encoding.UTF8.GetBytes(HashHelper.CompleteMD5Hex(Encoding.UTF8.GetBytes(payload.ToString())));
        var binaryString = sum.ToBinary();
        binaryString = binaryString.Substring(6) + binaryString.Substring(0, 6);
        for (var i = 0; i < sum.Length; i++)
        {
            var section = binaryString.Substring(i * 8, 8);
            byte by = 0;
            for (var j = 0; j < 8; j++)
                if (section[7 - j] == '1')
                    by = (byte)(by | (1 << (j & 0x1f)));
            sum[i] = (byte)(by ^ sum[i]);
        }

        var b64Encoded = Convert.ToBase64String(sum);
        var result = b64Encoded.Substring(0, 16).Replace("+", "m").Replace("/", "o") + "1";

        return result;
    }
}