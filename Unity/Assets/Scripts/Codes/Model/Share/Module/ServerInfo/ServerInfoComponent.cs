
using System.Collections.Generic;

namespace ET
{
    [ComponentOf]
    public class ServerInfoComponent:Entity, IAwake, IDestroy
    {
        public List<ServerInfo> ServerInfoList = new List<ServerInfo>();
    }
}
