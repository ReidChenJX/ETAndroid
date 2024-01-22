namespace ET.Server
{
    [ActorMessageHandler(SceneType.LoginCenter)]
    public class A2L_LoginAccountRequestHandler : AMActorRpcHandler<Scene, A2L_LoginAccountRequest, L2A_LoginAccountResponse>
    {
        // A2L_LoginAccountRequestHandler 登录服务器与账户中心服务器消息处理类，判断账户是否已经登录
        protected override async ETTask Run(Scene scene, A2L_LoginAccountRequest request, L2A_LoginAccountResponse response)
        {
            long accountId = request.AccountId;
            // 账户中心服务器协程锁
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.LoginCenterLock, accountId.GetHashCode()))
            {
                if (!scene.GetComponent<LoginInfoRecordComponent>().IsExist(accountId))
                {
                    return;
                }
                // 账户目前所处的 scene id
                int zone = scene.GetComponent<LoginInfoRecordComponent>().Get(accountId);
                // 该 scene（如map等）对应的Gate 网关服务器
                StartSceneConfig gateConfig = RealmGateAddressHelper.GetGate(zone, accountId);

                // 向 网关服务器 发送下线申请
                var disconnectGateUnit = (G2L_DisconnectGateUnit)await MessageHelper.CallActor(gateConfig.InstanceId, new L2G_DisconnectGateUnit() { AccountId = accountId });
                response.Error = disconnectGateUnit.Error;

            }
        }
    }
}