using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using Drug.NetEase.Client.Extensions;
using Newtonsoft.Json;

namespace Drug.NetEase.Client.BaseAuth;

public static class ServerAuth
{
    public const string Version = "0.0.4";
    public static string Token = null;
    public static string ServerUrl = "127.0.0.1";
    public static byte[] HttpEncrypt(string body)
    {
        var ret = RequestString("/Drug/EncryptAuthenticationBody", new
        {
            body = body.ToHex(),
            usertoken = Token
        });
        var retObject = JsonConvert.DeserializeObject<ServerAuthEntities.HttpEncryptRetEntity>(ret);
        if (retObject.details == "用户已到期")
        {
            throw new Exception("用戶已到期");
        }
        var authenticationBody = retObject.body;
        return authenticationBody.ToBytes();
    }
    public static string ParseLoginResponse(string hexReponse)
    {
        var ret = RequestString("/Drug/DecryptAuthenticationResponse", new
        {
            response = hexReponse,
            usertoken = Token
        });
        var retObject = JsonConvert.DeserializeObject<ServerAuthEntities.ParseLoginResponseRetEntity>(ret);
        if (retObject.details == "用户已到期")
        {
            throw new Exception("用戶已到期");
        }
        var authenticationResponse = retObject.response;
        return authenticationResponse.HexToString();
    }
    public static string ComputeDynamicToken(string path, byte[] body, string token)
    {
        var ret = RequestString("/Drug/ComputeDynamicToken", new
        {
            path = path.ToHex(),
            body = body.ToHex(),
            token = token.ToHex(),
            usertoken = Token
        });
        var retObject = JsonConvert.DeserializeObject<ServerAuthEntities.ComputeDynamicTokenRetEntity>(ret);
        if (retObject.details == "用户已到期")
        {
            throw new Exception("用戶已到期");
        }
        var dynamicToken = retObject.token;
        return dynamicToken.HexToString();
    }
    public static string GenerateCppConfig(string baseName, string response, long userId, string configPath,
        string userName, string skinFile, string skinMd5, bool skinSlim, string serverId, string serverAddress,
        int serverPort)
    {
        var ret = RequestString("/Drug/GenerateCppConfig",new
        {
            basename = baseName.ToHex(),
            response = response,
            userid = Convert.ToString(userId).ToHex(),
            configpath = configPath.ToHex(),
            username = userName.ToHex(),
            skinfile = skinFile.ToHex(),
            skinmd5 = skinMd5.ToHex(),
            skinslim = (skinSlim ? "true" : "false").ToHex(),
            serverid = serverId.ToHex(),
            serveraddress = serverAddress.ToHex(),
            serverport = Convert.ToString(serverPort).ToHex(),
            usertoken = Token
        });
        var retObject = JsonConvert.DeserializeObject<ServerAuthEntities.GenerateCppConfigRetEntity>(ret);
        if (retObject.details == "用户已到期")
        {
            throw new Exception("用戶已到期");
        }
        string cppConfig = retObject.cppconfig;
        MemoryCracker.ModifyList = retObject.mem;
        return cppConfig.HexToString();
    }

    public static string RequestString(string url, object parameter)
    {
        var encryptParameter = Encoding.UTF8.GetBytes(x19Crypt.HttpEncrypt(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(parameter))).ToHex());
        //var ret = x19Crypt.ParseLoginResponse(x19AuthRequest.RequestString($"http://127.0.0.1:8080{url}",encryptParameter).ToBytes());
        var ret = x19Crypt.ParseLoginResponse(x19AuthRequest.RequestString($"http://111.180.188.59:250{url}",encryptParameter).ToBytes());
        return Encoding.UTF8.GetString(ret);
    }

    public static bool ParseCodeFromImage(byte[] buffer, out string code)
    {
        var ret = RequestString("/Drug/ParseCodeFromImage",new
        {
            image = buffer.ToHex(),
            usertoken = Token
        });
        var retObject = JsonConvert.DeserializeObject<ServerAuthEntities.ParseCodeFromImageRetEntity>(ret);
        if (retObject.details == "用户已到期")
        {
            throw new Exception("用戶已到期");
        }
        code = retObject.imagecode;
        string flag = retObject.flag;
        return flag == "true";
    }

    public static string IsFree(string version)
    {
        var ret = RequestString("/Drug/IsFree",new
        {
            version = version
        });
        var retObject = JsonConvert.DeserializeObject<ServerAuthEntities.IsFreeRetEntity>(ret);
        return retObject.details;
    }

    public static string RegisterUser(string username, string password)
    {
        var uniqueCode = "";
        try
        {
            using (var cpuSearcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
            using (var baseboardSearcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
            {
                foreach (var cpu in cpuSearcher.Get())
                {
                    var cpuId = cpu["ProcessorId"].ToString();
                    foreach (var baseboard in baseboardSearcher.Get())
                    {
                        var baseboardId = baseboard["SerialNumber"].ToString();
                        uniqueCode = cpuId + baseboardId;
                        break; // 只获取第一个 CPU 和主板 的 ID
                    }
                    break;
                }
            }

            // 使用哈希算法生成不可逆的唯一标识
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(uniqueCode);
                var hashBytes = sha256.ComputeHash(bytes);
                uniqueCode = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
        catch
        {
        }
        var ret = RequestString("/Drug/RegisterUser",new
        {
            username = username,
            password = password,
            machinecode = uniqueCode
        });
        
        var retObject = JsonConvert.DeserializeObject<ServerAuthEntities.RegisterUserRetEntity>(ret);
        return retObject.details;
    }
    public static ServerAuthEntities.LoginUserRetEntity LoginUser(string username, string password)
    {
        var uniqueCode = "";
        try
        {
            using (var cpuSearcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
            using (var baseboardSearcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
            {
                foreach (var cpu in cpuSearcher.Get())
                {
                    var cpuId = cpu["ProcessorId"].ToString();
                    foreach (var baseboard in baseboardSearcher.Get())
                    {
                        var baseboardId = baseboard["SerialNumber"].ToString();
                        uniqueCode = cpuId + baseboardId;
                        break; // 只获取第一个 CPU 和主板 的 ID
                    }
                    break;
                }
            }

            // 使用哈希算法生成不可逆的唯一标识
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(uniqueCode);
                var hashBytes = sha256.ComputeHash(bytes);
                uniqueCode = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
        catch
        {
        }
        var ret = RequestString("/Drug/LoginUser",new
        {
            username = username,
            password = password,
            machinecode = uniqueCode
        });
        var retObject = JsonConvert.DeserializeObject<ServerAuthEntities.LoginUserRetEntity>(ret);
        if (retObject.details == "登錄成功")
        {
            Token = retObject.token;
        }
        return retObject;
    }

    public static void Ping()
    {
        var url = "http://111.180.188.59:";
        for (var i = 250; i < 301; i++)
        {
            try
            {
                if (x19AuthRequest.RequestString($"{url}{i}/").Contains("404"))
                {
                    ServerUrl = $"{url}{i}";
                    break;
                }
            }
            catch
            {
                
            }
        }
    }
}