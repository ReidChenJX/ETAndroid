
namespace ET.Server
{
    [ComponentOf]
    public class AccountCheckOutTimeComponent:Entity, IAwake<long>, IDestroy
    {
        public long Time = 0;
        public long AccountId;
    }
}
