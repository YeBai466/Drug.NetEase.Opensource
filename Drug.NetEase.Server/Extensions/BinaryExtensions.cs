using System;
using System.Text;

namespace Drug.NetEase.Server.Extensions;

public static class BinaryExtensions
{
    public static string ToBinary(this byte[] buffer)
    {
        StringBuilder sb = new StringBuilder(buffer.Length * 8);
        foreach (var item in buffer)
        {
            var itemBinary = Convert.ToString(item, 2);

            for (int i = 0; i < 8 - itemBinary.Length; i++)
                sb.Append('0');

            sb.Append(itemBinary);
        }
        return sb.ToString();
    }
}