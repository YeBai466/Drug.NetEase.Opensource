using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Drug.NetEase.Client.Exceptions;
using Drug.NetEase.Client.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Drug.NetEase.Client.BaseAuth;

public static class x19Auth
{
    public const string WpfVersion = "1.10.7.22905";
    private static x19AuthEntities.ReleaseJsonEntity _releaseJson;

    public static x19AuthEntities.ReleaseJsonEntity ReleaseJson
    {
        get
        {
            if (_releaseJson == null)
                _releaseJson = JsonConvert.DeserializeObject<x19AuthEntities.ReleaseJsonEntity>(
                    x19AuthRequest.RequestString("https://x19.update.netease.com/serverlist/release.json"));
            return _releaseJson;
        }
    }

    public static x19AuthEntities.AuthOtpRetEntity_Entity AuthenticationOtp(string osName, string osVer, string macAddr,
        string saUdid, string appVer, string disk, string sdkUid, string sessionId, string sdkVersion, string udid,
        string deviceId, string aid, string otpToken)
    {
        var packet =
            "{\"otp_token\":\"$OTPToken$\",\"otp_pwd\":\"\",\"aid\":$A1D$,\"sauth_json\":\"{\\u0022gameid\\u0022:\\u0022x19\\u0022,\\u0022login_channel\\u0022:\\u0022netease\\u0022,\\u0022app_channel\\u0022:\\u0022netease\\u0022,\\u0022platform\\u0022:\\u0022pc\\u0022,\\u0022sdkuid\\u0022:\\u0022$SDKUID$\\u0022,\\u0022sessionid\\u0022:\\u0022$sessionid$\\u0022,\\u0022sdk_version\\u0022:\\u00223.4.0\\u0022,\\u0022udid\\u0022:\\u0022$UDID$\\u0022,\\u0022deviceid\\u0022:\\u0022$deviceid$\\u0022,\\u0022aim_info\\u0022:\\u0022{\\\\u0022aim\\\\u0022:\\\\u0022$IP$\\\\u0022,\\\\u0022country\\\\u0022:\\\\u0022CN\\\\u0022,\\\\u0022tz\\\\u0022:\\\\u0022\\\\u002B0800\\\\u0022,\\\\u0022tzid\\\\u0022:\\\\u0022\\\\u0022}\\u0022,\\u0022client_login_sn\\u0022:\\u0022846C15C9F72E4C399247CFB35532C07A\\u0022,\\u0022gas_token\\u0022:\\u0022\\u0022,\\u0022source_platform\\u0022:\\u0022pc\\u0022,\\u0022ip\\u0022:\\u0022$IP$\\u0022}\",\"sa_data\":\"{\\u0022os_name\\u0022:\\u0022$OS_NAME$\\u0022,\\u0022os_ver\\u0022:\\u0022$OS_VER$\\u0022,\\u0022mac_addr\\u0022:\\u0022$MACA$\\u0022,\\u0022udid\\u0022:\\u0022$udid_sa$\\u0022,\\u0022app_ver\\u0022:\\u0022$APPVER$\\u0022,\\u0022sdk_ver\\u0022:\\u0022\\u0022,\\u0022network\\u0022:\\u0022\\u0022,\\u0022disk\\u0022:\\u0022$Disk$\\u0022,\\u0022is64bit\\u0022:\\u00221\\u0022,\\u0022video_card1\\u0022:\\u0022NVIDIA GeForce GTX 1060\\u0022,\\u0022video_card2\\u0022:\\u0022\\u0022,\\u0022video_card3\\u0022:\\u0022\\u0022,\\u0022video_card4\\u0022:\\u0022\\u0022,\\u0022launcher_type\\u0022:\\u0022PC_java\\u0022,\\u0022pay_channel\\u0022:\\u0022netease\\u0022}\",\"version\":{\"version\":\"$APPVER$\",\"launcher_md5\":\"\",\"updater_md5\":\"\"}}";
        packet = packet.Replace("$OS_NAME$", osName);
        packet = packet.Replace("$OS_VER$", osVer);
        packet = packet.Replace("$MACA$", macAddr);
        packet = packet.Replace("$udid_sa$", saUdid);
        packet = packet.Replace("$APPVER$", appVer);
        packet = packet.Replace("$Disk$", disk);
        packet = packet.Replace("$SDKUID$", sdkUid);
        packet = packet.Replace("$SdkUid$", sdkUid);
        packet = packet.Replace("$sessionid$", sessionId);
        packet = packet.Replace("$SDK_Verion$", sdkVersion);
        packet = packet.Replace("$UDID$", udid);
        packet = packet.Replace("$deviceid$", deviceId);
        packet = packet.Replace("$A1D$", aid);
        packet = packet.Replace("$OTPToken$", otpToken);
        var data = ServerAuth.HttpEncrypt(packet);
        var res = x19AuthRequest.RequestBytes(ReleaseJson.CoreServerUrl + "/authentication-otp", data);
        var hexString = BitConverter.ToString(res).Replace("-", "");
        var authOtpRetEntity =
            JsonConvert.DeserializeObject<x19AuthEntities.AuthOtpRetEntity>(
                ServerAuth.ParseLoginResponse(hexString));
        if (authOtpRetEntity.code != 0) throw new x19RequestException(authOtpRetEntity.message);
        authOtpRetEntity.entity.response = hexString;
        return authOtpRetEntity.entity;
    }

