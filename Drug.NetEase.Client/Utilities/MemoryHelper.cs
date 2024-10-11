using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Drug.NetEase.Client.Utilities;

public class MemoryHelper
{
	private UIntPtr VirtualQueryEx(IntPtr hProcess, UIntPtr lpAddress, out Native.MEMORY_BASIC_INFORMATION lpBuffer)
	{
		var flag = _mProc.Is64Bit || IntPtr.Size == 8;
		checked
		{
			UIntPtr result;
			if (flag)
			{
				var memory_BASIC_INFORMATION = default(Native.MEMORY_BASIC_INFORMATION64);
				var uintPtr = Native.Native_VirtualQueryEx(hProcess, lpAddress, out memory_BASIC_INFORMATION,
					new UIntPtr((uint)Marshal.SizeOf(memory_BASIC_INFORMATION)));
				lpBuffer.BaseAddress = memory_BASIC_INFORMATION.BaseAddress;
				lpBuffer.AllocationBase = memory_BASIC_INFORMATION.AllocationBase;
				lpBuffer.AllocationProtect = memory_BASIC_INFORMATION.AllocationProtect;
				lpBuffer.RegionSize = (long)memory_BASIC_INFORMATION.RegionSize;
				lpBuffer.State = memory_BASIC_INFORMATION.State;
				lpBuffer.Protect = memory_BASIC_INFORMATION.Protect;
				lpBuffer.Type = memory_BASIC_INFORMATION.Type;
				result = uintPtr;
			}
			else
			{
				var memory_BASIC_INFORMATION2 = default(Native.MEMORY_BASIC_INFORMATION32);
				var uintPtr = Native.Native_VirtualQueryEx(hProcess, lpAddress, out memory_BASIC_INFORMATION2,
					new UIntPtr((uint)Marshal.SizeOf(memory_BASIC_INFORMATION2)));
				lpBuffer.BaseAddress = memory_BASIC_INFORMATION2.BaseAddress;
				lpBuffer.AllocationBase = memory_BASIC_INFORMATION2.AllocationBase;
				lpBuffer.AllocationProtect = memory_BASIC_INFORMATION2.AllocationProtect;
				lpBuffer.RegionSize = memory_BASIC_INFORMATION2.RegionSize;
				lpBuffer.State = memory_BASIC_INFORMATION2.State;
				lpBuffer.Protect = memory_BASIC_INFORMATION2.Protect;
				lpBuffer.Type = memory_BASIC_INFORMATION2.Type;
				result = uintPtr;
			}

			return result;
		}
	}

	private bool OpenProcess(int pid, out string FailReason)
	{
		var flag = pid <= 0;
		bool result;
		if (flag)
		{
			FailReason = "OpenProcess given proc ID 0.";
			result = false;
		}
		else
		{
			var flag2 = _mProc.Process != null && _mProc.Process.Id == pid;
			if (flag2)
			{
				FailReason = "_mProc.Process is null";
				result = true;
			}
			else
			{
				try
				{
					_mProc.Process = Process.GetProcessById(pid);
					var flag3 = _mProc.Process != null && !_mProc.Process.Responding;
					if (flag3)
					{
						FailReason = "Process is not responding or null.";
						result = false;
					}
					else
					{
						_mProc.Handle = Native.OpenProcess(2035711U, true, pid);
						try
						{
							Process.EnterDebugMode();
						}
						catch (Win32Exception)
						{
						}

						var flag4 = _mProc.Handle == IntPtr.Zero;
						if (flag4)
						{
							var lastWin32Error = Marshal.GetLastWin32Error();
							Process.LeaveDebugMode();
							_mProc = null;
							FailReason = "failed opening a handle to the target process(GetLastWin32ErrorCode: " +
							             lastWin32Error + ")";
							result = false;
						}
						else
						{
							bool flag5;
							_mProc.Is64Bit = Environment.Is64BitOperatingSystem &&
							                Native.IsWow64Process(_mProc.Handle, out flag5) && !flag5;
							_mProc.MainModule = _mProc.Process.MainModule;
							var str = "Process #";
							FailReason = "";
							result = true;
						}
					}
				}
				catch (Exception ex)
				{
					var str2 = "ERROR: OpenProcess has crashed. ";
					var str3 = "OpenProcess has crashed. ";
					var ex3 = ex;
					FailReason = str3 + (ex3 != null ? ex3.ToString() : null);
					result = false;
				}
			}
		}

		return result;
	}

