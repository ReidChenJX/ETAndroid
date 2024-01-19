using System;
using System.Net;
using System.Net.Sockets;

namespace ET.Client
{
    public static class LoginHelper
    {
        public static async ETTask Login(Scene clientScene, string account, string password)
        {
            try
            {
                // 创建一个ETModel层的Session
                clientScene.RemoveComponent<RouterAddressComponent>();
                // 获取路由跟realmDispatcher地址
                RouterAddressComponent routerAddressComponent = clientScene.GetComponent<RouterAddressComponent>();
                if (routerAddressComponent == null)
                {
                    routerAddressComponent = clientScene.AddComponent<RouterAddressComponent, string, int>(ConstValue.RouterHttpHost, ConstValue.RouterHttpPort);
                    await routerAddressComponent.Init();

                    clientScene.AddComponent<NetClientComponent, AddressFamily>(routerAddressComponent.RouterManagerIPAddress.AddressFamily);
                }
                IPEndPoint realmAddress = routerAddressComponent.GetRealmAddress(account);

                R2C_Login r2CLogin;
                using (Session session = await RouterHelper.CreateRouterSession(clientScene, realmAddress))
                {
                    r2CLogin = (R2C_Login)await session.Call(new C2R_Login() { Account = account, Password = password });
                }

                // 创建一个gate Session,并且保存到SessionComponent中
                Session gateSession = await RouterHelper.CreateRouterSession(clientScene, NetworkHelper.ToIPEndPoint(r2CLogin.Address));
                clientScene.AddComponent<SessionComponent>().Session = gateSession;

                G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await gateSession.Call(
                    new C2G_LoginGate() { Key = r2CLogin.Key, GateId = r2CLogin.GateId });

                Log.Debug("登陆gate成功!");

                await EventSystem.Instance.PublishAsync(clientScene, new EventType.LoginFinish());
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            
        } 

        public static async ETTask<int> LoginAccount(Scene clientScene, string account, string password)
        {
            A2C_LoginAccount a2cLoginAccount = null;

            try
            {
                // ET 7.2 增加路由session，由路由层获取登录服务器地址，由此可见登录服务器可多个
                // 创建一个ETModel层的Session
                clientScene.RemoveComponent<RouterAddressComponent>();
                // 获取路由跟realmDispatcher地址
                RouterAddressComponent routerAddressComponent = clientScene.GetComponent<RouterAddressComponent>();
                if (routerAddressComponent == null)
                {
                    routerAddressComponent = clientScene.AddComponent<RouterAddressComponent, string, int>(ConstValue.RouterHttpHost, ConstValue.RouterHttpPort);
                    await routerAddressComponent.Init();

                    clientScene.AddComponent<NetClientComponent, AddressFamily>(routerAddressComponent.RouterManagerIPAddress.AddressFamily);
                }
                // 登录验证服务器地址
                IPEndPoint realmAddress = routerAddressComponent.GetRealmAddress(account);

                // 登录验证
                // session 为路由服务器，由路由服务器转发给登录服务器，并返回网关(gate)服务器地址
                using (Session session = await RouterHelper.CreateRouterSession(clientScene, realmAddress))
                {
                    Log.Debug("开始调用C2A_LoginAccount!");
                    Log.Debug(session.DomainScene().SceneType.ToString());
                    a2cLoginAccount = (A2C_LoginAccount)await session.Call(new C2A_LoginAccount() { AccountName = account, PassWord = password });
                }

                // gate Session
                Session gateSession = await RouterHelper.CreateRouterSession(clientScene, NetworkHelper.ToIPEndPoint(a2cLoginAccount.Address));
                // gate Session 挂载至客户端，后续消息由gate 进行转发
                clientScene.AddComponent<SessionComponent>().Session = gateSession;

                clientScene.GetComponent<AccountInfoComponent>().Token = a2cLoginAccount.Token;
                clientScene.GetComponent<AccountInfoComponent>().AccountId = a2cLoginAccount.AccountId;

                G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await gateSession.Call(
                    new C2G_LoginGate() { Key = a2cLoginAccount.Key, GateId = a2cLoginAccount.GateId });

                Log.Debug("登陆gate成功!");
            }
            catch (Exception ex) 
            {
                Log.Error(ex);

            }
            


            // 登录返回
            if(a2cLoginAccount.Error != ErrorCode.ERR_Success)
            {
                return a2cLoginAccount.Error;
            }
            return ErrorCode.ERR_Success;
            
        }
    }
}