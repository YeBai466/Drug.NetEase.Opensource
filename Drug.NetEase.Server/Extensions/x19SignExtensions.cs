using System;
using System.Collections.Generic;
using System.Text;

namespace Drug.NetEase.Server.Extensions
{
	public static class x19SignExtensions
	{
		private const string KEY = "942894570397f6d1c9cca2535ad18a2b";
		
		public static string Encrypt(this string data)
		{
			return "!x19sign!" + data.Encrypt(KEY);
		}

		public static string Decrypt(this string data)
		{
			return data.StartsWith("!x19sign!") ? data.Remove(0, "!x19sign!".Length).Decrypt(KEY) : data;
		}
		
		private static string Encrypt(this string data, string key)
		{
			return EncryptData(Encoding.UTF8.GetBytes(data.PadRight(32, '\0')).ToLongArray(), Encoding.UTF8.GetBytes(key.PadRight(32, '\0')).ToLongArray()).ToHexString();
		}

		private static string Decrypt(this string data, string key)
		{
			string result;
			if (string.IsNullOrWhiteSpace(data))
			{
				result = data;
			}
			else
			{
				byte[] array = DecryptData(data.HexStringToLongArray(), Encoding.UTF8.GetBytes(key.PadRight(32, '\0')).ToLongArray()).ToByteArray();
				result = Encoding.UTF8.GetString(array, 0, array.Length);
			}
			return result;
		}

		private static long[] EncryptData(long[] data, long[] key)
		{
			int num = data.Length;
			long[] result;
			if (num < 1)
			{
				result = data;
			}
			else
			{
				long num2 = data[data.Length - 1];
				long num3 = data[0];
				long num4 = 0L;
				long num5 = (long)(6 + 52 / num);
				for (;;)
				{
					long num6 = num5;
					num5 = num6 - 1L;
					if (num6 <= 0L)
					{
						break;
					}
					num4 += 2654435769L;
					long num7 = (num4 >> 2) & 3L;
					long num8;
					for (num8 = 0L; num8 < (long)(num - 1); num8 += 1L)
					{
						num3 = data[(int)(checked((IntPtr)(unchecked(num8 + 1L))))];
						num2 = (data[(int)(checked((IntPtr)num8))] += (((num2 >> 5) ^ (num3 << 2)) + ((num3 >> 3) ^ (num2 << 4))) ^ ((num4 ^ num3) + (key[(int)(checked((IntPtr)((num8 & 3L) ^ num7)))] ^ num2)));
					}
					num3 = data[0];
					num2 = (data[num - 1] += (((num2 >> 5) ^ (num3 << 2)) + ((num3 >> 3) ^ (num2 << 4))) ^ ((num4 ^ num3) + (key[(int)(checked((IntPtr)((num8 & 3L) ^ num7)))] ^ num2)));
				}
				result = data;
			}
			return result;
		}
		private static long[] DecryptData(long[] data, long[] key)
		{
			int num = data.Length;
			long[] result;
			if (num < 1)
			{
				result = data;
			}
			else
			{
				long num2 = data[data.Length - 1];
				long num3 = data[0];
				long num4 = (long)(6 + 52 / num);
				for (long num5 = num4 * 2654435769L; num5 != 0L; num5 -= 2654435769L)
				{
					long num6 = (num5 >> 2) & 3L;
					long num7;
					for (num7 = (long)(num - 1); num7 > 0L; num7 -= 1L)
					{
						num2 = data[(int)(checked((IntPtr)(unchecked(num7 - 1L))))];
						num3 = (data[(int)(checked((IntPtr)num7))] -= (((num2 >> 5) ^ (num3 << 2)) + ((num3 >> 3) ^ (num2 << 4))) ^ ((num5 ^ num3) + (key[(int)(checked((IntPtr)((num7 & 3L) ^ num6)))] ^ num2)));
					}
					num2 = data[num - 1];
					num3 = (data[0] -= (((num2 >> 5) ^ (num3 << 2)) + ((num3 >> 3) ^ (num2 << 4))) ^ ((num5 ^ num3) + (key[(int)(checked((IntPtr)((num7 & 3L) ^ num6)))] ^ num2)));
				}
				result = data;
			}
			return result;
		}

		private static long[] ToLongArray(this byte[] byteArray)
		{
			int num = ((byteArray.Length % 8 == 0) ? 0 : 1) + byteArray.Length / 8;
			long[] array = new long[num];
			for (int i = 0; i < num - 1; i++)
			{
				array[i] = BitConverter.ToInt64(byteArray, i * 8);
			}
			byte[] array2 = new byte[8];
			Array.Copy(byteArray, (num - 1) * 8, array2, 0, byteArray.Length - (num - 1) * 8);
			array[num - 1] = BitConverter.ToInt64(array2, 0);
			return array;
		}

		private static byte[] ToByteArray(this long[] longArray)
		{
			List<byte> list = new List<byte>(longArray.Length * 8);
			for (int i = 0; i < longArray.Length; i++)
			{
				list.AddRange(BitConverter.GetBytes(longArray[i]));
			}
			while (list[list.Count - 1] == 0)
			{
				list.RemoveAt(list.Count - 1);
			}
			return list.ToArray();
		}

		private static string ToHexString(this long[] longArray)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < longArray.Length; i++)
			{
				stringBuilder.Append(longArray[i].ToString("x2").PadLeft(16, '0'));
			}
			return stringBuilder.ToString();
		}

		private static long[] HexStringToLongArray(this string hexString)
		{
			int num = hexString.Length / 16;
			long[] array = new long[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = Convert.ToInt64(hexString.Substring(i * 16, 16), 16);
			}
			return array;
		}
	}
}
