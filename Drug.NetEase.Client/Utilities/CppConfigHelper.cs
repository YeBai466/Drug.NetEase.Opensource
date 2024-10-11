using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Drug.NetEase.Client.BaseAuth;
using Drug.NetEase.Client.Extensions;
using Microsoft.Win32;

namespace Drug.NetEase.Client.Utilities;

public class CppConfigHelper
{
    public static string CppGamePath
    {
        get
        {
            if (_cppGamePath == null)
            {
                try
                {
                    _cppGamePath = (string)Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Netease\MCLauncher")
                        .GetValue("MinecraftBENeteasePath") + @"\windowsmc";
                    if (!Directory.Exists(_cppGamePath))
                        throw new Exception();
                }
                catch
                {
                    try
                    {
                        _cppGamePath = (string)Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Netease\PC4399_MCLauncher")
                            .GetValue("MinecraftBENeteasePath") + @"\windowsmc";
                        if (!Directory.Exists(_cppGamePath))
                            throw new Exception();
                    }
                    catch
                    { 
                        _cppGamePath = "";
                    }   
                }
            }
            return _cppGamePath;
        }
    }

    private static string _cppGamePath;
    
    public CppConfigHelper(string baseName, string response, long userId, string configPath,
        string userName, string skinFile, string skinMd5, bool skinSlim, string serverId = "4654307171942868781",
        string serverAddress = "play.bjd-mc.com", int serverPort = 19132)
    {
        _cppConfig = ServerAuth.GenerateCppConfig(baseName,response,userId,configPath,
            userName,skinFile,skinMd5,skinSlim,serverId,serverAddress,serverPort);
        _configPath = configPath;
    }
    
    public Process GetProcess(string errorFile)
    {
        if (File.Exists(_configPath))
            File.Delete(_configPath);
        if (File.Exists(errorFile))
            File.Delete(errorFile);
        File.Create(_configPath).Close();
        File.Create(errorFile).Close();
        File.WriteAllText(_configPath,_cppConfig);
        var process = new Process
        {
            StartInfo =
            {
                FileName = @$"{CppGamePath}\Minecraft.Windows.exe",
                WorkingDirectory = CppGamePath,
                Arguments = $"config=\"{_configPath}\" errorlog=\"{errorFile}\"",
                UseShellExecute = false
            }
        };
        return process;
    }

    private string _configPath;
    private string _cppConfig;
}