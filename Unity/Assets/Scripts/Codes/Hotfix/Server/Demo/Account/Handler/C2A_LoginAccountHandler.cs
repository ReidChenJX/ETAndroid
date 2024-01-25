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
            Log.Debug("C2A_LoginAccountHandler: ��Ϣ����");
            if (session.DomainScene().SceneType != SceneType.Account)
            {
                Log.Error($"�����Scene���󣬵�¼����ǰSceneΪ��{session.DomainScene().SceneType}");
                session?.Dispose();
                return;
            }

            // ����ͨ�����Ƴ���ǰSession �ϵ��Զ����ٶ�ʱ��
            session.RemoveComponent<SessionAcceptTimeoutComponent>();

            // ������һ����֤
            if (session.GetComponent<SessionLockComponent>() != null)
            {
                // session �ڵ�һ������ʱ����SessionLockComponent ��������������жϴ�ʱ�ѹ��أ����Զ�ȡ��
                response.Error = ErrorCode.ERR_RequestRespeated;
                session.Disconnect().Coroutine();
                return;
            }

            // ��¼��֤
            if (string.IsNullOrEmpty(request.AccountName) || string.IsNullOrEmpty(request.PassWord))
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


            Log.Debug("C2A_LoginAccountHandler: ErrorCode.ERR_Success ������֤����");
            // ����ɹ�����session ��������
            using (session.AddComponent<SessionLockComponent>())
            {
                // ��ֹͬʱ��¼ע�ᣬ�����ݿ���������
                using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.LoginAccount, request.AccountName.Trim().GetHashCode()))
                {
                    // ��Password MD5����
                    string passWordMD5 = MD5Helper.StringMD5(request.PassWord.Trim());
                    // ���ݿ���֤
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
                        Log.Debug("���ݿ������ݣ��Զ�������");
                        // Session Ҳ��������½���account ��Ҫ���������£����ڼ�¼������˻���Ϣ
                        account = session.AddChild<Account>();
                        account.AccountName = request.AccountName.Trim();
                        account.PassWord = passWordMD5;
                        account.CreateTime = TimeHelper.ServerNow();
                        account.AccountType = (int)AccountType.General;

                        // �������
                        await DBManagerComponent.Instance.GetZoneDB(session.DomainZone()).Save<Account>(account);

                        Log.Debug("�������ɹ�");
                    }

                    Log.Debug("ErrorCode.ERR_Success: ��֤�ɹ����жϸ��˺��Ƿ��Ѿ���¼");

                    // ��֤�ɹ����жϸ��˺��Ƿ������˻����ķ�����
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
                    
                    // �жϸ��˺��Ƿ����ڵ�¼������
                    long accountSessionInstanceId = session.DomainScene().GetComponent<AccountSessionsComponent>().Get(account.Id);
                    Session otherSession = Root.Instance.Get(accountSessionInstanceId) as Session;

                    // ���˺��ѵ�¼���ɷ�������ͻ��˷�������
                    if (otherSession != null)
                    {
                        otherSession.Send(new A2C_Disconnect() { Error = ErrorCode.ERR_ExtraAccount });
                        otherSession.Disconnect().Coroutine();
                    }

                    session.DomainScene().GetComponent<AccountSessionsComponent>().Add(account.Id, session.InstanceId);
                    // ��¼�������һ��ʱ����Զ��Ͽ�
                    session.AddComponent<AccountCheckOutTimeComponent, long>(account.Id);

                    // ��֤ͨ������ȡGate��Ϣ   Gate ������ʱ�����أ�Role �����ɹ�����ѡ�� Gate
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