namespace Drug.NetEase.Server.BaseAuth;

public class DrugDataBaseEntities
{
    public class UserEntity
    {
        public int userid;
        public string username;
        public string registeraddress;
        public string usertoken;
        public string password;
        public string createtime;
        public string expiredtime;
        public string machinecode;
        public string banmessage;
    }
}