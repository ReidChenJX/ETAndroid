
using System.Collections.Generic;

namespace ET.Client
{
    [ComponentOf]
    public class ServerInfoComponent:Entity, IAwake, IDestroy
    {
        public List<ServerInfo> ServerInfoList = new List<ServerInfo>();
        public int CurrentServerId { get; set; }

    }
}
