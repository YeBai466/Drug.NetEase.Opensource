using System;
using System.IO;
using System.Net;
using System.Text;

namespace Drug.NetEase.Client.BaseAuth;

public static class x19AuthRequest
{
    public static byte[] RequestBytes(string url, byte[] data)
    {
        var request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "POST";
        request.ContentType = "application/json";
        request.UserAgent = "WPFLauncher/0.0.0.0";

        if (data != null && data.Length > 0)
        {
            request.ContentLength = data.Length;
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }
        }

        using (var response = (HttpWebResponse)request.GetResponse())
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    responseStream.CopyTo(memoryStream);
                }

                return memoryStream.ToArray();
            }
        }
    }
    public static string EncryptRequest(x19AuthEntities.AuthOtpRetEntity_Entity Auth, string url,  string data, string mode = "POST")
    {
        using (WebClient webClient = new WebClient())
        {
            string apiUrl = x19Auth.ReleaseJson.WebServerUrl + url;

            // 设置请求头
            webClient.Headers.Add("User-Agent", "WPFLauncher/0.0.0.0");
            webClient.Headers.Add("user-id", Auth.entity_id); // 请确保 DUserID 已定义
            webClient.Headers.Add("user-token", ServerAuth.ComputeDynamicToken(url, Encoding.UTF8.GetBytes(data),Auth.token));

            // 发送请求
            if (mode == "POST")
            {
                byte[] responseBytes = webClient.UploadData(apiUrl, "POST", Encoding.UTF8.GetBytes(data));
                return Encoding.UTF8.GetString(responseBytes);
            }
            else
            {
                return webClient.DownloadString(apiUrl);
            }
        }
    }
    public static string RequestString(string url, byte[] data)
    {
        var request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "POST";
        request.ContentType = "application/json";
        request.UserAgent = "WPFLauncher/0.0.0.0";

        if (data != null && data.Length > 0)
        {
            request.ContentLength = data.Length;
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }
        }

        using (var response = (HttpWebResponse)request.GetResponse())
        {
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                return streamReader.ReadToEnd();
            }
        }
    }

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
    public static string RequestString(string url, string postData)
    {
        using (WebClient client = new WebClient())
        {
            client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            byte[] responseBytes = client.UploadData(url, "POST", Encoding.UTF8.GetBytes(postData));
            return Encoding.UTF8.GetString(responseBytes);
        }
    }
}