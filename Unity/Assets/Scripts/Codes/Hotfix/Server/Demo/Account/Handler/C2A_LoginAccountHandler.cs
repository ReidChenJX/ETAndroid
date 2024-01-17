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

            // 登录验证
            if (string.IsNullOrEmpty(request.AccountName)|| string.IsNullOrEmpty(request.PassWord))
            {
                response.Error = ErrorCode.ERR_LoginInfoError;

                // * 未返回消息先注销Session 是否会导致 response 无法返回？
                //session?.Dispose();
                return;
            }

            if (!Regex.IsMatch(request.PassWord.Trim(), @"^(?=.*[0-9].*)(?=.*[A-Z].*)(?=.*[a-z].*).{6,15}$"))
            {
                response.Error = ErrorCode.ERR_LoginInfoError;

                return;
            }

            // 数据库验证
            var accountInfoList = await DBManagerComponent.Instance.GetZoneDB(session.DomainZone()).Query<Account>(d=>d.AccountName.Equals(request.AccountName.Trim()));

            Account account = null;

            if(accountInfoList.Count > 0)
            {

            }
            else
            {
                // 数据库无数据，自动创建。
                // Session 也是组件，新建的account 需要挂载在其下，用于记录，入库账户信息
                account = session.AddChild<Account>();



            }


            await ETTask.CompletedTask;
        }
    }
}