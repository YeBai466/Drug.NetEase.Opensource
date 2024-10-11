using System;
using System.Diagnostics;
using System.IO;
using Drug.NetEase.Server.BaseAuth;
using Drug.NetEase.Server.Extensions;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace Drug.NetEase.Server.Utilities;

public class CppConfigHelper
{
    public static string CppGamePath
    {
        get
        {
            if (_cppGamePath == null)
            {
                try
                {
                    _cppGamePath = (string)Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Netease\MCLauncher").GetValue("DownloadPath") + @"\MinecraftBENetease\windowsmc";
                    if (!Directory.Exists(_cppGamePath))
                        throw new Exception();
                }
                catch
                {
                    try
                    {
                        _cppGamePath = (string)Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Netease\PC4399_MCLauncher").GetValue("DownloadPath") + @"\MinecraftBENetease\windowsmc";
                        if (!Directory.Exists(_cppGamePath))
                            throw new Exception();
                    }
                    catch
                    {
                        _cppGamePath = "";
                    }
                }
            }
            return _cppGamePath;
        }
    }

    private static string _cppGamePath;
    
    public CppConfigHelper(string baseName, string h5Token, long userId, string configPath,
        string userName, string skinFile, string skinMd5, bool skinSlim, string serverId = "4654307171942868781",
        string serverAddress = "play.bjd-mc.com", int serverPort = 19132)
    {
        room_info.room_name = baseName;
        world_info.name = baseName;
        room_info.room_name = baseName;
        player_info.urs = baseName;
        room_info.token = h5Token;
        player_info.user_id = userId;
        player_info.user_name = userName;
        skin_info.skin = skinFile;
        skin_info.md5 = skinMd5;
        skin_info.slim = skinSlim;
        world_info.level_id = serverId;
        room_info.item_ids = new[] { serverId };
        room_info.ip = serverAddress;
        room_info.port = serverPort;
        path = configPath;

        misc.auth_server_url = x19Auth.ReleaseJson.AuthServerCppUrl;
        web_server_url = x19Auth.ReleaseJson.ApiGatewayUrl;
        core_server_url = x19Auth.ReleaseJson.CoreServerUrl;
    }

    public string GetConfig()
    {
        return JsonConvert.SerializeObject(this).Encrypt();
    }

    public Process GetProcess(string errorFile)
    {
        if (File.Exists(path))
            File.Delete(path);
        if (File.Exists(errorFile))
            File.Delete(errorFile);
        File.Create(path).Close();
        File.Create(errorFile).Close();
        File.WriteAllText(path,GetConfig());
        var process = new Process
        {
            StartInfo =
            {
                FileName = @$"{CppGamePath}\Minecraft.Windows.exe",
                WorkingDirectory = CppGamePath,
                Arguments = $"config=\"{path}\" errorlog=\"{errorFile}\"",
                UseShellExecute = false
            }
        };
        return process;
    }
    public LaunchParams launch_params = new ();
    public class LaunchParams
    {
        public bool close_pet_addon = false;
    }
    public WorldInfo world_info = new ();
    public class WorldInfo
    {
        public string level_id;
        public int game_type = 100;
        public int difficulty = 2;
        public int permission_level = 1;
        public bool cheat = false;
        public bool other_cheat = false;
        public CheatInfo cheat_info = new ();
        public string[] resource_packs = {""};
        public string[] behavior_packs = {""};
        public string name;
        public int world_type = 1;
        public bool start_with_map = false;
        public bool bonus_items = false;
        public string seed = "";
    }
    public class CheatInfo
    {
        public bool pvp = true;
        public bool show_coordinates = false;
        public bool always_day = false;
        public bool daylight_cycle = true;
        public bool fire_spreads = true;
        public bool tnt_explodes = true;
        public bool keep_inventory = false;
        public bool mob_spawn = true;
        public bool natural_regeneration = true;
        public bool mob_loot = true;
        public bool mob_griefing = true;
        public bool tile_drops = true;
        public bool entities_drop_loot = true;
        public bool weather_cycle = true;
        public bool command_blocks_enabled = true;
        public int random_tick_speed = 1;
        public bool experimental_gameplay = false;
        public bool experimental_holiday = false;
        public bool experimental_biomes = false;
        public bool experimental_modding = false;
        public bool fancy_bubbles = false;
    }
    public RoomInfo room_info = new ();
    public class RoomInfo
    {
        public string ip;
        public int port;
        public string room_name;
        public string token;
        public int room_id = 0;
        public int host_id = 0;
        public bool allow_pe = true;
        public int max_player = 0;
        public int visibility_mode = 0;
        public string[] item_ids;
        public object tag_ids;
        public int simple_game_version = 0;
        public object create_room_extra_bits;
    }
    public PlayerInfo player_info = new ();
    public class PlayerInfo
    {
        public long user_id;
        public string user_name;
        public string urs;
    }
    public SkinInfo skin_info = new ();
    public class SkinInfo
    {
        public string skin;
        public string md5;
        public bool slim;
        public string skin_iid = "100";
    }
    public AntiAddictionInfo anti_addiction_info = new ();
    public class AntiAddictionInfo
    {
        public bool enable = false;
        public int left_time = 0;
        public int exp_multiplier = 1;
        public int block_multplier = 1;
        public string first_message = "";
    }
    public Misc misc = new ();
    public class Misc
    {
        public int multiplayer_game_type = 100;
        public string auth_server_url;
        public int launcher_port = 0;
        public string sensitive_word_file = "";
    }
    public string path;
    public string web_server_url;
    public string core_server_url;
    public SocialSetting social_setting = new ();
    public class SocialSetting
    {
        public bool underage_mode = false;
        public bool block_strangers = false;
        public bool block_all_messages = false;
        public bool block_reposted_and_commented = false;
        public int message_visibility = 0;
    }
    public string[] vip_using_mod = Array.Empty<string>();
    public ResultEntity Result = new ();
    public class ResultEntity
    {
        public bool a = true;
        public string b = "";
        public object c = null;
    }
}