    public static x19AuthEntities.LoginOtpRetEntity_entity LoginOtp(string sAuthJson)
    {
        var loginOtpRetEntity = JsonConvert.DeserializeObject<x19AuthEntities.LoginOtpRetEntity>(
            x19AuthRequest.RequestString(ReleaseJson.CoreServerUrl + "/login-otp",
                Encoding.Default.GetBytes(sAuthJson)));
        if (loginOtpRetEntity.code != 0)
        {
            throw new x19RequestException(loginOtpRetEntity.message);
        }

        return loginOtpRetEntity.entity;
    }
    public static (x19AuthEntities.AuthOtpRetEntity_Entity,Exception) CookiesLogin(string cookies)
    {
        try
        {
            var loginOtpEntity =
                JsonConvert.DeserializeObject<x19AuthEntities.LoginOtpEntity>(cookies);
            var sAuthJson =
                JsonConvert.DeserializeObject<x19AuthEntities.LoginOtpEntity_sauth_json>(
                    loginOtpEntity
                        .sauth_json);
            var loginOtpRetEntity = LoginOtp(cookies);
            var authOtpRetEntityEntity = AuthenticationOtp("windows", "Microsoft Windows 10 专业版",
                StringExtensions.RandomLetter(12), "BFEBFBFF000406E34EA2FD9D", WpfVersion,
                StringExtensions.RandomLetter(8),
                sAuthJson.sdkuid, sAuthJson.sessionid, "1.0.0", sAuthJson.udid,
                sAuthJson.deviceid,
                loginOtpRetEntity.aid.ToString(), loginOtpRetEntity.otp_token);
            return (authOtpRetEntityEntity,null);
        }
        catch (Exception e)
        {
            return (null, e);
        }
    }
    public static string CheckNickname(x19AuthEntities.AuthOtpRetEntity_Entity auth, string nickname)
    {
        var str = JsonConvert.SerializeObject(new
        {
            nickname = nickname
        });
        var ret = JsonConvert.DeserializeObject<x19AuthEntities.CheckNicknameEntity>(x19AuthRequest.EncryptRequest(auth, "/personal-info/check-nickname", str));
        return ret.code == 0 ? "" : ret.message;
    }

    public static string CheckHasNickname(x19AuthEntities.AuthOtpRetEntity_Entity auth)
    {
        var str = JsonConvert.SerializeObject(new
        {
            entity_id = auth.entity_id,
            name = "drug"
        });
        var ret = JsonConvert.DeserializeObject<x19AuthEntities.SetNicknameEntity>(x19AuthRequest.EncryptRequest(auth, "/nickname-setting", str));
        return ret.code == 20 ? ret.entity.name : "";
    }

    public static string SetNickName(x19AuthEntities.AuthOtpRetEntity_Entity auth, string name)
    {
        
        var str = JsonConvert.SerializeObject(new
        {
            entity_id = auth.entity_id,
            name = name
        });
        var ret = JsonConvert.DeserializeObject<x19AuthEntities.SetNicknameEntity>(x19AuthRequest.EncryptRequest(auth, "/nickname-setting", str));
        if (ret.code == 8)
            return "";
        return ret.entity.name;
    }

