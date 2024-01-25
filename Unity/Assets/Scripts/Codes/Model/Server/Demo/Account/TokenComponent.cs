using System.Collections.Generic;

namespace ET.Server
{
    // 用户服务器端存储已连接客户端的 Token
    [ComponentOfAttribute]
    public class TokenComponent:Entity, IAwake
    {
        public readonly Dictionary<long, string> TokenDictionary = new Dictionary<long, string>();

    }
}