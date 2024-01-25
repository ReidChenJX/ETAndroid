using System.Collections.Generic;
using System.Linq;

namespace ET.Server
{
    public class ServerInfoManagerComponentAwakeSystem: AwakeSystem<ServerInfoManagerComponent>
    {
        protected override void Awake(ServerInfoManagerComponent self)
        {
            self.Awake().Coroutine();
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
            // 从数据库中获取 区服服务器列表
            List<ServerInfo> serverDBList = await DBManagerComponent.Instance.GetZoneDB(self.DomainZone()).Query<ServerInfo>(d => true);

            if (serverDBList == null || serverDBList.Count() <= 0)
            {
                Log.Error("DB ServerInfo count is zero");
                
                // 从 Excel 初始化区服信息
                self.ServerInfos.Clear();
                var serverInfoConfig = ServerInfoConfigCategory.Instance.GetAll();

                foreach (var info in serverInfoConfig.Values)
                {
                    ServerInfo newServerInfo = self.AddChildWithId<ServerInfo>(info.Id);
                    newServerInfo.Status = (int)ServerStatus.Normal;
                    newServerInfo.ServerName = info.ServerName;
                    
                    self.ServerInfos.Add(newServerInfo);
                    
                    // 入库
                    await DBManagerComponent.Instance.GetZoneDB(self.DomainZone()).Save(newServerInfo);

                }
            }
            
            self.ServerInfos.Clear();

            foreach (var serverinfo in serverDBList)
            {
                self.AddChild(serverinfo);
                self.ServerInfos.Add(serverinfo);
                
            }
        }
    
    }
}

