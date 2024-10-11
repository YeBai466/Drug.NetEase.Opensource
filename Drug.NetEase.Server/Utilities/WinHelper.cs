using System.Diagnostics;

namespace Drug.NetEase.Server.Utilities;

public class WinHelper
{
    public static int GetModuleHandle(string Modulename)
    {
        foreach (object obj in Process.GetCurrentProcess().Modules)
        {
            ProcessModule processModule = (ProcessModule)obj;
            bool flag = processModule.ModuleName == Modulename;
            if (flag)
            {
                return (int)processModule.BaseAddress;
            }
        }
        return 0;
    }
}