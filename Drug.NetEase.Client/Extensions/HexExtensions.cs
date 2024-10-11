using System;
using System.Text;

namespace Drug.NetEase.Client.Extensions;

public static class HexExtensions
{
    public static string ToSpaceHex(this string text)
    {
        return BitConverter.ToString(Encoding.UTF8.GetBytes(text)).Replace('-', ' ');
    }
    public static string ToHex(this string text)
    {
        return Encoding.UTF8.GetBytes(text).ToHex();
    }

    public static string ToHex(this long num)
    {
        return Convert.ToString(num, 16);
    }
    public static byte[] ToBytes(this string hex)
    {
        if (string.IsNullOrEmpty(hex))
        {
            return null;
        }

        byte[] bytes = new byte[hex.Length / 2];

        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = byte.Parse(hex.Substring(i * 2, 2),
                System.Globalization.NumberStyles.HexNumber);
        }
        return bytes;
    }
    public static string HexToString(this string hex)
    {
        return Encoding.UTF8.GetString(hex.ToBytes());
    }
    
    public static string ToHex(this byte[] bytes) 
    { 
        string returnStr = ""; 
        if (bytes != null) 
        { 
            for (int i = 0; i < bytes.Length; i++) 
            { 
                returnStr += bytes[i].ToString("X2"); 
            } 
        } 
        return returnStr; 
    }
}