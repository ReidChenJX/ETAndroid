namespace ET.Server
{
    public class ServerInfoManagerComponentAwakeSystem: AwakeSystem<ServerInfoManagerComponent>
    {
        protected override void Awake(ServerInfoManagerComponent self)
        {
            self.Awake().Coroutine();
            
            throw new System.NotImplementedException();
        }
    }

    public class ServerInfoManagerComponentDestroySystem: DestroySystem<ServerInfoManagerComponent>
    {
        protected override void Destroy(ServerInfoManagerComponent self)
        {
            foreach (var serverInfo in self.ServerInfos)
            {
                serverInfo?.Dispose();
            }
            self.ServerInfos.Clear();
        }
    }

    public class ServerInfoManagerComponentLoadSystem: LoadSystem<ServerInfoManagerComponent>
    {
        protected override void Load(ServerInfoManagerComponent self)
        {
            self.Awake().Coroutine();
        }
    }
    
    [FriendOf(typeof(ServerInfoManagerComponent))]
    public static class ServerInfoManagerComponentSystem
    {
        public static async ETTask Awake(this ServerInfoManagerComponent self)
        {
            // 从数据库中获取 游戏服务器列表
            
            
            await ETTask.CompletedTask;
        }
    
    }
}