	public bool OpenProcess(string proc)
	{
		string text;
		return OpenProcess(GetProcIdFromName(proc), out text);
	}

	private int GetProcIdFromName(string name)
	{
		var processes = Process.GetProcesses();
		var flag = name.ToLower().Contains(".exe");
		if (flag) name = name.Replace(".exe", "");
		var flag2 = name.ToLower().Contains(".bin");
		if (flag2) name = name.Replace(".bin", "");
		foreach (var process in processes)
		{
			var flag3 = process.ProcessName.Equals(name, StringComparison.CurrentCultureIgnoreCase);
			if (flag3) return process.Id;
		}

		return 0;
	}

	private string LoadCode(string name, string iniFile)
	{
		var stringBuilder = new StringBuilder(1024);
		var flag = !string.IsNullOrEmpty(iniFile);
		if (flag)
		{
			var flag2 = File.Exists(iniFile);
			if (flag2)
			{
				var privateProfileString = Native.GetPrivateProfileString("codes", name, "", stringBuilder,
					checked((uint)stringBuilder.Capacity), iniFile);
			}
			else
			{
			}
		}
		else
		{
			stringBuilder.Append(name);
		}

		return stringBuilder.ToString();
	}


	private bool ChangeProtection(string code, Native.MemoryProtection newProtection,
		out Native.MemoryProtection oldProtection, string file = "")
	{
		var code2 = GetCode(code, file);
		var flag = code2 == UIntPtr.Zero || _mProc.Handle == IntPtr.Zero;
		bool result;
		if (flag)
		{
			oldProtection = 0U;
			result = false;
		}
		else
		{
			result = Native.VirtualProtectEx(_mProc.Handle, code2, (IntPtr)(_mProc.Is64Bit ? 8 : 4), newProtection,
				out oldProtection);
		}

		return result;
	}

