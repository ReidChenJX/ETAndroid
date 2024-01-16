using System;
using System.Runtime.InteropServices;

namespace ET.Server
{
    [MessageHandler(SceneType.Account)]
    public class C2A_LoginAccountHandler : AMRpcHandler<C2A_LoginAccount, A2C_LoginAccount>
    {
        protected override async ETTask Run(Session session, C2A_LoginAccount request, A2C_LoginAccount response)
        {



            await ETTask.CompletedTask;
        }
    }
}