using System.Collections.Generic;
using System.Net.Sockets;

namespace ET.Client
{
    [ComponentOf]
    public class RoleInfoComponent: Entity, IAwake, IDestroy
    {
        public List<RoleInfo> RoleInfos { get; set; } = new List<RoleInfo>();
        public long CurrentRoleId { get; set; } = 0;

    }
}