	private UIntPtr GetCode(string name, string path = "", int size = 8)
	{
		var text = "";
		bool flag = _mProc == null;
		checked
		{
			UIntPtr result;
			if (flag)
			{
				result = UIntPtr.Zero;
			}
			else
			{
				bool is64Bit = _mProc.Is64Bit;
				if (is64Bit)
				{
					var flag2 = size == 8;
					if (flag2) size = 16;
					result = Get64BitCode(name, path, size);
				}
				else
				{
					var flag3 = !string.IsNullOrEmpty(path);
					if (flag3)
						text = LoadCode(name, path);
					else
						text = name;
					var flag4 = string.IsNullOrEmpty(text);
					if (flag4)
					{
						result = UIntPtr.Zero;
					}
					else
					{
						var flag5 = text.Contains(" ");
						if (flag5) text = text.Replace(" ", string.Empty);
						var flag6 = !text.Contains("+") && !text.Contains(",");
						if (flag6)
							try
							{
								return new UIntPtr(Convert.ToUInt32(text, 16));
							}
							catch
							{
								Console.WriteLine("Error in GetCode(). Failed to read address " + text);
								return UIntPtr.Zero;
							}

						var text2 = text;
						var flag7 = text.Contains("+");
						if (flag7) text2 = text.Substring(text.IndexOf('+') + 1);
						var array = new byte[size];
						var flag8 = text2.Contains(',');
						if (flag8)
						{
							var list = new List<int>();
							var array2 = text2.Split(',');
							foreach (var text3 in array2)
							{
								var text4 = text3;
								var flag9 = text3.Contains("0x");
								if (flag9) text4 = text3.Replace("0x", "");
								var flag10 = !text3.Contains("-");
								int num;
								if (flag10)
								{
									num = int.Parse(text4, NumberStyles.AllowHexSpecifier);
								}
								else
								{
									text4 = text4.Replace("-", "");
									num = int.Parse(text4, NumberStyles.AllowHexSpecifier);
									num *= -1;
								}

								list.Add(num);
							}

							var array4 = list.ToArray();
							var flag11 = text.Contains("base") || text.Contains("main");
							if (flag11)
							{
								Native.ReadProcessMemory(_mProc.Handle,
									(UIntPtr)(ulong)((int)_mProc.MainModule.BaseAddress + array4[0]), array,
									(UIntPtr)(ulong)size, IntPtr.Zero);
							}
							else
							{
								var flag12 = !text.Contains("base") && !text.Contains("main") && text.Contains("+");
								if (flag12)
								{
									var array5 = text.Split('+');
									var value = IntPtr.Zero;
									var flag13 = !array5[0].ToLower().Contains(".dll") &&
									             !array5[0].ToLower().Contains(".exe") &&
									             !array5[0].ToLower().Contains(".bin");
									if (flag13)
									{
										var text5 = array5[0];
										var flag14 = text5.Contains("0x");
										if (flag14) text5 = text5.Replace("0x", "");
										value = (IntPtr)int.Parse(text5, NumberStyles.HexNumber);
									}
									else
									{
										try
										{
											value = GetModuleAddressByName(array5[0]);
										}
										catch
										{
										}
									}

									Native.ReadProcessMemory(_mProc.Handle, (UIntPtr)(ulong)((int)value + array4[0]),
										array, (UIntPtr)(ulong)size, IntPtr.Zero);
								}
								else
								{
									Native.ReadProcessMemory(_mProc.Handle, (UIntPtr)(ulong)array4[0], array,
										(UIntPtr)(ulong)size, IntPtr.Zero);
								}
							}

							var num2 = BitConverter.ToUInt32(array, 0);
							var uintPtr = (UIntPtr)0UL;
							for (var j = 1; j < array4.Length; j++)
							{
								uintPtr = new UIntPtr(Convert.ToUInt32((long)(num2 + (ulong)array4[j])));
								Native.ReadProcessMemory(_mProc.Handle, uintPtr, array, (UIntPtr)(ulong)size,
									IntPtr.Zero);
								num2 = BitConverter.ToUInt32(array, 0);
							}

							result = uintPtr;
						}
						else
						{
							var num3 = Convert.ToInt32(text2, 16);
							var value2 = IntPtr.Zero;
							var flag15 = text.ToLower().Contains("base") || text.ToLower().Contains("main");
							if (flag15)
							{
								value2 = _mProc.MainModule.BaseAddress;
							}
							else
							{
								var flag16 = !text.ToLower().Contains("base") && !text.ToLower().Contains("main") &&
								             text.Contains("+");
								if (flag16)
								{
									var array6 = text.Split('+');
									var flag17 = !array6[0].ToLower().Contains(".dll") &&
									             !array6[0].ToLower().Contains(".exe") &&
									             !array6[0].ToLower().Contains(".bin");
									if (flag17)
									{
										var text6 = array6[0];
										var flag18 = text6.Contains("0x");
										if (flag18) text6 = text6.Replace("0x", "");
										value2 = (IntPtr)int.Parse(text6, NumberStyles.HexNumber);
									}
									else
									{
										try
										{
											value2 = GetModuleAddressByName(array6[0]);
										}
										catch
										{
										}
									}
								}
								else
								{
									value2 = GetModuleAddressByName(text.Split('+')[0]);
								}
							}

							result = (UIntPtr)(ulong)((int)value2 + num3);
						}
					}
				}
			}

			return result;
		}
	}

