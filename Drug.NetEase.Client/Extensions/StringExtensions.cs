using System;
using System.Linq;

namespace Drug.NetEase.Client.Extensions;

public class StringExtensions
{
    public static string RandomLetter(int length)
    {
        Random random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    public static string GenerateRandomString(int n)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();

        // 从字符集合中随机选择n个字符，组成一个字符串
        return new string(Enumerable.Repeat(chars, n)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}