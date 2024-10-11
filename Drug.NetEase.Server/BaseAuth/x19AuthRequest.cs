using System.IO;
using System.Net;

namespace Drug.NetEase.Server.BaseAuth;

public class x19AuthRequest
{
    public static string RequestString(string url)
    {
        var request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";
        request.ContentType = "application/json";
        request.UserAgent = "WPFLauncher/0.0.0.0";

        using (var response = (HttpWebResponse)request.GetResponse())
        {
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}