	private IntPtr GetModuleAddressByName(string name)
	{
		return _mProc.Process.Modules.Cast<ProcessModule>().SingleOrDefault((ProcessModule m) =>
			string.Equals(m.ModuleName, name, StringComparison.OrdinalIgnoreCase)).BaseAddress;
	}
	public UIntPtr Get64BitCode(string name, string path = "", int size = 16)
	{
		var text = "";
		var flag = !string.IsNullOrEmpty(path);
		if (flag)
			text = LoadCode(name, path);
		else
			text = name;
		var flag2 = string.IsNullOrEmpty(text);
		checked
		{
			UIntPtr result;
			if (flag2)
			{
				result = UIntPtr.Zero;
			}
			else
			{
				var flag3 = text.Contains(" ");
				if (flag3) text.Replace(" ", string.Empty);
				var text2 = text;
				var flag4 = text.Contains("+");
				if (flag4) text2 = text.Substring(text.IndexOf('+') + 1);
				var array = new byte[size];
				var flag5 = !text.Contains("+") && !text.Contains(",");
				if (flag5)
					try
					{
						return new UIntPtr(Convert.ToUInt64(text, 16));
					}
					catch
					{
						Console.WriteLine("Error in GetCode(). Failed to read address " + text);
						return UIntPtr.Zero;
					}

				var flag6 = text2.Contains(',');
				if (flag6)
				{
					var list = new List<long>();
					var array2 = text2.Split(',');
					foreach (var text3 in array2)
					{
						var text4 = text3;
						var flag7 = text3.Contains("0x");
						if (flag7) text4 = text3.Replace("0x", "");
						var flag8 = !text3.Contains("-");
						long num;
						if (flag8)
						{
							num = long.Parse(text4, NumberStyles.AllowHexSpecifier);
						}
						else
						{
							text4 = text4.Replace("-", "");
							num = long.Parse(text4, NumberStyles.AllowHexSpecifier);
							num *= -1L;
						}

						list.Add(num);
					}

					var array4 = list.ToArray();
					var flag9 = text.Contains("base") || text.Contains("main");
					if (flag9)
					{
						Native.ReadProcessMemory(_mProc.Handle,
							(UIntPtr)(ulong)((long)_mProc.MainModule.BaseAddress + array4[0]), array,
							(UIntPtr)(ulong)size, IntPtr.Zero);
					}
					else
					{
						var flag10 = !text.Contains("base") && !text.Contains("main") && text.Contains("+");
						if (flag10)
						{
							var array5 = text.Split('+');
							var value = IntPtr.Zero;
							var flag11 = !array5[0].ToLower().Contains(".dll") &&
							             !array5[0].ToLower().Contains(".exe") && !array5[0].ToLower().Contains(".bin");
							if (flag11)
								value = (IntPtr)long.Parse(array5[0], NumberStyles.HexNumber);
							else
								try
								{
									value = GetModuleAddressByName(array5[0]);
								}
								catch
								{
								}

							Native.ReadProcessMemory(_mProc.Handle, (UIntPtr)(ulong)((long)value + array4[0]), array,
								(UIntPtr)(ulong)size, IntPtr.Zero);
						}
						else
						{
							Native.ReadProcessMemory(_mProc.Handle, (UIntPtr)(ulong)array4[0], array,
								(UIntPtr)(ulong)size, IntPtr.Zero);
						}
					}

					var num2 = BitConverter.ToInt64(array, 0);
					var uintPtr = (UIntPtr)0UL;
					for (var j = 1; j < array4.Length; j++)
					{
						uintPtr = new UIntPtr(Convert.ToUInt64(num2 + array4[j]));
						Native.ReadProcessMemory(_mProc.Handle, uintPtr, array, (UIntPtr)(ulong)size, IntPtr.Zero);
						num2 = BitConverter.ToInt64(array, 0);
					}

					result = uintPtr;
				}
				else
				{
					var num3 = Convert.ToInt64(text2, 16);
					var value2 = IntPtr.Zero;
					var flag12 = text.Contains("base") || text.Contains("main");
					if (flag12)
					{
						value2 = _mProc.MainModule.BaseAddress;
					}
					else
					{
						var flag13 = !text.Contains("base") && !text.Contains("main") && text.Contains("+");
						if (flag13)
						{
							var array6 = text.Split('+');
							var flag14 = !array6[0].ToLower().Contains(".dll") &&
							             !array6[0].ToLower().Contains(".exe") && !array6[0].ToLower().Contains(".bin");
							if (flag14)
							{
								var text5 = array6[0];
								var flag15 = text5.Contains("0x");
								if (flag15) text5 = text5.Replace("0x", "");
								value2 = (IntPtr)long.Parse(text5, NumberStyles.HexNumber);
							}
							else
							{
								try
								{
									value2 = GetModuleAddressByName(array6[0]);
								}
								catch
								{
								}
							}
						}
						else
						{
							value2 = GetModuleAddressByName(text.Split('+')[0]);
						}
					}

					result = (UIntPtr)(ulong)((long)value2 + num3);
				}
			}

			return result;
		}
	}
	public Task<IEnumerable<long>> AoBScan(string search, bool readable, bool writable, bool executable,
		string file = "")
	{
		return AoBScan(0L, long.MaxValue, search, readable, writable, executable, file);
	}
	
