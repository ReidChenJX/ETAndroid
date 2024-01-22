namespace ET.Server
{
    [ActorMessageHandler(SceneType.LoginCenter)]
    public class A2L_LoginAccountRequestHandler : AMActorRpcHandler<Scene, A2L_LoginAccountRequest, L2A_LoginAccountResponse>
    {
        protected override async ETTask Run(Scene unit, A2L_LoginAccountRequest request, L2A_LoginAccountResponse response)
        {
            await ETTask.CompletedTask;
        }
    }