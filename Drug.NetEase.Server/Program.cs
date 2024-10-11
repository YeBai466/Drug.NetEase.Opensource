using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Drug.NetEase.Server.BaseAuth;
using Drug.NetEase.Server.Extensions;
using Drug.NetEase.Server.Utilities;
using Newtonsoft.Json;

namespace Drug.NetEase.Server
{
    internal class Program
    {
        private static dynamic _config;
        private static string _version;
        private static bool _isStopped;
        private static string _stoppedMessage;
        private static string _expiredMessage;
        private static bool _isFree;
        
        public static void Main(string[] args)
        {
            "尝试连接Drug数据库".Log();
            if (DrugDataBase.Connect())
                "连接Drug数据库成功".Log();
            else
                "连接Drug数据库失败".Log();
            WmCodeHelper.Init();
            "WmCode初始化完成".Log();
            MemoryCracker.CrackCommonDll();
            "CommonDll初始化完成".Log();
            var configFile = @$"{Directory.GetCurrentDirectory()}\config.json";
            if (!File.Exists(configFile))
                File.Create(configFile).Close();
            ReadConfig:
            {
                try
                {
                    var configJson = File.ReadAllText(configFile);
                    _config = JsonConvert.DeserializeObject(configJson);
                    _version = _config.version;
                    if (_version == null)
                        throw new ArgumentException();
                    string isStopped =  _config.isstopped;
                    if (isStopped == null)
                        throw new ArgumentException();
                    _isStopped = isStopped == "true";
                    string isFree =  _config.isfree;
                    if (isFree == null)
                        throw new ArgumentException();
                    _isFree = isFree == "true";
                    _stoppedMessage = _config.stoppedmessage;
                    if (_stoppedMessage == null)
                        throw new ArgumentException();
                    _expiredMessage = _config.expiredmessage;
                    if (_expiredMessage == null)
                        throw new ArgumentException();
                    _expiredMessage = _expiredMessage.Replace("%version%", _version);
                    
                    "开始读取配置文件".Log();
                    $"Version: {_version}".Log();
                    $"IsStopped: {isStopped}".Log();
                    $"StoppedMessage: {_stoppedMessage}".Log();
                    $"ExpiredMessage: {_expiredMessage}".Log();
                    $"IsFree: {isFree}".Log();
                    "已读取配置文件".Log();
                }
                catch
                {
                    var configJson = JsonConvert.SerializeObject(new
                    {
                        version = "0.0.0",
                        isstopped = "false",
                        stoppedmessage = "伺服器維護中",
                        expiredmessage = "當前有新的免費版本 %version%，請加入686829296獲取",
                        isfree = "false"
                    });
                    File.WriteAllText(configFile,configJson);
                    "配置文件不存在或不合法，已写入默认配置文件".Log();
                    goto ReadConfig;
                }   
            }

            var listeningPort = 8080;
            
            var httpListener = new HttpListener();
            httpListener.Prefixes.Add(@$"http://+:{listeningPort}/");
            $"HttpListener prefix add {listeningPort}".Log();
            try
            {
                httpListener.Start();
            }
            catch (Exception e)
            {
                $"Please start by using Administrator => {e}".Log();
                Console.ReadLine();
                return;
            }
            $"HttpListener started on {listeningPort}".Log();
            while (httpListener.IsListening)
            {
                var context = httpListener.BeginGetContext(ListenerCallback, httpListener);
                context.AsyncWaitHandle.WaitOne();
            }
        }

        private static void ListenerCallback(IAsyncResult ar)
        {
            try
            {
                var listener = ar.AsyncState as HttpListener;
                var context = listener.EndGetContext(ar);
                var httpMethod = context.Request.HttpMethod;
                var requestUrl = context.Request.Url.AbsolutePath.Replace('\\', '/');
                var body = context.Request.InputStream;
                var encoding = context.Request.ContentEncoding;
                var reader = new StreamReader(body, encoding);
                var requestData = reader.ReadToEnd();
                var requestAddress = context.Request.Headers["Drug_Address"];
                $"{requestAddress} {requestUrl}".Log();
                var decryptRequestData = Encoding.UTF8.GetString(DrugCrypt.Decrypt(requestData.ToBytes()));
                var responseString = DrugCrypt.Encrypt(Encoding.UTF8.GetBytes(HandleRequest(httpMethod, requestUrl, decryptRequestData, requestAddress))).ToHex();
                
                try
                {
                    var buffer = Encoding.UTF8.GetBytes(responseString);
                    var response = context.Response;
                    response.ContentType = "text/html";
                    response.ContentLength64 = buffer.Length;
                    var output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                    context.Response.Close();
                }
                catch (Exception e)
                {
                    $"An error occurred while sending response => {e}".Log();
                }
            }
            catch (Exception e)
            {
                $"An error occurred while getting http content => {e}".Log(); 
            }
        }

