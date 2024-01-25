using MongoDB.Driver.Core.Clusters.ServerSelectors;
using MongoDB.Driver.Core.WireProtocol.Messages;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ET.Server
{
    [MessageHandler(SceneType.Account)]
    public class C2A_LoginAccountHandler : AMRpcHandler<C2A_LoginAccount, A2C_LoginAccount>
    {
        protected override async ETTask Run(Session session, C2A_LoginAccount request, A2C_LoginAccount response)
        {
            Log.Debug("C2A_LoginAccountHandler: 消息处理");
            if (session.DomainScene().SceneType != SceneType.Account)
            {
                Log.Error($"请求的Scene错误，登录请求当前Scene为：{session.DomainScene().SceneType}");
                session?.Dispose();
                return;
            }

            // 链接通过，移出当前Session 上的自动销毁定时器
            session.RemoveComponent<SessionAcceptTimeoutComponent>();

            // 锁定第一次验证
            if (session.GetComponent<SessionLockComponent>() != null)
            {
                // session 在第一次请求时挂载SessionLockComponent 组件，后续请求判断此时已挂载，则自动取消
                response.Error = ErrorCode.ERR_RequestRespeated;
                session.Disconnect().Coroutine();
                return;
            }

            // 登录验证
            if (string.IsNullOrEmpty(request.AccountName) || string.IsNullOrEmpty(request.PassWord))
            {
                response.Error = ErrorCode.ERR_LoginInfoError;

                // * 未返回消息先注销Session 是否会导致 response 无法返回？
                session.Disconnect().Coroutine();
                return;
            }

            if (!Regex.IsMatch(request.PassWord.Trim(), @"^(?=.*[0-9].*)(?=.*[A-Z].*)(?=.*[a-z].*).{6,15}$"))
            {
                response.Error = ErrorCode.ERR_PasswordFormError;
                session.Disconnect().Coroutine();
                return;
            }


            Log.Debug("C2A_LoginAccountHandler: ErrorCode.ERR_Success 进入验证环节");
            // 请求成功，对session 进行锁定
            using (session.AddComponent<SessionLockComponent>())
            {
                // 防止同时登录注册，对数据库请求锁定
                using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.LoginAccount, request.AccountName.Trim().GetHashCode()))
                {
                    // 对Password MD5加密
                    string passWordMD5 = MD5Helper.StringMD5(request.PassWord.Trim());
                    // 数据库验证
                    var accountInfoList = await DBManagerComponent.Instance.GetZoneDB(session.DomainZone()).Query<Account>(d => d.AccountName.Equals(request.AccountName.Trim()));
                    Account account = null;

                    if (accountInfoList != null && accountInfoList.Count > 0)
                    {
                        account = accountInfoList[0];
                        session.AddChild(account);

                        if (account.AccountType == (int)AccountType.BlackList)
                        {
                            response.Error = ErrorCode.ERR_AccountBlackListError;
                            session.Disconnect().Coroutine();
                            account?.Dispose();
                            //return;
                        }
                        if (!account.PassWord.Equals(passWordMD5))
                        {
                            response.Error = ErrorCode.ERR_LoginInfoError;
                            session.Disconnect().Coroutine();
                            account?.Dispose();
                            //return;
                        }

                    }
                    else
                    {
                        Log.Debug("数据库无数据，自动创建。");
                        // Session 也是组件，新建的account 需要挂载在其下，用于记录，入库账户信息
                        account = session.AddChild<Account>();
                        account.AccountName = request.AccountName.Trim();
                        account.PassWord = passWordMD5;
                        account.CreateTime = TimeHelper.ServerNow();
                        account.AccountType = (int)AccountType.General;

                        // 数据入库
                        await DBManagerComponent.Instance.GetZoneDB(session.DomainZone()).Save<Account>(account);

                        Log.Debug("数据入库成功");
                    }

                    Log.Debug("ErrorCode.ERR_Success: 验证成功后，判断该账号是否已经登录");

                    // 验证成功后，判断该账号是否已在账户中心服务器
                    StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.GetBySceneName(session.DomainZone(), "LoginCenter");
                    long loginCenterInstanceId = startSceneConfig.InstanceId;
                    var loginAccountRespone = (L2A_LoginAccountResponse)await ActorMessageSenderComponent.Instance.Call(
                        loginCenterInstanceId, new A2L_LoginAccountRequest() { AccountId = account.Id });

                    if (loginAccountRespone.Error != ErrorCode.ERR_Success)
                    {
                        response.Error = ErrorCode.ERR_ExtraLoginCenter;
                        session.Disconnect().Coroutine();
                        account?.Dispose();
                        return;
                    }
                    
                    // 判断该账号是否已在登录服务器
                    long accountSessionInstanceId = session.DomainScene().GetComponent<AccountSessionsComponent>().Get(account.Id);
                    Session otherSession = Root.Instance.Get(accountSessionInstanceId) as Session;

                    // 该账号已登录，由服务器向客户端发起退线
                    if (otherSession != null)
                    {
                        otherSession.Send(new A2C_Disconnect() { Error = ErrorCode.ERR_ExtraAccount });
                        otherSession.Disconnect().Coroutine();
                    }

                    session.DomainScene().GetComponent<AccountSessionsComponent>().Add(account.Id, session.InstanceId);
                    // 登录请求持续一定时间后，自动断开
                    session.AddComponent<AccountCheckOutTimeComponent, long>(account.Id);

                    // 验证通过，获取Gate信息   Gate 数据暂时不返回，Role 创建成功后再选定 Gate
                    // StartSceneConfig config = RealmGateAddressHelper.GetGate(session.DomainZone(), account.Id);
                    // Log.Debug($"gate address: {MongoHelper.ToJson(config)}");
                    //
                    // G2R_GetLoginKey g2RGetLoginKey = (G2R_GetLoginKey)await ActorMessageSenderComponent.Instance.Call(
                    //     config.InstanceId, new R2G_GetLoginKey() { Account = request.AccountName });
                    //
                    // response.Address = config.InnerIPOutPort.ToString();
                    // response.Key = g2RGetLoginKey.Key;
                    // response.GateId = g2RGetLoginKey.GateId;

                    string token = TimeHelper.ServerNow().ToString() + RandomGenerator.RandomNumber(int.MinValue, int.MaxValue).ToString();
                    session.DomainScene().GetComponent<TokenComponent>().Remove(account.Id);
                    session.DomainScene().GetComponent<TokenComponent>().Add(account.Id, token);

                    response.AccountId = account.Id;
                    response.Token = token;
                    account?.Dispose();
                }
            }
        }
    }
}