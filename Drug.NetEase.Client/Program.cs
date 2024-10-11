using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Drug.NetEase.Client.BaseAuth;
using Drug.NetEase.Client.Extensions;
using Drug.NetEase.Client.Resources;
using Drug.NetEase.Client.Utilities;
using Newtonsoft.Json.Utilities.LinqBridge;

namespace Drug.NetEase.Client
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (ServerAuth.ServerUrl == "127.0.0.1")
            {
                Console.WriteLine("选择綫路中... 请耐心等待");
                ServerAuth.Ping();
                Console.Clear();   
            }
            
            //解除控制台输入限制
            var inputBuffer = new byte[4096];
            var inputStream = Console.OpenStandardInput(inputBuffer.Length);
            Console.SetIn(new StreamReader(inputStream, Console.InputEncoding, false, inputBuffer.Length));
            
            //神必操作
            try
            {
                Console.WriteLine(x19AuthRequest.RequestString("http://61.136.162.109:11451/gg/GG.txt")+"\n");
            }
            catch (Exception e)
            {
                "获取公告有错误发生了".Log();
                e.ToString().Log();
            }
            "开发者注: 因不满某团队的做法，以及某工具对PC进PE的独裁统治，Drug团队的高尚人士会对这些人进行制裁".Log();
            $"Drug V{ServerAuth.Version} 免费版".Log();
            if (ServerAuth.Token == null)
            {
                AuthLogin:
                {
                    try
                    {
                        "请选择你要进行的操作".Log();
                        "1. 登录".Log();
                        "2. 注册".Log();
                        Console.Write("> ");
                        switch (Console.ReadLine().Trim().Replace(" ",""))
                        {
                            case "1":
                            {
                                "请输入账号".Log();
                                Console.Write("> ");
                                var username = Console.ReadLine();
                                "请输入密码".Log();
                                Console.Write("> ");
                                
                                var password = Console.ReadLine();
                                var ret = ServerAuth.LoginUser(username, password);
                                if (ret.details != "登录成功")
                                {
                                    $"登录失败: {ret.details}".Log();
                                    goto AuthLogin;
                                }

                                if (ret.isexpired == "true")
                                {
                                    "登录失败: 用户已到期".Log();
                                    goto AuthLogin;
                                }
                                $"欢迎 用户 {username}".Log();
                                if (!(Convert.ToInt64(ret.expiredtime) < new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()))
                                {
                                    var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Convert.ToInt64(ret.expiredtime));
                                    $"到期时间: {dateTime.ToString("yyyy年MM月dd日 HH时mm分ss秒")}".Log();
                                }
                                break;
                            }
                            case "2":
                            {
                                "请输入账号".Log();
                                Console.Write("> ");
                                var username = Console.ReadLine();
                                "请输入密码".Log();
                                Console.Write("> ");
                                var password = Console.ReadLine();
                                var ret2 = ServerAuth.RegisterUser(username, password);
                                if (ret2 != "注册成功")
                                {
                                    $"注册失败: {ret2}".Log();
                                    goto AuthLogin;
                                }

                                var ret = ServerAuth.LoginUser(username, password);
                                if (ret.details != "登录成功")
                                {
                                    $"登录失败: {ret.details}".Log();
                                    goto AuthLogin;
                                }

                                if (ret.isexpired == "true")
                                {
                                    "登录失败: 用户已到期".Log();
                                    goto AuthLogin;
                                }
                                $"欢迎 用户 {username}".Log();
                                if (!(Convert.ToInt64(ret.expiredtime) < new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()))
                                {
                                    DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Convert.ToInt64(ret.expiredtime));
                                    $"到期时间: {dt.ToString("yyyy年MM月dd日 HH时mm分ss秒")}".Log();
                                }
                                break;
                            }
                            default:
                            {
                                goto AuthLogin;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        "登录有错误发生了".Log();
                        e.ToString().Log();
                        goto AuthLogin;
                    }
                }
            }
            //判断是否免费（OLD）
            try
            {
                var isFree = ServerAuth.IsFree(ServerAuth.Version);
                if (isFree != "Of course.")
                {
                    Console.WriteLine($"{isFree}，回车键退出");
                    Console.ReadLine();
                    return;
                }
            }
            catch (Exception e)
            {
                "链接伺服器有错误发生了".Log();
                e.ToString().Log();
                "回车键退出".Log();
                Console.ReadLine();
                return;
            }
            
            //判断是否下存在BE版本我的世界
            try
            {
                if (CppConfigHelper.CppGamePath == "")
                {
                    "当前4399或网易启动器没有安装过BE版我的世界，请使用4399或网易盒子进入梦幻世界后再启动本程序，回车键退出".Log();
                    Console.ReadLine();
                    return;
                }
            }
            catch (Exception e)
            {
                "获取BE版本我的世界路径有错误发生了".Log();
                e.ToString().Log();
                "回车键退出".Log();
                Console.ReadLine();
                return;
            }
            $"这是你的BE我的世界路径 {CppConfigHelper.CppGamePath} 请确保它是有效的".Log();

            //登录
            x19AuthEntities.AuthOtpRetEntity_Entity auth;
            Login:
            {
                "请选择你要登录的方式".Log();
                "1. 使用Cookies登录".Log();
                "2. 使用4399登录".Log();
                Console.Write("> ");
                string cookies;
                switch (Console.ReadLine().Trim().Replace(" ",""))
                {
                    case "1":
                    {
                        "请输入Cookies".Log();
                        Console.Write("> ");
                        cookies = Console.ReadLine();
                        break;
                    }
                    case "2":
                    {
                        "请输入4399账号".Log();
                        Console.Write("> ");
                        var account = Console.ReadLine();
                        "请输入4399密码".Log();
                        Console.Write("> ");
                        var password = Console.ReadLine();
                        var ret = x19Auth.Pt4399Login(account, password);
                        if (ret.Item2 == 0)
                        {
                            cookies = ret.Item1;
                            break;
                        }
                        else
                        {
                            $"登录失败了，原因：{ret.Item1}".Log();
                            goto Login;
                        }
                    }
                    default:
                    {
                        goto Login;
                    }
                }
                
                var authRet = x19Auth.CookiesLogin(cookies);
                if (authRet.Item2 != null)
                {
                    "登录时发生错误".Log();
                    authRet.Item2.ToString().Log();
                    goto Login;
                }
                auth = authRet.Item1;
                $"登录成功， UserId:{auth.entity_id}".Log();
            }
            //设置昵称
            var nickname = x19Auth.CheckHasNickname(auth);
            SetNickName:
            {
                if (nickname == "")
                {
                    "当前账号未设置昵称，请输入昵称或回车键随机生成一个昵称".Log();
                    Console.Write("> ");
                    var settingNickname = Console.ReadLine();
                    var random = false;
                    if (settingNickname == "")
                    {
                        //随机名称
                        random = true;
                        settingNickname = StringExtensions.RandomLetter(11);
                        $"随机生成的昵称 {settingNickname}".Log();
                    }

                    var checkNickname = x19Auth.CheckNickname(auth, settingNickname);
                    if (checkNickname != "")
                    {
                        //检验名称失败
                        if (checkNickname == "包含敏感词")
                        {
                            //包含违禁词
                            if (random)
                            {
                                "随机生成的昵称包含敏感词，请重新设置一个昵称".Log();
                                goto SetNickName;
                            }
                            else
                            {
                                $"{settingNickname} 包含违禁词，请重新设置一个昵称".Log();
                                goto SetNickName;
                            }
                        }
                        else
                        {
                            //已被占用
                            if (random)
                            {
                                "随机生成的昵称被占用了，请重新设置一个昵称".Log();
                                goto SetNickName;
                            }
                            else
                            {
                                $"{settingNickname} 被占用了，请重新设置一个昵称".Log();
                                goto SetNickName;
                            }
                        }
                        
                    }
                    //设置昵称
                    nickname = x19Auth.SetNickName(auth,settingNickname);
                }else
                    //昵称已存在，无需设置
                    $"该账号已存在昵称 欢迎 {nickname}".Log();
            }
            "回车键启动游戏".Log();
            Console.Write("> ");
            Console.ReadLine();
            
            //启动游戏
            var configFile = @$"{Directory.GetCurrentDirectory()}\Drug.cppconfig";
            var errorLogFile = @$"{Directory.GetCurrentDirectory()}\errorLog.log";
            var skinFile = @$"{Directory.GetCurrentDirectory()}\skin.png";
            if (!File.Exists(skinFile))
            {
                "检测到皮肤文件不存在，已生成默认皮肤".Log();
                File.Create(skinFile).Close();
                File.WriteAllBytes(skinFile,resources.skin);
            }
            var skinHash = HashHelper.CompleteMD5FromFile(skinFile);
            var gameProcess = new CppConfigHelper("Drug", auth.response, Convert.ToInt64(auth.entity_id),
                configFile,nickname,skinFile,skinHash,true,
                "4654307171942868781","play.bjd-mc.com",19132).GetProcess(errorLogFile);
            gameProcess.Start();
            "游戏已启动，无法进入服务器等情况请重新启动游戏".Log();
            
            //Crack
            MemoryCracker.CrackCppGameProcess();
            
            //等待游戏关闭
            gameProcess.WaitForExit();
            $"游戏已经结束 进程的返回值: {gameProcess.ExitCode}".Log();
            "回车键继续".Log();
            Console.ReadLine();
            Console.Clear();
            Main(null);
        }
    }
}