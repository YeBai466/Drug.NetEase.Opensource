using System.Runtime.InteropServices;
using System.Text;

namespace Drug.NetEase.Server.Utilities;

public class WmCodeHelper
{
    public static void Init()
    {
        var optionIndex = 1; // 传入是否使用unicode格式的选项索引
        var optionValue = 1; // 设置为使用unicode格式
        UseUnicodeString(optionIndex, optionValue);
            
        // 调用DLL中的函数
        LoadWmFromFile("4399.dat", "1");
            
        //Option配置
        SetWmOption(1, 0);
        SetWmOption(2, 0);
        SetWmOption(3, 0);
        SetWmOption(4, 1);
        SetWmOption(5, 0);
        SetWmOption(6, 55);//模糊度
        SetWmOption(7, 0);
    }
    [DllImport("WmCode.dll", CharSet = CharSet.Auto)]
    private static extern bool LoadWmFromFile(string Path, string Password);

    [DllImport("WmCode.dll", CharSet = CharSet.Auto)]
    private static extern bool UseUnicodeString(int optionIndex, int optionValue);

    [DllImport("WmCode.dll", CharSet = CharSet.Auto)]
    private static extern bool SetWmOption(int OptionIndex, int OptionValue);
    
    [DllImport("WmCode.dll")]
    public static extern bool GetImageFromBuffer(byte[] FileBuffer, int ImgBufLen, StringBuilder Vcode);
}