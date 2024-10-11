using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Drug.NetEase.Server.Extensions;

public static class StringExtensions
{
    public static string RandStringRunes(int length)
    {
        var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var result = new string(
            Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)])
                .ToArray());
        return result;
    }
    public static string RandomLetter(int length)
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    public static bool ContainsNonAlphanumeric(this string text)
    {
        var regex = new Regex("[^a-zA-Z0-9_]");
        return regex.IsMatch(text);
    }
}