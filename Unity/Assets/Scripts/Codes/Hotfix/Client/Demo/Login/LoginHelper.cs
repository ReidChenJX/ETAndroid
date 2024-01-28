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
                    routerAddressComponent =
                            clientScene.AddComponent<RouterAddressComponent, string, int>(ConstValue.RouterHttpHost, ConstValue.RouterHttpPort);
                    await routerAddressComponent.Init();

                    clientScene.AddComponent<NetClientComponent, AddressFamily>(routerAddressComponent.RouterManagerIPAddress.AddressFamily);
                }

                IPEndPoint realmAddress = routerAddressComponent.GetRealmAddress(account);

                R2C_Login r2CLogin = null;
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
                clientScene.RemoveComponent<RouterAddressComponent>();
                clientScene.RemoveComponent<NetClientComponent>();

                clientScene.RemoveComponent<SessionComponent>();
                // ET 7.2 增加路由session，由路由层获取登录服务器地址，由此可见登录服务器可多个
                // 创建一个ETModel层的Session
                // 获取路由跟realmDispatcher地址
                RouterAddressComponent routerAddressComponent = clientScene.GetComponent<RouterAddressComponent>();
                if (routerAddressComponent == null)
                {
                    routerAddressComponent =
                            clientScene.AddComponent<RouterAddressComponent, string, int>(ConstValue.RouterHttpHost, ConstValue.RouterHttpPort);
                    await routerAddressComponent.Init();

                    clientScene.AddComponent<NetClientComponent, AddressFamily>(routerAddressComponent.RouterManagerIPAddress.AddressFamily);
                }

                // 登录验证服务器地址
                // 验证服务器类型由 GetRealmAddress 中使用的IP地址决定
                IPEndPoint accountAddress = routerAddressComponent.GetRealmAddress(account);

                // 登录验证
                // session 为登录服务器
                Session session = await RouterHelper.CreateRouterSession(clientScene, accountAddress);

                Log.Debug("开始调用C2A_LoginAccount!");
                a2cLoginAccount = (A2C_LoginAccount)await session.Call(new C2A_LoginAccount() { AccountName = account, PassWord = password });

                if (a2cLoginAccount.Error != ErrorCode.ERR_Success)
                {
                    return a2cLoginAccount.Error;
                }

                clientScene.AddComponent<SessionComponent>().Session = session;
                clientScene.GetComponent<AccountInfoComponent>().Token = a2cLoginAccount.Token;
                clientScene.GetComponent<AccountInfoComponent>().AccountId = a2cLoginAccount.AccountId;
                clientScene.GetComponent<AccountInfoComponent>().AccountName = account;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            // 登录返回
            if (a2cLoginAccount.Error != ErrorCode.ERR_Success)
            {
                return a2cLoginAccount.Error;
            }

            return ErrorCode.ERR_Success;
        }

        // 获取区服服务器列表
        public static async ETTask<int> GetServerInfos(Scene clientScene)
        {
            A2C_GetServerInfo a2cGetServerInfo = null;

            // clientScene 为 Account
            try
            {
                a2cGetServerInfo = (A2C_GetServerInfo)await clientScene.GetComponent<SessionComponent>().Session.Call(new C2A_GetServerInfo()
                {
                    AccountId = clientScene.GetComponent<AccountInfoComponent>().AccountId,
                    Token = clientScene.GetComponent<AccountInfoComponent>().Token
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            // 区服服务器信息获取返回
            if (a2cGetServerInfo.Error != ErrorCode.ERR_Success)
            {
                return a2cGetServerInfo.Error;
            }

            // clinet ServerInfoComponent 记录 a2cGetServerInfo 中的 ServerInfoProto
            foreach (var serverPorto in a2cGetServerInfo.ServerInfoList)
            {
                ServerInfo serverInfo = clientScene.GetComponent<ServerInfoComponent>().AddChild<ServerInfo>();
                serverInfo.FromMessage(serverPorto);
                clientScene.GetComponent<ServerInfoComponent>().Add(serverInfo);
            }

            await ETTask.CompletedTask;
            return ErrorCode.ERR_Success;
        }

        // Role 创建与登录获取，目前单一场景，单一角色
        public static async ETTask<int> CreateRole(Scene clientScene, string name)
        {
            A2C_CreateRole a2CCreateRole = null;

            try
            {
                a2CCreateRole = (A2C_CreateRole)await clientScene.GetComponent<SessionComponent>().Session.Call(new C2A_CreateRole()
                {
                    AccountId = clientScene.GetComponent<AccountInfoComponent>().AccountId,
                    Token = clientScene.GetComponent<AccountInfoComponent>().Token,
                    Name = name,
                    ServerId = clientScene.GetComponent<ServerInfoComponent>().CurrentServerId
                });
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }

            if (a2CCreateRole.Error != ErrorCode.ERR_Success)
            {
                Log.Error(a2CCreateRole.Error.ToString());
                return a2CCreateRole.Error;
            }

            // 记录 角色选择信息
            RoleInfo newRoleInfo = clientScene.GetComponent<RoleInfoComponent>().AddChild<RoleInfo>();
            newRoleInfo.FromMessage(a2CCreateRole.RoleInfo);

            clientScene.GetComponent<RoleInfoComponent>().RoleInfos.Add(newRoleInfo);
            clientScene.GetComponent<RoleInfoComponent>().CurrentRoleId = newRoleInfo.Id;
            Log.Debug($"当前自动创建并获取的角色{newRoleInfo.Id}");

            return a2CCreateRole.Error;
        }

        public static async ETTask<int> GatGate(Scene clientScene)
        {
            // 获取Gate 网关地址，并登录
            G2C_LoginGate g2CLoginGate = null;
            try
            {
                // A2C_GetGate 客户端向登录服务器申请网关
                A2C_GetGate a2CGetGate = (A2C_GetGate)await clientScene.GetComponent<SessionComponent>().Session.Call(new C2A_GetGate()
                {
                    AccountId = clientScene.GetComponent<AccountInfoComponent>().AccountId,
                    Token = clientScene.GetComponent<AccountInfoComponent>().Token,
                    AccountName = clientScene.GetComponent<AccountInfoComponent>().AccountName
                });

                // gate Session
                Session gateSession = await RouterHelper.CreateRouterSession(clientScene, NetworkHelper.ToIPEndPoint(a2CGetGate.Address));
                // gate Session 挂载至客户端，后续消息由gate 进行转发
                clientScene.GetComponent<SessionComponent>().Session.Dispose();
                clientScene.GetComponent<SessionComponent>().Session = gateSession;

                // client add GateInfoComponent
                g2CLoginGate = (G2C_LoginGate)await gateSession.Call(
                    new C2G_LoginGate()
                    {
                        Key = a2CGetGate.Key, 
                        GateId = a2CGetGate.GateId,
                        AccountId = clientScene.GetComponent<AccountInfoComponent>().AccountId
                    });

                clientScene.GetComponent<GateInfoComponent>().Adderss = a2CGetGate.Address;
                clientScene.GetComponent<GateInfoComponent>().GateId = a2CGetGate.GateId;
                clientScene.GetComponent<GateInfoComponent>().Key = a2CGetGate.Key;
                clientScene.GetComponent<GateInfoComponent>().PlayerId = g2CLoginGate.PlayerId;

                Log.Debug("登陆gate成功!");
            }
            catch (Exception e)
            {
                clientScene.GetComponent<SessionComponent>().Session.Dispose();
                Log.Debug(e.ToString());
            }

            if (g2CLoginGate.Error != ErrorCode.ERR_Success)
            {
                clientScene.GetComponent<SessionComponent>().Session.Dispose();
                return  g2CLoginGate.Error;
            }
            

            return ErrorCode.ERR_Success;
        }
    }
}