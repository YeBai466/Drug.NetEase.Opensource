using System.Collections.Generic;
using Drug.NetEase.Client.Utilities;

namespace Drug.NetEase.Client.BaseAuth;

public static class ServerAuthEntities
{
    public class HttpEncryptRetEntity
    {
        public int code;
        public string message;
        public string details;
        public string body;
    }
    public class ParseLoginResponseRetEntity
    {
        public int code;
        public string message;
        public string details;
        public string response;
    }
    public class ComputeDynamicTokenRetEntity
    {
        public int code;
        public string message;
        public string details;
        public string token;
    }
    public class GenerateCppConfigRetEntity
    {
        public int code;
        public string message;
        public string details;
        public string cppconfig;
        public List<MemoryHelper.MemoryModify> mem;
    }
    public class ParseCodeFromImageRetEntity
    {
        public int code;
        public string message;
        public string details;
        public string imagecode;
        public string flag;
    }
    public class IsFreeRetEntity
    {
        public int code;
        public string message;
        public string details;
    }
    public class RegisterUserRetEntity
    {
        public int code;
        public string message;
        public string details;
    }
    public class LoginUserRetEntity
    {
        public int code;
        public string message;
        public string details;
        public string token;
        public string isexpired;
        public string expiredtime;
    }
}