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

            // ��¼��֤
            if (string.IsNullOrEmpty(request.AccountName)|| string.IsNullOrEmpty(request.PassWord))
            {
                response.Error = ErrorCode.ERR_LoginInfoError;

                // * δ������Ϣ��ע��Session �Ƿ�ᵼ�� response �޷����أ�
                //session?.Dispose();
                return;
            }

            if (!Regex.IsMatch(request.PassWord.Trim(), @"^(?=.*[0-9].*)(?=.*[A-Z].*)(?=.*[a-z].*).{6,15}$"))
            {
                response.Error = ErrorCode.ERR_LoginInfoError;

                return;
            }

            // ���ݿ���֤
            var accountInfoList = await DBManagerComponent.Instance.GetZoneDB(session.DomainZone()).Query<Account>(d=>d.AccountName.Equals(request.AccountName.Trim()));

            Account account = null;

            if(accountInfoList.Count > 0)
            {

            }
            else
            {
                // ���ݿ������ݣ��Զ�������
                // Session Ҳ��������½���account ��Ҫ���������£����ڼ�¼������˻���Ϣ
                account = session.AddChild<Account>();



            }


            await ETTask.CompletedTask;
        }
    }
}