        private static string HandleRequest(string httpMethod, string requestUrl, string requestData, string requestAddress)
        {
            dynamic requestParameter;
            try
            {
                requestParameter = JsonConvert.DeserializeObject(requestData);
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new
                {
                    code = -1,
                    message = "An error occurred while getting request parameter.",
                    details = e.ToString()
                });
            }

            DrugDataBaseEntities.UserEntity requestUser = null;
            string userToken = requestParameter.usertoken;
            if (userToken != null)
            {
                requestUser = DrugDataBase.GetUserByUserToken(userToken);
            }

            if (requestUser != null)
            {
                if (Convert.ToInt64(requestUser.expiredtime) < new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds())
                {
                    if (!_isFree)
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            code = 0,
                            message = "Request successfully.",
                            details = "用户已到期"
                        });
                    }
                }
            }
            try
            {
                switch (requestUrl)
                {
                    case "/Drug/EncryptAuthenticationBody":
                    {
                        if (requestUser == null)
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "用户不存在"
                            });
                        }
                        string authenticationBody = requestParameter.body;
                        return JsonConvert.SerializeObject(new
                        {
                            code = 0,
                            message = "Request successfully.",
                            details = "",
                            body = x19Crypt.HttpEncrypt(authenticationBody.ToBytes()).ToHex()
                        });
                    }
                    case "/Drug/DecryptAuthenticationResponse":
                    {
                        if (requestUser == null)
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "用户不存在"
                            });
                        }
                        string authenticationResponse = requestParameter.response;
                        return JsonConvert.SerializeObject(new
                        {
                            code = 0,
                            message = "Request successfully.",
                            details = "",
                            response = x19Crypt.ParseLoginResponse(authenticationResponse.ToBytes()).ToHex()
                        });
                    }
                    case "/Drug/ComputeDynamicToken":
                    {
                        if (requestUser == null)
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "用户不存在"
                            });
                        }
                        string path = requestParameter.path;
                        string body = requestParameter.body;
                        string token = requestParameter.token;
                        return JsonConvert.SerializeObject(new
                        {
                            code = 0,
                            message = "Request successfully.",
                            details = "",
                            token = x19Crypt.ComputeDynamicToken(path.HexToString(),body.ToBytes(),token.HexToString()).ToHex()
                        });
                    }
                    case "/Drug/GenerateCppConfig":
                    {
                        if (requestUser == null)
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "用户不存在"
                            });
                        }
                        string baseName = requestParameter.basename;
                        string authenticationResponse = requestParameter.response;
                        MemoryCracker.ParseLoginResponse(authenticationResponse);
                        var h5Token = MemoryCracker.GetH5Token();
                        string userId = requestParameter.userid;
                        string configPath = requestParameter.configpath;
                        string userName = requestParameter.username;
                        string skinFile = requestParameter.skinfile;
                        string skinMd5 = requestParameter.skinmd5;
                        string skinSlim = requestParameter.skinslim;
                        string serverId = requestParameter.serverid;
                        string serverAddress = requestParameter.serveraddress;
                        string serverPort = requestParameter.serverport;
                        var modifyList = new List<MemoryCracker.MemoryModify>();
                        modifyList.Add(new MemoryCracker.MemoryModify("CurrentInputMode","00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00"));
                        modifyList.Add(new MemoryCracker.MemoryModify("DefaultInputMode","44 65 76 69 63 65 4F 53 00 00 00 00 00 00 00 00"));
                        modifyList.Add(new MemoryCracker.MemoryModify("DeviceModel","00 00 00 00 00 00 00 00 00 00 00"));
                        modifyList.Add(new MemoryCracker.MemoryModify("DeviceOS","00 00 00 00 00 00 00 00"));
                        return JsonConvert.SerializeObject(new
                        {
                            code = 0,
                            message = "Request successfully.",
                            details = "",
                            cppconfig = new CppConfigHelper(baseName.HexToString(),h5Token,
                                Convert.ToInt64(userId.HexToString()),configPath.HexToString(),
                                userName.HexToString(),skinFile.HexToString(),skinMd5.HexToString(),
                                skinSlim.HexToString() == "true",serverId.HexToString(),
                                serverAddress.HexToString(),Convert.ToInt32(serverPort.HexToString())).GetConfig().ToHex(),
                            mem = modifyList
                        });
                    }
                    case "/Drug/ParseCodeFromImage":
                    {
                        if (requestUser == null)
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "用户不存在"
                            });
                        }
                        string image = requestParameter.image;
                        var buffer = image.ToBytes();
                        var sb = new StringBuilder('\0', 256);
                        var flag = WmCodeHelper.GetImageFromBuffer(buffer, buffer.Length,sb);
                        var code = flag ? sb.ToString() : "";
                        return JsonConvert.SerializeObject(new
                        {
                            code = 0,
                            message = "Request successfully.",
                            details = "",
                            flag = flag ? "true" : "false",
                            imagecode = code
                        });
                    }
                    case "/Drug/IsFree":
                    {
                        if (_isStopped)
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = _stoppedMessage
                            });
                        }
                        string version = requestParameter.version;
                        if (version != _version)
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = _expiredMessage
                            });
                        }
                        return JsonConvert.SerializeObject(new
                        {
                            code = 0,
                            message = "Request successfully.",
                            details = "Of course."
                        });
                    }
                    case "/Drug/LoginUser":
                    {
                        if (_isStopped)
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = _stoppedMessage
                            });
                        }
                        string username = requestParameter.username;
                        if (username.ContainsNonAlphanumeric())
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "用戶名有除大小寫英文，數字，下劃綫以外的字符"
                            });
                        }
                        string password = requestParameter.password;
                        if (password.ContainsNonAlphanumeric())
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "密碼有除大小寫英文，數字，下劃綫以外的字符"
                            });
                        }
                        string machineCode = requestParameter.machinecode;

                        if (username == "")
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "用戶名為空"
                            });
                        }

                        if (password == "")
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "密碼為空"
                            });
                        }

                        var user = DrugDataBase.GetUserByUsername(username);
                        
                        if (user == null)
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "用戶不存在"
                            });
                        }

                        if (user.machinecode != machineCode)
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "機械碼校驗失敗"
                            });
                        }
                        
                        if (user.password != password)
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "密碼錯誤"
                            });
                        }
                        return JsonConvert.SerializeObject(new
                        {
                            code = 0,
                            message = "Request successfully.",
                            details = "登錄成功", 
                            expiredtime = user.expiredtime,
                            isexpired = _isFree ? "false" : Convert.ToInt64(user.expiredtime) < new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() ? "true" : "false",
                            token = DrugDataBase.UpdateUserToken(user.userid)
                        });
                    }
                    case "/Drug/RegisterUser":
                    {
                        if (_isStopped)
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = _stoppedMessage
                            });
                        }
                        string username = requestParameter.username;
                        if (username.ContainsNonAlphanumeric())
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "用戶名有除大小寫英文，數字，下劃綫以外的字符"
                            });
                        }
                        string password = requestParameter.password;
                        if (password.ContainsNonAlphanumeric())
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "密碼有除大小寫英文，數字，下劃綫以外的字符"
                            });
                        }
                        string machineCode = requestParameter.machinecode;

                        if (username == "")
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "用戶名為空"
                            });
                        }
                        
                        if (username.Length < 5)
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "用戶名不能少於五個字符"
                            });
                        }
                        
                        if (username.Length > 10)
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "用戶名不能多於十個字符"
                            });
                        }

                        if (password == "")
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "密碼為空"
                            });
                        }
                        
                        if (password.Length < 5)
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "密碼不能少於五個字符"
                            });
                        }
                        
                        if (password.Length > 10)
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "密碼不能多於十個字符"
                            });
                        }
                        
                        var user = DrugDataBase.GetUserByUsername(username);
                        if (user != null)
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "用戶名已存在"
                            });
                        }

                        user = DrugDataBase.GetUserByRegisterAddress(requestAddress);
                        if (user != null)
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "一个IP只能注册一个账号"
                            });
                        }

                        if (DrugDataBase.GetUserByMachineCode(machineCode).Count > 10)
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                code = 0,
                                message = "Request successfully.",
                                details = "該電腦注冊過的賬號過多"
                            });
                        }

                        DrugDataBase.RegisterUser(username, password, requestAddress,machineCode);
                        return JsonConvert.SerializeObject(new
                        {
                            code = 0,
                            message = "Request successfully.",
                            details = "注冊成功"
                        });
                    }
                    default:
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            code = -1,
                            message = "Unknown request url.",
                            details = ""
                        });
                    }
                }
            }
            catch (Exception e)
            {
                $"An error occurred while getting response => {e}".Log();
                return JsonConvert.SerializeObject(new
                {
                    code = -2,
                    message = "An error occurred while getting response.",
                    details = e.ToString()
                });
            }
        }
    }
}