	private Task<IEnumerable<long>> AoBScan(long start, long end, string search, bool readable, bool writable,
		bool executable, string file = "")
	{
		return Task.Run(checked(delegate
		{
			List<MemoryRegionResult> list = new();
			var text = LoadCode(search, file);
			var array = text.Split(' ');
			var aobPattern = new byte[array.Length];
			var mask = new byte[array.Length];
			for (var i = 0; i < array.Length; i++)
			{
				var text2 = array[i];
				var flag = text2 == "??" || (text2.Length == 1 && text2 == "?");
				if (flag)
				{
					mask[i] = 0;
					array[i] = "0x00";
				}
				else
				{
					var flag2 = char.IsLetterOrDigit(text2[0]) && text2[1] == '?';
					if (flag2)
					{
						mask[i] = 240;
						array[i] = text2[0] + "0";
					}
					else
					{
						var flag3 = char.IsLetterOrDigit(text2[1]) && text2[0] == '?';
						if (flag3)
						{
							mask[i] = 15;
							array[i] = "0" + text2[1];
						}
						else
						{
							mask[i] = byte.MaxValue;
						}
					}
				}
			}

			for (var j = 0; j < array.Length; j++) aobPattern[j] = Convert.ToByte(Convert.ToByte(array[j], 16) & mask[j]);
			var system_INFO = default(Native.SYSTEM_INFO);
			Native.GetSystemInfo(out system_INFO);
			var minimumApplicationAddress = system_INFO.minimumApplicationAddress;
			var maximumApplicationAddress = system_INFO.maximumApplicationAddress;
			var flag4 = start < (long)minimumApplicationAddress.ToUInt64();
			if (flag4) start = (long)minimumApplicationAddress.ToUInt64();
			var flag5 = end > (long)maximumApplicationAddress.ToUInt64();
			if (flag5) end = (long)maximumApplicationAddress.ToUInt64();
			var uintPtr = new UIntPtr((ulong)start);
			var memory_BASIC_INFORMATION = default(Native.MEMORY_BASIC_INFORMATION);
			while (VirtualQueryEx(_mProc.Handle, uintPtr, out memory_BASIC_INFORMATION).ToUInt64() != 0UL &&
			       uintPtr.ToUInt64() < (ulong)end && uintPtr.ToUInt64() + (ulong)memory_BASIC_INFORMATION.RegionSize >
			       uintPtr.ToUInt64())
			{
				var flag6 = memory_BASIC_INFORMATION.State == 4096U;
				flag6 &= memory_BASIC_INFORMATION.BaseAddress.ToUInt64() < maximumApplicationAddress.ToUInt64();
				flag6 &= (memory_BASIC_INFORMATION.Protect & 256U) == 0U;
				flag6 &= (memory_BASIC_INFORMATION.Protect & 1U) == 0U;
				flag6 &= memory_BASIC_INFORMATION.Type == 131072U || memory_BASIC_INFORMATION.Type == 16777216U;
				var flag7 = flag6;
				if (flag7)
				{
					var flag8 = (memory_BASIC_INFORMATION.Protect & 2U) > 0U;
					var flag9 = (memory_BASIC_INFORMATION.Protect & 4U) > 0U ||
					            (memory_BASIC_INFORMATION.Protect & 8U) > 0U ||
					            (memory_BASIC_INFORMATION.Protect & 64U) > 0U ||
					            (memory_BASIC_INFORMATION.Protect & 128U) > 0U;
					var flag10 = (memory_BASIC_INFORMATION.Protect & 16U) > 0U ||
					             (memory_BASIC_INFORMATION.Protect & 32U) > 0U ||
					             (memory_BASIC_INFORMATION.Protect & 64U) > 0U ||
					             (memory_BASIC_INFORMATION.Protect & 128U) > 0U;
					flag8 &= readable;
					flag9 &= writable;
					flag10 &= executable;
					flag6 &= flag8 || flag9 || flag10;
				}

				var flag11 = !flag6;
				if (flag11)
				{
					uintPtr = new UIntPtr(memory_BASIC_INFORMATION.BaseAddress.ToUInt64() +
					                      (ulong)memory_BASIC_INFORMATION.RegionSize);
				}
				else
				{
					MemoryRegionResult item2 = new MemoryRegionResult
					{
						CurrentBaseAddress = uintPtr,
						RegionSize = memory_BASIC_INFORMATION.RegionSize,
						RegionBase = memory_BASIC_INFORMATION.BaseAddress
					};
					uintPtr = new UIntPtr(memory_BASIC_INFORMATION.BaseAddress.ToUInt64() +
					                      (ulong)memory_BASIC_INFORMATION.RegionSize);
					var flag12 = list.Count > 0;
					if (flag12)
					{
						MemoryRegionResult memoryRegionResult = list[list.Count - 1];
						var flag13 = (long)(ulong)memoryRegionResult.RegionBase + memoryRegionResult.RegionSize ==
						             (long)(ulong)memory_BASIC_INFORMATION.BaseAddress;
						if (flag13)
						{
							list[list.Count - 1] = new MemoryRegionResult
							{
								CurrentBaseAddress = memoryRegionResult.CurrentBaseAddress,
								RegionBase = memoryRegionResult.RegionBase,
								RegionSize = memoryRegionResult.RegionSize + memory_BASIC_INFORMATION.RegionSize
							};
							continue;
						}
					}

					list.Add(item2);
				}
			}

			var bagResult = new ConcurrentBag<long>();
			Parallel.ForEach(list, delegate(MemoryRegionResult item, ParallelLoopState parallelLoopState, long index)
			{
				var array2 = CompareScan(item, aobPattern, mask);
				foreach (var item3 in array2) bagResult.Add(item3);
			});
			return (from c in bagResult.ToList()
				orderby c
				select c).AsEnumerable();
		}));
	}

