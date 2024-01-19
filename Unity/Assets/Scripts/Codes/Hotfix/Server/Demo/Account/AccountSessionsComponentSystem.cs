
namespace ET.Server
{
    public class AccountSessionsComponentDestorySystem : DestroySystem<AccountSessionsComponent>
    {
        protected override void Destroy(AccountSessionsComponent self)
        {
            self.AccountSessionDictionary.Clear();
        }
    }




    [FriendOf(typeof(AccountSessionsComponent))]
    public static class AccountSessionsComponentSystem
    {
        public static long Get(this AccountSessionsComponent self, long accountId)
        {
            if (!self.AccountSessionDictionary.TryGetValue(key: accountId, out long instanceId))
            {
                return 0;
            }

            return instanceId;
        }

        public static void Add(this AccountSessionsComponent self, long accountId, long sessionInstanceId)
        {
            if (self.AccountSessionDictionary.ContainsKey(key: accountId))
            {
                self.AccountSessionDictionary[accountId] = sessionInstanceId;
                return;

            }
            self.AccountSessionDictionary.Add(accountId, sessionInstanceId);
        }

        public static void Remove(this AccountSessionsComponent self, long accountId)
        {
            if (self.AccountSessionDictionary.ContainsKey(key: accountId))
            {
                self.AccountSessionDictionary.Remove(accountId);
            }
        }
    }
}
