using System.Collections.Generic;

namespace ET.Server
{
    // �û��������˴洢�����ӿͻ��˵� Token
    [ComponentOfAttribute]
    public class TokenComponent:Entity, IAwake
    {
        public readonly Dictionary<long, string> TokenDictionary = new Dictionary<long, string>();

    }
}