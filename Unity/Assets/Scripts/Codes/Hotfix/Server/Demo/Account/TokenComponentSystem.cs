

namespace ET.Server
{
    [FriendOf(typeof(TokenComponent))]
    public static class TokenComponentSystem
    {
        // 服务器端Token 方法
        public static void Add(this TokenComponent self, long key, string token)
        {
            self.TokenDictionary.Add(key, token);
            self.TimeOutRemoveKey(key, token).Coroutine();
        }


        public static string Get(this TokenComponent self, long key)
        {
            string value;
            self.TokenDictionary.TryGetValue(key, out value);
            return value;
        }

        public static void Remove(this TokenComponent self, long key)
        {
            if (self.TokenDictionary.ContainsKey(key))
            {
                self.TokenDictionary.Remove(key);
            }
        }

        // Token 过期移出
        private static async ETTask TimeOutRemoveKey(this TokenComponent self, long key, string token)
        {
            await TimerComponent.Instance.WaitAsync(600000);
            
            string onlineToken = self.Get(key);
            if(!string.IsNullOrEmpty(onlineToken) && onlineToken == token)
            {
                self.Remove(key);
            }
        }

    }
}
