
namespace ET
{
    public class ServerInfoComponentDestroySystem : DestroySystem<ServerInfoComponent>
    {
        protected override void Destroy(ServerInfoComponent self)
        {
            foreach (ServerInfo serverinfo in self.ServerInfoList)
            {
                serverinfo?.Dispose();
            }

            self.ServerInfoList.Clear();
        }
    }



    [FriendOf(typeof(ServerInfoComponent))]
    public static class ServerInfoComponentSystem
    {
        public static void Add(this ServerInfoComponent self, ServerInfo serverinfo)
        {
            self.ServerInfoList.Add(serverinfo);
        }
    }
}
