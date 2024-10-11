using System;
using System.Runtime.InteropServices;

namespace Drug.NetEase.Server;

public class Native
{
    public const string DllName = "mcl.common.dll";
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    public static extern void FreeMemory(IntPtr dop);
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    public static extern int ParseLoginResponse(IntPtr eli, int elj, out IntPtr elk, out IntPtr ell);
    [DllImport("mcl.common.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    public static extern int GetH5Token(out IntPtr fku, out IntPtr fkv);
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetCurrentProcess();
    [DllImport("kernel32.dll")]
    public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, uint[] lpBuffer, uint nSize, IntPtr lpNumberOfBytesWritten);
}