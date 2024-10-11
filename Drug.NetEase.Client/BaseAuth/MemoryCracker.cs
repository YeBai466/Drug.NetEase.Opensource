using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Drug.NetEase.Client.Extensions;
using Drug.NetEase.Client.Utilities;

namespace Drug.NetEase.Client.BaseAuth;

public class MemoryCracker
{
    public static List<MemoryHelper.MemoryModify> ModifyList;
    public static void CrackCppGameProcess()
    {
        try
        {
            var memory = new MemoryHelper();
            if (!memory.OpenProcess("Minecraft.Windows.exe"))
                return;
            foreach (var modify in ModifyList)
            {
                Task.Run(() =>
                {
                    try
                    {
                        long address = 0;
                        while (address == 0)
                        {
                            foreach (var addressScan in memory.AoBScan(modify.Original.ToSpaceHex(), true, false, false).Result)
                            {
                                address = addressScan;
                                break;
                            }
                        }
                        memory.WriteMemory(address.ToHex(), "bytes", modify.Modify);
                    }
                    catch
                    {
                    }
                });
            }
        }
        catch
        {
        }
    }
}