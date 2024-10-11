using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Drug.NetEase.Client
{
	public class Native
	{
		[DllImport("kernel32.dll")]
		public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		// Token: 0x06000046 RID: 70
		[DllImport("kernel32.dll", EntryPoint = "VirtualQueryEx")]
		public static extern UIntPtr Native_VirtualQueryEx(IntPtr hProcess, UIntPtr lpAddress, out MEMORY_BASIC_INFORMATION32 lpBuffer, UIntPtr dwLength);

		// Token: 0x06000047 RID: 71
		[DllImport("kernel32.dll", EntryPoint = "VirtualQueryEx")]
		public static extern UIntPtr Native_VirtualQueryEx(IntPtr hProcess, UIntPtr lpAddress, out MEMORY_BASIC_INFORMATION64 lpBuffer, UIntPtr dwLength);

		// Token: 0x06000049 RID: 73
		[DllImport("kernel32.dll")]
		public static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		public static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

		[DllImport("kernel32.dll")]
		public static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

		[DllImport("kernel32.dll")]
		public static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] IntPtr lpBuffer, UIntPtr nSize, out ulong lpNumberOfBytesRead);

		[DllImport("kernel32.dll")]
		public static extern bool VirtualProtectEx(IntPtr hProcess, UIntPtr lpAddress, IntPtr dwSize, MemoryProtection flNewProtect, out MemoryProtection lpflOldProtect);

		[DllImport("kernel32.dll")]
		public static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesWritten);
		
		[DllImport("kernel32")]
		public static extern bool IsWow64Process(IntPtr hProcess, out bool lpSystemInfo);

		
		// Token: 0x02000012 RID: 18
		public struct SYSTEM_INFO
		{
			// Token: 0x04000061 RID: 97
			public ushort processorArchitecture;

			// Token: 0x04000062 RID: 98
			private ushort reserved;

			// Token: 0x04000063 RID: 99
			public uint pageSize;

			// Token: 0x04000064 RID: 100
			public UIntPtr minimumApplicationAddress;

			// Token: 0x04000065 RID: 101
			public UIntPtr maximumApplicationAddress;

			// Token: 0x04000066 RID: 102
			public IntPtr activeProcessorMask;

			// Token: 0x04000067 RID: 103
			public uint numberOfProcessors;

			// Token: 0x04000068 RID: 104
			public uint processorType;

			// Token: 0x04000069 RID: 105
			public uint allocationGranularity;

			// Token: 0x0400006A RID: 106
			public ushort processorLevel;

			// Token: 0x0400006B RID: 107
			public ushort processorRevision;
		}

		// Token: 0x02000013 RID: 19
		public struct MEMORY_BASIC_INFORMATION32
		{
			// Token: 0x0400006C RID: 108
			public UIntPtr BaseAddress;

			// Token: 0x0400006D RID: 109
			public UIntPtr AllocationBase;

			// Token: 0x0400006E RID: 110
			public uint AllocationProtect;

			// Token: 0x0400006F RID: 111
			public uint RegionSize;

			// Token: 0x04000070 RID: 112
			public uint State;

			// Token: 0x04000071 RID: 113
			public uint Protect;

			// Token: 0x04000072 RID: 114
			public uint Type;
		}

		// Token: 0x02000014 RID: 20
		public struct MEMORY_BASIC_INFORMATION64
		{
			// Token: 0x04000073 RID: 115
			public UIntPtr BaseAddress;

			// Token: 0x04000074 RID: 116
			public UIntPtr AllocationBase;

			// Token: 0x04000075 RID: 117
			public uint AllocationProtect;

			// Token: 0x04000076 RID: 118
			public uint __alignment1;

			// Token: 0x04000077 RID: 119
			public ulong RegionSize;

			// Token: 0x04000078 RID: 120
			public uint State;

			// Token: 0x04000079 RID: 121
			public uint Protect;

			// Token: 0x0400007A RID: 122
			public uint Type;

			// Token: 0x0400007B RID: 123
			public uint __alignment2;
		}

		// Token: 0x02000015 RID: 21
		public struct MEMORY_BASIC_INFORMATION
		{
			// Token: 0x0400007C RID: 124
			public UIntPtr BaseAddress;

			// Token: 0x0400007D RID: 125
			public UIntPtr AllocationBase;

			// Token: 0x0400007E RID: 126
			public uint AllocationProtect;

			// Token: 0x0400007F RID: 127
			public long RegionSize;

			// Token: 0x04000080 RID: 128
			public uint State;

			// Token: 0x04000081 RID: 129
			public uint Protect;

			// Token: 0x04000082 RID: 130
			public uint Type;
		}

		// Token: 0x02000016 RID: 22
		private enum SnapshotFlags : uint
		{
			// Token: 0x04000084 RID: 132
			HeapList = 1U,
			// Token: 0x04000085 RID: 133
			Process,
			// Token: 0x04000086 RID: 134
			Thread = 4U,
			// Token: 0x04000087 RID: 135
			Module = 8U,
			// Token: 0x04000088 RID: 136
			Module32 = 16U,
			// Token: 0x04000089 RID: 137
			Inherit = 2147483648U,
			// Token: 0x0400008A RID: 138
			All = 31U,
			// Token: 0x0400008B RID: 139
			NoHeaps = 1073741824U
		}

		// Token: 0x02000017 RID: 23
		[Flags]
		public enum ThreadAccess
		{
			// Token: 0x0400008D RID: 141
			TERMINATE = 1,
			// Token: 0x0400008E RID: 142
			SUSPEND_RESUME = 2,
			// Token: 0x0400008F RID: 143
			GET_CONTEXT = 8,
			// Token: 0x04000090 RID: 144
			SET_CONTEXT = 16,
			// Token: 0x04000091 RID: 145
			SET_INFORMATION = 32,
			// Token: 0x04000092 RID: 146
			QUERY_INFORMATION = 64,
			// Token: 0x04000093 RID: 147
			SET_THREAD_TOKEN = 128,
			// Token: 0x04000094 RID: 148
			IMPERSONATE = 256,
			// Token: 0x04000095 RID: 149
			DIRECT_IMPERSONATION = 512
		}

		// Token: 0x02000018 RID: 24
		[Flags]
		public enum MemoryProtection : uint
		{
			// Token: 0x04000097 RID: 151
			Execute = 16U,
			// Token: 0x04000098 RID: 152
			ExecuteRead = 32U,
			// Token: 0x04000099 RID: 153
			ExecuteReadWrite = 64U,
			// Token: 0x0400009A RID: 154
			ExecuteWriteCopy = 128U,
			// Token: 0x0400009B RID: 155
			NoAccess = 1U,
			// Token: 0x0400009C RID: 156
			ReadOnly = 2U,
			// Token: 0x0400009D RID: 157
			ReadWrite = 4U,
			// Token: 0x0400009E RID: 158
			WriteCopy = 8U,
			// Token: 0x0400009F RID: 159
			GuardModifierflag = 256U,
			// Token: 0x040000A0 RID: 160
			NoCacheModifierflag = 512U,
			// Token: 0x040000A1 RID: 161
			WriteCombineModifierflag = 1024U
		}

		// Token: 0x02000019 RID: 25
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct PROCESSENTRY32
		{
			// Token: 0x040000A2 RID: 162
			private const int MAX_PATH = 260;

			// Token: 0x040000A3 RID: 163
			internal uint dwSize;

			// Token: 0x040000A4 RID: 164
			internal uint cntUsage;

			// Token: 0x040000A5 RID: 165
			internal uint th32ProcessID;

			// Token: 0x040000A6 RID: 166
			internal IntPtr th32DefaultHeapID;

			// Token: 0x040000A7 RID: 167
			internal uint th32ModuleID;

			// Token: 0x040000A8 RID: 168
			internal uint cntThreads;

			// Token: 0x040000A9 RID: 169
			internal uint th32ParentProcessID;

			// Token: 0x040000AA RID: 170
			internal int pcPriClassBase;

			// Token: 0x040000AB RID: 171
			internal uint dwFlags;

			// Token: 0x040000AC RID: 172
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			internal string szExeFile;
		}

		// Token: 0x0200001A RID: 26
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct MODULEENTRY32
		{
			// Token: 0x040000AD RID: 173
			internal uint dwSize;

			// Token: 0x040000AE RID: 174
			internal uint th32ModuleID;

			// Token: 0x040000AF RID: 175
			internal uint th32ProcessID;

			// Token: 0x040000B0 RID: 176
			internal uint GlblcntUsage;

			// Token: 0x040000B1 RID: 177
			internal uint ProccntUsage;

			// Token: 0x040000B2 RID: 178
			internal IntPtr modBaseAddr;

			// Token: 0x040000B3 RID: 179
			internal uint modBaseSize;

			// Token: 0x040000B4 RID: 180
			internal IntPtr hModule;

			// Token: 0x040000B5 RID: 181
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			internal string szModule;

			// Token: 0x040000B6 RID: 182
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			internal string szExePath;
		}
	}
}
