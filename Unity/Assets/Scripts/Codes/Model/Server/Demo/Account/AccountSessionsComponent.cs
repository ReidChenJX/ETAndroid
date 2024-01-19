using System.Collections.Generic;

namespace ET.Server
{
    // 服务器管理登录会话，依次可实现顶号操作等功能
    // <long accountId, long SessionInstanceId>
    [ComponentOf]
    public class AccountSessionsComponent:Entity, IAwake, IDestroy
    {
        public readonly Dictionary<long, long> AccountSessionDictionary = new Dictionary<long, long>();
    }
}
