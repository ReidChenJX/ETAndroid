namespace ET.Server
{
    public enum AccountType
    {
        General = 0,
        BlackList = 1,
    }

    public class Account: Entity, IAwake
    {
        public string AccountName;      // Account 登录名
        public string PassWord;         // 密码
        public long CreateTime;         // 创建时间
        public int AccountType;         // 账户类型

    }



}