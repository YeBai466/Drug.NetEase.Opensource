using System;

namespace Drug.NetEase.Client.Extensions;

public static class LoggingExtensions
{
    public static void Log(this string content)
    {
        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} INF: {content}");
    }
}