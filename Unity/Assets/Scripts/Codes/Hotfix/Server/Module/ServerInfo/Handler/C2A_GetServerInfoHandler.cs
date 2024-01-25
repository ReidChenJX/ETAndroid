using System.Collections.Generic;

namespace ET.Server
{
    [MessageHandler(SceneType.Account)]
    public class C2A_GetServerInfoHandler: AMRpcHandler<C2A_GetServerInfo, A2C_GetServerInfo>
    {
        // 消息处理类：客户端申请服务器列表
        protected override async ETTask Run(Session session, C2A_GetServerInfo request, A2C_GetServerInfo response)
        {
            // token 验证
            string token = session.DomainScene().GetComponent<TokenComponent>().Get(request.AccountId);

            if (token == null || token != request.Token)
            {
                response.Error = ErrorCode.ERR_TokenError;
                session?.Disconnect().Coroutine();
                return;
            }
            
            // 区服服务器信息返回
            List<ServerInfoProto> serverInfoProto = new List<ServerInfoProto>();
            foreach (var serverInfo in session.DomainScene().GetComponent<ServerInfoManagerComponent>().ServerInfos)
            {
                serverInfoProto.Add(serverInfo.ToMessage());
                Log.Debug("foreach success");
            }

            response.ServerInfoList = serverInfoProto;
            await ETTask.CompletedTask;

        }
    }
}
