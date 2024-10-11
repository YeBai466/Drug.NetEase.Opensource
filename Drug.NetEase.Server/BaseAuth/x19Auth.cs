using Newtonsoft.Json;

namespace Drug.NetEase.Server.BaseAuth;

public class x19Auth
{
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
}