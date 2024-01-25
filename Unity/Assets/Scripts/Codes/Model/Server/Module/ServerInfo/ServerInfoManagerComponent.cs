using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf]
    public class ServerInfoManagerComponent: Entity, IAwake, IDestroy, ILoad
    {
        public List<ServerInfo> ServerInfos { get; set; } = new List<ServerInfo>();
    }
}

