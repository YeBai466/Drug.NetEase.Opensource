using System;
using System.Runtime.InteropServices;
using System.Text;
using Drug.NetEase.Server.Extensions;
using Drug.NetEase.Server.Utilities;

namespace Drug.NetEase.Server.BaseAuth;

public class MemoryCracker
{
    public static bool CrackCommonDll()
    {
        Native.FreeMemory((IntPtr)0);
        return Native.WriteProcessMemory(Native.GetCurrentProcess(),
            (IntPtr)WinHelper.GetModuleHandle(Native.DllName) + 76496,
            new uint[] { Convert.ToUInt32("B001C3", 16) }, 3U, (IntPtr)0);
    }
    public static string GetH5Token()
    {
        string result = string.Empty;
        try
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr zero2 = IntPtr.Zero;
            int h5Token = Native.GetH5Token(out zero2, out zero);
            if (h5Token != 0 || zero != IntPtr.Zero)
            {
                byte[] array = new byte[h5Token];
                Marshal.Copy(zero, array, 0, h5Token);
                Native.FreeMemory(zero2);
                Native.FreeMemory(zero);
                result = Encoding.UTF8.GetString(array);
            }
        }
        catch (Exception frg)
        {
        }
        return Convert.ToBase64String(result.ToBytes());;
    }
    public static string ParseLoginResponse(string data)
    {
        var datah = data.ToBytes();
        bool flag = datah == null || datah.Length == 0;
        string result;
        if (flag)
        {
            result = string.Empty;
        }
        else
        {
            IntPtr intPtr = Marshal.AllocHGlobal(checked(Marshal.SizeOf(datah[0]) * datah.Length));
            string text = string.Empty;
            int elj = datah.Length;
            try
            {
                Marshal.Copy(datah, 0, intPtr, datah.Length);
                IntPtr zero = IntPtr.Zero;
                IntPtr zero2 = IntPtr.Zero;
                int num = Native.ParseLoginResponse(intPtr, elj, out zero, out zero2);
                bool flag2 = num != 0 && zero != IntPtr.Zero;
                if (flag2)
                {
                    byte[] array = new byte[num];
                    Marshal.Copy(zero, array, 0, num);
                    Native.FreeMemory(zero);
                    text = Encoding.UTF8.GetString(array);
                }
                bool flag3 = zero2 != IntPtr.Zero;
                if (flag3)
                {
                    byte[] array2 = new byte[16];
                    Marshal.Copy(zero2, array2, 0, array2.Length);
                    Native.FreeMemory(zero2);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(intPtr);
            }
            result = text;
        }
        return result;
    }
    public class MemoryModify
    {
        public MemoryModify(string original, string modify)
        {
            Original = original;
            Modify = modify;
        }
        public string Original;
        public string Modify;
    }
}