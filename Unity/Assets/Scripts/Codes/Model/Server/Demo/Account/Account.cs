namespace ET.Server
{
    public enum AccountType
    {
        General = 0,
        BlackList = 1,
    }

    public class Account: Entity, IAwake
    {
        public string AccountName;      // Account ��¼��
        public string PassWord;         // ����
        public long CreateTime;         // ����ʱ��
        public int AccountType;         // �˻�����

    }



}