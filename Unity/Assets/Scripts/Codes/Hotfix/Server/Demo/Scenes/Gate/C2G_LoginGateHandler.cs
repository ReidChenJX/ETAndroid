using System;


namespace ET.Server
{
	[MessageHandler(SceneType.Gate)]
	public class C2G_LoginGateHandler : AMRpcHandler<C2G_LoginGate, G2C_LoginGate>
	{
		protected override async ETTask Run(Session session, C2G_LoginGate request, G2C_LoginGate response)
		{
			
			if (session.GetComponent<SessionLockComponent>() != null)
			{
				// session 在第一次请求时挂载SessionLockComponent 组件，后续请求判断此时已挂载，则自动取消
				response.Error = ErrorCode.ERR_RequestRespeated;
				session.Disconnect().Coroutine();
				return;
			}
			
			Scene scene = session.DomainScene();
			string account = scene.GetComponent<GateSessionKeyComponent>().Get(request.Key);
			if (account == null)
			{
				response.Error = ErrorCore.ERR_ConnectGateKeyError;
				response.Message = "Gate key验证失败!";
				return;
			}
			// 链接通过，移出当前Session 上的自动销毁定时器
			session.RemoveComponent<SessionAcceptTimeoutComponent>();
			
			scene.GetComponent<GateSessionKeyComponent>().Remove(request.Key);
			long instanceId = session.InstanceId;
			using (session.AddComponent<SessionLockComponent>())
			{
				using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.LoginGate, request.Key))
				{
					if (instanceId != session.InstanceId)
					{
						return;
					}
					
					// 由Gate 服务器向 LoginCenter 服务器发送信息
					StartSceneConfig loginCenterConfig = StartSceneConfigCategory.Instance.LoginCenterConfig;
					L2G_AddLoginRecord l2GAddLoginRecord = (L2G_AddLoginRecord)await ActorMessageSenderComponent.Instance.Call(loginCenterConfig.InstanceId,
						new G2L_AddLoginRecord() { AccountId = request.AccountId, ServerId = scene.Zone });

					if (l2GAddLoginRecord.Error != ErrorCode.ERR_Success)
					{
						Log.Debug(l2GAddLoginRecord.Error.ToString());
						session?.Disconnect().Coroutine();
						return;
					}
					
					// PlayerComponent : <AccountId, Player> 
					Player player = scene.GetComponent<PlayerComponent>().Get(request.AccountId);

					if (player == null)
					{
						// 添加一个新的GateUnit
						PlayerComponent playerComponent = scene.GetComponent<PlayerComponent>();
						player = playerComponent.AddChild<Player, string>(account);
						playerComponent.Add(player);
						
					}

					session.AddComponent<SessionPlayerComponent>().PlayerId = player.Id;
					session.AddComponent<MailBoxComponent, MailboxType>(MailboxType.GateSession);
					
					response.PlayerId = player.Id;
					await ETTask.CompletedTask;
					
				}
			}
		}
	}
}