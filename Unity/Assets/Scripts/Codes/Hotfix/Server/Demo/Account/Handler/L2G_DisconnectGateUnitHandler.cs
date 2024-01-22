namespace ET.Server
{
    [ActorMessageHandler(SceneType.Gate)]
    public class L2G_DisconnectGateUnitHandler : AMActorRpcHandler<Scene, L2G_DisconnectGateUnit, G2L_DisconnectGateUnit>
    {
        protected override async ETTask Run(Scene scene, L2G_DisconnectGateUnit request, G2L_DisconnectGateUnit response)
        {

            long accountId = request.AccountId;

            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.GateLoginLock, accountId.GetHashCode()))
            {
                // Gate 上的玩家管理组件
                PlayerComponent playerComponent = scene.GetComponent<PlayerComponent>();
                Player gateUnit= playerComponent.Get(accountId);

                if(gateUnit == null)
                {
                    return;
                }

                // 玩家下线操作
                playerComponent.Remove(accountId);
                gateUnit.Dispose();

            }
        }
    }
}