    public static (string, int) Pt4399Login(string username, string password)
    {
        var captchasessionid = $"captchaReq{StringExtensions.GenerateRandomString(19)}";
        var sessionId = $"{Guid.NewGuid():D}"; // 生成随机会话ID
        var loginUrl = "http://ptlogin.4399.com/ptlogin/login.do?v=1";
        var loginData =
            $"postLoginHandler=default&externalLogin=qq&bizId=2100001792&appId=kid_wdsj&gameId=wd&sec=1&password={password}&username={username}";
        var cookieName = "Uauth";
        var postDataBytes = Encoding.UTF8.GetBytes(loginData);

        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler();
        handler.CookieContainer = cookieContainer; // 使用 CookieContainer 来管理请求和响应的Cookie

        var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("Cookie", new Cookie("ptusertype", "kid_wdsj.4399_login").ToString());
        client.DefaultRequestHeaders.Add("Cookie", new Cookie("phlogact", "l123456").ToString());
        client.DefaultRequestHeaders.Add("Cookie", new Cookie("USESSIONID", sessionId).ToString());
        client.DefaultRequestHeaders.Accept.Clear(); // 清空Accept，让服务器返回默认格式的数据

        ServicePointManager.SecurityProtocol =
            SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        ServicePointManager.ServerCertificateValidationCallback =
            (sender, certificate, chain, sslPolicyErrors) => true;

        var content = new ByteArrayContent(postDataBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
        content.Headers.ContentLength = postDataBytes.Length;

        var loginResponse = client.PostAsync(loginUrl, content).Result;
        var loginResponseContent = loginResponse.Content.ReadAsStringAsync().Result;

        if (loginResponseContent.Contains("<div id=\"Msg\" class=\"login_hor login_err_tip\">") &&
            loginResponseContent.Contains("<div id=\"Msg\" class=\"login_hor login_err_tip\"></div>"))
            //风控了 需要输入验证码
            for (var i = 0; i < 5; i++)
            {
                var Code_img = client
                    .GetByteArrayAsync(
                        $"http://ptlogin.4399.com/ptlogin/captcha.do?captchaId={captchasessionid}")
                    .Result;
                //以下使用GetImageFromBuffer接口
                if (ServerAuth.ParseCodeFromImage(Code_img, out var Result))
                {
                    if (Result.Length != 4) continue;

                    var yanzhengma = Result;
                    loginData =
                        $"postLoginHandler=default&externalLogin=qq&bizId=2100001792&appId=kid_wdsj&gameId=wd&sec=1&password={password}&username={username}&sessionId={captchasessionid}&inputCaptcha={yanzhengma}";
                    postDataBytes = Encoding.UTF8.GetBytes(loginData);
                    content = new ByteArrayContent(postDataBytes);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                    content.Headers.ContentLength = postDataBytes.Length;

                    loginResponse = client.PostAsync(loginUrl, content).Result;
                    loginResponseContent = loginResponse.Content.ReadAsStringAsync().Result;
                    if (loginResponseContent.Contains("<div id=\"Msg\" class=\"login_hor login_err_tip\"></div>") !=
                        true) break;
                }
            }

        if (loginResponseContent.Contains("<div id=\"Msg\" class=\"login_hor login_err_tip\"></div>"))
            //登录过于频繁，经过程序自动填写验证码后仍旧失败，请重试或重启路由器
            return ("验证码识别失败且登录过于频繁", 1);

        if (loginResponseContent.Contains("<div id=\"Msg\" class=\"login_hor login_err_tip\">"))
        {
            var errorMsg = loginResponseContent.Substring(
                loginResponseContent.IndexOf(
                    "<div id=\"Msg\" class=\"login_hor login_err_tip\">\r\n\t\t\t\t\t\t\t\t\t") +
                "<div id=\"Msg\" class=\"login_hor login_err_tip\">\r\n\t\t\t\t\t\t\t\t\t".Length,
                loginResponseContent.IndexOf("\r\n\t\t\t\t\t\t\t\t</div>") -
                loginResponseContent.IndexOf(
                    "<div id=\"Msg\" class=\"login_hor login_err_tip\">\r\n\t\t\t\t\t\t\t\t\t") -
                "<div id=\"Msg\" class=\"login_hor login_err_tip\">\r\n\t\t\t\t\t\t\t\t\t".Length).Trim();
            return (errorMsg, 2);
        }


        if (loginResponse.StatusCode == HttpStatusCode.OK)
        {
            IEnumerable<string> setCookieHeaders;
            try
            {
                setCookieHeaders = loginResponse.Headers.GetValues("Set-Cookie");
            }
            catch (Exception ex)
            {
                // Console.WriteLine(ex.ToString());
                return ("请检查账号密码是否输入正确", 3);
            }

            foreach (var setCookieHeader in setCookieHeaders)
            {
                var cookie = new Cookie(cookieName, "", "/", "ptlogin.4399.com");
                var index123 = setCookieHeader.IndexOf(cookieName + "=");

                if (index123 >= 0)
                {
                    var value = setCookieHeader.Substring(index123 + cookieName.Length + 1);
                    index123 = value.IndexOf(";");

                    if (index123 >= 0) value = value.Substring(0, index123);

                    cookie.Value = WebUtility.UrlDecode(value);

                    var values = cookie.Value.Split('|');

                    // 构造第二个POST请求需要提交的数据
                    var randTime = values[4];
                    var checkRequest = new HttpRequestMessage(HttpMethod.Post,
                        $"http://ptlogin.4399.com/ptlogin/checkKidLoginUserCookie.do?appId=kid_wdsj&gameUrl=http://cdn.h5wan.4399sj.com/microterminal-h5-frame?game_id=500352&rand_time={randTime}&nick=null&onLineStart=false&show=1&isCrossDomain=1&retUrl=http%253A%252F%252Fptlogin.4399.com%252Fresource%252Fucenter.html");
                    checkRequest.Headers.Add("Cookie", new Cookie("phlogact", "l123456").ToString());
                    checkRequest.Headers.Add("Cookie", new Cookie("USESSIONID", sessionId).ToString());
                    checkRequest.Headers.Add("Cookie", new Cookie("Uauth", cookie.Value).ToString());
                    checkRequest.Headers.Add("Cookie", new Cookie("Puser", username).ToString());

                    var checkResponse = client.SendAsync(checkRequest).Result;
                    checkResponse.EnsureSuccessStatusCode();

                    var infoRequest = new HttpRequestMessage(HttpMethod.Get,
                        $"https://microgame.5054399.net/v2/service/sdk/info?callback=&queryStr={HttpUtility.UrlEncode(checkResponse.RequestMessage.RequestUri.Query.Trim('?'))}");
                    var infoResponse = client.SendAsync(infoRequest).Result;
                    infoResponse.EnsureSuccessStatusCode();
                    var infoContents = infoResponse.Content.ReadAsStringAsync().Result;
                    var jsonObj = JObject.Parse(infoContents);
                    var code = (int)jsonObj["code"];
                    var msg = (string)jsonObj["msg"];

                    if (code == 10000)
                    {
                        var sdkLoginData = (string)jsonObj["data"]["sdk_login_data"];
                        var sdkUsername = (string)jsonObj["data"]["username"];
                        var udid = StringExtensions.GenerateRandomString(32);
                        var parameters = new Dictionary<string, string>();
                        foreach (var parameter in sdkLoginData.Split('&'))
                        {
                            var parts = parameter.Split('=');
                            var name = parts[0];
                            var valuess = parts.Length > 1 ? parts[1] : null;
                            parameters[name] = valuess;
                        }

                        var sdkReturnName = parameters["username"];
                        var time = parameters["time"];
                        var uid = parameters["uid"];
                        var token = parameters["token"];
                        var sauth =
                            "{\"sauth_json\" : \"{\\\"gameid\\\":\\\"x19\\\",\\\"login_channel\\\":\\\"4399pc\\\",\\\"app_channel\\\":\\\"4399pc\\\",\\\"platform\\\":\\\"pc\\\",\\\"sdkuid\\\":\\\"%uid%\\\",\\\"sessionid\\\":\\\"%token%\\\",\\\"sdk_version\\\":\\\"1.0.0\\\",\\\"udid\\\":\\\"%udid%\\\",\\\"deviceid\\\":\\\"%deviceid%\\\",\\\"aim_info\\\":\\\"{\\\\\\\"aim\\\\\\\":\\\\\\\"127.0.0.1\\\\\\\",\\\\\\\"country\\\\\\\":\\\\\\\"CN\\\\\\\",\\\\\\\"tz\\\\\\\":\\\\\\\"+0800\\\\\\\",\\\\\\\"tzid\\\\\\\":\\\\\\\"\\\\\\\"}\\\",\\\"client_login_sn\\\":\\\"%client_login%\\\",\\\"gas_token\\\":\\\"\\\",\\\"source_platform\\\":\\\"pc\\\",\\\"ip\\\":\\\"127.0.0.1\\\",\\\"userid\\\":\\\"%username%\\\",\\\"realname\\\":\\\"{\\\\\\\"realname_type\\\\\\\":\\\\\\\"0\\\\\\\"}\\\",\\\"timestamp\\\":\\\"%time%\\\"}\"}";
                        sauth = sauth.Replace("%uid%", uid);
                        sauth = sauth.Replace("%token%", token);
                        sauth = sauth.Replace("%time%", time);
                        sauth = sauth.Replace("%username%", username.ToLower());
                        sauth = sauth.Replace("%udid%", udid);
                        sauth = sauth.Replace("%deviceid%", udid);
                        sauth = sauth.Replace("%client_login%", Guid.NewGuid().ToString("N").ToUpperInvariant());

                        return (sauth, 0);
                    }

                    return (msg, 4);
                }
            }
        }

        return ("未知错误", 5);
    }
}