	private unsafe long[] CompareScan(MemoryRegionResult item, byte[] aobPattern, byte[] mask)
	{
		var flag = mask.Length != aobPattern.Length;
		if (flag) throw new ArgumentException("aobPattern.Length != mask.Length");
		checked
		{
			var intPtr = Marshal.AllocHGlobal((int)item.RegionSize);
			ulong num;
			Native.ReadProcessMemory(_mProc.Handle, item.CurrentBaseAddress, intPtr, (UIntPtr)(ulong)item.RegionSize,
				out num);
			var num2 = 0 - aobPattern.Length;
			var list = new List<long>();
			do
			{
				num2 = FindPattern((byte*)intPtr.ToPointer(), (int)num, aobPattern, mask, num2 + aobPattern.Length);
				var flag2 = num2 >= 0;
				if (flag2) list.Add((long)(ulong)item.CurrentBaseAddress + num2);
			} while (num2 != -1);

			Marshal.FreeHGlobal(intPtr);
			return list.ToArray();
		}
	}

	private unsafe int FindPattern(byte* body, int bodyLength, byte[] pattern, byte[] masks, int start = 0)
	{
		var num = -1;
		checked
		{
			var flag = bodyLength <= 0 || pattern.Length == 0 || start > bodyLength - pattern.Length ||
			           pattern.Length > bodyLength;
			int result;
			if (flag)
			{
				result = num;
			}
			else
			{
				for (var i = start; i <= bodyLength - pattern.Length; i++)
				{
					var flag2 = (body[i] & masks[0]) == (pattern[0] & masks[0]);
					if (flag2)
					{
						var flag3 = true;
						for (var j = pattern.Length - 1; j >= 1; j--)
						{
							var flag4 = (body[checked(i + j)] & masks[j]) == (pattern[j] & masks[j]);
							if (!flag4)
							{
								flag3 = false;
								break;
							}
						}

						var flag5 = !flag3;
						if (!flag5)
						{
							num = i;
							break;
						}
					}
				}

				result = num;
			}

			return result;
		}
	}


