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
                Log.Error($"�����Scene���󣬵�¼����ǰSceneΪ��{session.DomainScene().SceneType}");
                session?.Dispose();
                return;
            }
            // ����ͨ�����Ƴ���ǰSession �ϵ��Զ����ٶ�ʱ��
            session.RemoveComponent<SessionAcceptTimeoutComponent>();

            // ������һ����֤
            if(session.GetComponent<SessionLockComponent>() != null)
            {
                // session �ڵ�һ������ʱ����SessionLockComponent ��������������жϴ�ʱ�ѹ��أ����Զ�ȡ��
                response.Error = ErrorCode.ERR_RequestRespeated;
                session.Disconnect().Coroutine();
                return;
            }

            // ��¼��֤
            if (string.IsNullOrEmpty(request.AccountName)|| string.IsNullOrEmpty(request.PassWord))
            {
                response.Error = ErrorCode.ERR_LoginInfoError;

                // * δ������Ϣ��ע��Session �Ƿ�ᵼ�� response �޷����أ�
                session.Disconnect().Coroutine();
                return;
            }

            if (!Regex.IsMatch(request.PassWord.Trim(), @"^(?=.*[0-9].*)(?=.*[A-Z].*)(?=.*[a-z].*).{6,15}$"))
            {
                response.Error = ErrorCode.ERR_PasswordFormError;
                session.Disconnect().Coroutine();
                return;
            }

            // ����ɹ�����session ��������
            using (session.AddComponent<SessionLockComponent>())
            {
                // ��ֹͬʱ��¼ע�ᣬ�����ݿ���������
                using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.LoginAccount, request.AccountName.Trim().GetHashCode()))
                {
                    // ���ݿ���֤
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
                        // ���ݿ������ݣ��Զ�������
                        // Session Ҳ��������½���account ��Ҫ���������£����ڼ�¼������˻���Ϣ
                        account = session.AddChild<Account>();
                        account.AccountName = request.AccountName.Trim();
                        account.PassWord = request.PassWord.Trim();
                        account.CreateTime = TimeHelper.ServerNow();
                        account.AccountType = (int)AccountType.General;

                        // �������
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