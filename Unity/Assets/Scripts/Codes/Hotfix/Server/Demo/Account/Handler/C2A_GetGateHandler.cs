namespace ET.Server
{
    [MessageHandler(SceneType.Account)]
    public class C2A_GetGateHandler: AMRpcHandler<C2A_GetGate, A2C_GetGate>
    {
        protected override async ETTask Run(Session session, C2A_GetGate request, A2C_GetGate response)
        {
            // token 验证
            string token = session.DomainScene().GetComponent<TokenComponent>().Get(request.AccountId);

            if (token == null || token != request.Token)
            {
                response.Error = ErrorCode.ERR_TokenError;
                session?.Disconnect().Coroutine();
                return;
            }

            if (session.GetComponent<SessionLockComponent>() != null)
            {
                // session 在第一次请求时挂载SessionLockComponent 组件，后续请求判断此时已挂载，则自动取消
                response.Error = ErrorCode.ERR_RequestRespeated;
                session.Disconnect().Coroutine();
                return;
            }

            using (session.AddComponent<SessionLockComponent>())
            {
                using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.LoginAccount, request.AccountId))
                {
                    // 验证通过，获取Gate信息   Gate 数据暂时不返回，Role 创建成功后再选定 Gate
                    StartSceneConfig config = RealmGateAddressHelper.GetGate(session.DomainZone(), request.AccountId);
                    Log.Debug($"gate address: {MongoHelper.ToJson(config)}");

                    G2R_GetLoginKey g2RGetLoginKey = (G2R_GetLoginKey)await ActorMessageSenderComponent.Instance.Call(
                        config.InstanceId, new R2G_GetLoginKey() { Account = request.AccountName });

                    response.Address = config.InnerIPOutPort.ToString();
                    response.Key = g2RGetLoginKey.Key;
                    response.GateId = g2RGetLoginKey.GateId;
                }
            }
        }
    }
}