	public bool WriteMemory(string code, string type, string write, string file = "", Encoding stringEncoding = null,
		bool RemoveWriteProtection = true)
	{
		var array = new byte[4];
		var num = 4;
		var code2 = GetCode(code, file);
		var flag = code2 == UIntPtr.Zero || code2.ToUInt64() < 65536UL;
		checked
		{
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				var flag2 = type.ToLower() == "float";
				if (flag2)
				{
					write = Convert.ToString(float.Parse(write, CultureInfo.InvariantCulture));
					array = BitConverter.GetBytes(Convert.ToSingle(write));
					num = 4;
				}
				else
				{
					var flag3 = type.ToLower() == "int";
					if (flag3)
					{
						array = BitConverter.GetBytes(Convert.ToInt32(write));
						num = 4;
					}
					else
					{
						var flag4 = type.ToLower() == "byte";
						if (flag4)
						{
							array = new[] { Convert.ToByte(write, 16) };
							num = 1;
						}
						else
						{
							var flag5 = type.ToLower() == "2bytes";
							if (flag5)
							{
								array = new[]
								{
									(byte)(Convert.ToInt32(write) % 256),
									(byte)(Convert.ToInt32(write) / 256)
								};
								num = 2;
							}
							else
							{
								var flag6 = type.ToLower() == "bytes";
								if (flag6)
								{
									var flag7 = write.Contains(",") || write.Contains(" ");
									if (flag7)
									{
										var flag8 = write.Contains(",");
										string[] array2;
										if (flag8)
											array2 = write.Split(',');
										else
											array2 = write.Split(' ');
										var num2 = array2.Count();
										array = new byte[num2];
										for (var i = 0; i < num2; i++) array[i] = Convert.ToByte(array2[i], 16);
										num = array2.Count();
									}
									else
									{
										array = new[] { Convert.ToByte(write, 16) };
										num = 1;
									}
								}
								else
								{
									var flag9 = type.ToLower() == "double";
									if (flag9)
									{
										array = BitConverter.GetBytes(Convert.ToDouble(write));
										num = 8;
									}
									else
									{
										var flag10 = type.ToLower() == "long";
										if (flag10)
										{
											array = BitConverter.GetBytes(Convert.ToInt64(write));
											num = 8;
										}
										else
										{
											var flag11 = type.ToLower() == "string";
											if (flag11)
											{
												var flag12 = stringEncoding == null;
												if (flag12)
													array = Encoding.UTF8.GetBytes(write);
												else
													array = stringEncoding.GetBytes(write);
												num = array.Length;
											}
										}
									}
								}
							}
						}
					}
				}

				Native.MemoryProtection newProtection = 0U;
				if (RemoveWriteProtection)
					ChangeProtection(code, Native.MemoryProtection.ExecuteReadWrite, out newProtection, file);
				var flag13 = Native.WriteProcessMemory(_mProc.Handle, code2, array, (UIntPtr)(ulong)num, IntPtr.Zero);
				if (RemoveWriteProtection)
				{
					ChangeProtection(code, newProtection, out var memoryProtection, file);
				}

				result = flag13;
			}

			return result;
		}
	}

	private Proc _mProc = new ();
	private struct MemoryRegionResult
	{
		public UIntPtr CurrentBaseAddress;
		public long RegionSize;
		public UIntPtr RegionBase;
	}
	private class Proc
	{
		public Process Process;
		public IntPtr Handle;
		public bool Is64Bit;
		public ProcessModule MainModule;
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