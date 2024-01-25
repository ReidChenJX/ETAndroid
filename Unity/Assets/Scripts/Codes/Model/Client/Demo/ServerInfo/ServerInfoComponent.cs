
using System.Collections.Generic;

namespace ET.Client
{
    [ComponentOf]
    public class ServerInfoComponent:Entity, IAwake, IDestroy
    {
        public List<ServerInfo> ServerInfoList { get; set; } = new List<ServerInfo>();
        public int CurrentServerId { get; set; }

    }
}
