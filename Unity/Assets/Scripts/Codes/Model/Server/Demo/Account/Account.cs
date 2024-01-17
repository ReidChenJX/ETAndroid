namespace ET.Server
{
    public enum AccountType
    {
        General = 0,
        BlackList = 1,
    }

    [ChildOfAttribute]
    public class Account: Entity, IAwake
    {
        public string AccountName { get; set; }     // Account ��¼��
        public string PassWord { get; set; }          // ����
        public long CreateTime;         // ����ʱ��
        public int AccountType;         // �˻�����

    }



}