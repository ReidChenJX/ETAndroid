

using System;
using System.Timers;

namespace ET.Server
{
    [Invoke(TimerInvokeType.AccountSessionCheckOut)]
    public class AccountSessionCheckOutTimer : ATimer<AccountCheckOutTimeComponent>
    {
        protected override void Run(AccountCheckOutTimeComponent self)
        {
            try
            {
                self.DelectSession();
            }
            catch(Exception e)
            {
                Log.Error(e.ToString());
            }
        }
    }





    public class AccountCheckOutTimeComponentAwakeSystem : AwakeSystem<AccountCheckOutTimeComponent, long>
    {
        protected override void Awake(AccountCheckOutTimeComponent self, long accountId)
        {
            self.AccountId = accountId;
            // 定时器：如果长时间链接Account服务器，则断开链接
            TimerComponent.Instance.Remove(ref self.Time);
            self.Time = TimerComponent.Instance.NewOnceTimer(TimeHelper.ServerNow() + 600000, TimerInvokeType.AccountSessionCheckOut, self);
        }
    }

    public class AccountCheckOutTimeComponentDestroySystem : DestroySystem<AccountCheckOutTimeComponent>
    {
        protected override void Destroy(AccountCheckOutTimeComponent self)
        {
            self.AccountId = 0;
            TimerComponent.Instance.Remove(ref self.Time);
        }
    }




    [FriendOf(typeof(AccountCheckOutTimeComponent))]
    public static  class AccountCheckOutTimeComponentSystem
    {
        public static void DelectSession(this AccountCheckOutTimeComponent self)
        {
            Session session = self.GetParent<Session>();
            long sessionInstanceId = session.DomainScene().GetComponent<AccountSessionsComponent>().Get(self.AccountId);

            if(session.InstanceId == sessionInstanceId)
            {
                session.DomainScene().GetComponent<AccountSessionsComponent>().Remove(self.AccountId);
            }

            // 断线消息发送
            session?.Send(new A2C_Disconnect() { Error = 0});
            session?.Disconnect().Coroutine();


        }
    }
}
