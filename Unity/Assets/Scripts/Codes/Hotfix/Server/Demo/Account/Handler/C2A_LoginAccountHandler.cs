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
            if (session.DomainScene().SceneType != SceneType.Account)
            {
                Log.Error($"请求的Scene错误，登录请求当前Scene为：{session.DomainScene().SceneType}");
                session?.Dispose();
                return;
            }
            // 链接通过，移出当前Session 上的自动销毁定时器
            session.RemoveComponent<SessionAcceptTimeoutComponent>();

            // 锁定第一次验证
            if(session.GetComponent<SessionLockComponent>() != null)
            {
                // session 在第一次请求时挂载SessionLockComponent 组件，后续请求判断此时已挂载，则自动取消
                response.Error = ErrorCode.ERR_RequestRespeated;
                session.Disconnect().Coroutine();
                return;
            }

            // 登录验证
            if (string.IsNullOrEmpty(request.AccountName)|| string.IsNullOrEmpty(request.PassWord))
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

            // 请求成功，对session 进行锁定
            using (session.AddComponent<SessionLockComponent>())
            {
                // 防止同时登录注册，对数据库请求锁定
                using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.LoginAccount, request.AccountName.Trim().GetHashCode()))
                {
                    // 数据库验证
                    var accountInfoList = await DBManagerComponent.Instance.GetZoneDB(session.DomainZone()).Query<Account>(d => d.AccountName.Equals(request.AccountName.Trim()));

                    Account account = null;

                    if (accountInfoList != null || accountInfoList.Count > 0)
                    {
                        account = accountInfoList[0];
                        session.AddChild(account);

                        if (account.AccountType == (int)AccountType.BlackList)
                        {
                            response.Error = ErrorCode.ERR_AccountBlackListError;
                            session.Disconnect().Coroutine();
                            account?.Dispose();
                            return;
                        }
                        if (!account.PassWord.Equals(request.PassWord.Trim()))
                        {
                            response.Error = ErrorCode.ERR_LoginInfoError;
                            session.Disconnect().Coroutine();
                            account?.Dispose();
                            return;
                        }

                    }
                    else
                    {
                        // 数据库无数据，自动创建。
                        // Session 也是组件，新建的account 需要挂载在其下，用于记录，入库账户信息
                        account = session.AddChild<Account>();
                        account.AccountName = request.AccountName.Trim();
                        account.PassWord = request.PassWord.Trim();
                        account.CreateTime = TimeHelper.ServerNow();
                        account.AccountType = (int)AccountType.General;

                        // 数据入库
                        await DBManagerComponent.Instance.GetZoneDB(session.DomainZone()).Save<Account>(account);


                    }

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