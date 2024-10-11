using System;

namespace Drug.NetEase.Client.Exceptions;

public class x19RequestException : Exception
{
    public x19RequestException(string msg):base(msg) { }
}