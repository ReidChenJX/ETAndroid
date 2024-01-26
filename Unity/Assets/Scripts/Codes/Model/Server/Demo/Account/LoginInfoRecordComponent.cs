using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class LoginInfoRecordComponent : Entity, IAwake, IDestroy
    {
        // <accountId  SceneID>
        public Dictionary<long, int> AccountLoginInfoDict = new Dictionary<long, int>();
    }
}