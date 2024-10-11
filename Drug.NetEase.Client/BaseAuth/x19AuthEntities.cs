namespace Drug.NetEase.Client.BaseAuth;

public static class x19AuthEntities
{
    public class LoginOtpEntity
    {
        public string sauth_json;
    }
    public class LoginOtpEntity_sauth_json
    {
        public string sdkuid;
        public string sessionid;
        public string udid;
        public string deviceid;
    }
    public class AuthOtpRetEntity
    {
        public int code;
        public string message;
        public AuthOtpRetEntity_Entity entity;
    }

    public class AuthOtpRetEntity_Entity
    {
        public string entity_id;
        public string token;

        public string response;
    }
    public class ReleaseJsonEntity
    {
        public int HostNum;
        public int ServerHostNum;
        public int TempServerStop;
        public string CdnUrl;
        public string StaticWebVersionUrl;
        public string StaticWeb2VersionUrl;
        public string SeadraUrl;
        public string EmbedWebPageUrl;
        public string NewsVideo;
        public string GameCenter;
        public string VideoPrefix;
        public string ComponentCenter;
        public string GameDetail;
        public string CompDetail;
        public string LiveUrl;
        public string ForumUrl;
        public string WebServerUrl;
        public string WebServerGrayUrl;
        public string CoreServerUrl;
        public string TransferServerUrl;
        public string PeTransferServerUrl;
        public string PeTransferServerHttpUrl;
        public string TransferServerHttpUrl;
        public string PeTransferServerNewHttpUrl;
        public string AuthServerUrl;
        public string AuthServerCppUrl;
        public string AuthorityUrl;
        public string CustomerServiceUrl;
        public string ChatServerUrl;
        public string PathNUrl;
        public string PePathNUrl;
        public string MgbSdkUrl;
        public string DCWebUrl;
        public string ApiGatewayUrl;
        public string ApiGatewayGrayUrl;
        public string PlatformUrl;
        public string RentalTransferUrl;
    }
    public class LoginOtpRetEntity
    {
        public int code;
        public string message;
        public LoginOtpRetEntity_entity entity;
    }
    public class LoginOtpRetEntity_entity
    {
        public string otp_token;
        public int aid;
    }
    public class CheckNicknameEntity
    {
        public int code;
        public string message;
    }
    public class SetNicknameEntity
    {
        public int code;
        public string details;
        public SetNicknameEntity_Entity entity;
    }
    public class SetNicknameEntity_Entity
    {
        public string entity_id;
        public string name;
    }
}