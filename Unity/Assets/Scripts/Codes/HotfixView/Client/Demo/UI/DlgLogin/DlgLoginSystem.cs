using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [FriendOf(typeof(DlgLogin))]
    public static  class DlgLoginSystem
    {

        public static void RegisterUIEvent(this DlgLogin self)
        {
            self.View.E_LoginButton.AddListenerAsync(() => { return self.OnLoginClickHandler(); });
        }

        public static void ShowWindow(this DlgLogin self, Entity contextData = null)
        {

			
        }

        public static async ETTask OnLoginClickHandler(this DlgLogin self)
        {
            try
            {
                string accountText = self.View.E_AccountInputField.text.Trim();
                string passWdText = self.View.E_PasswordInputField.text.Trim();
                int errorCode =  await LoginHelper.LoginAccount(
                    self.ClientScene(), 
                    accountText, 
                    passWdText);

                if(errorCode != ErrorCode.ERR_Success)
                {
                    // 登录错误
                    Log.Error(errorCode.ToString());
                    return;
                }
                
                // 获取区服信息
                errorCode = await LoginHelper.GetServerInfos(self.ClientScene());
                // 手动设置选择区服
                self.DomainScene().GetComponent<ServerInfoComponent>().CurrentServerId = 1;
                // 手动创建角色信息 角色名 = 登录名
                errorCode = await LoginHelper.CreateRole(self.ClientScene(), accountText);
                
                // EnterMap 
                await EnterMapHelper.EnterMapAsync(self.ClientScene());
                
                // TODO 显示登录后的UI界面
                self.DomainScene().GetComponent<UIComponent>().HideWindow(WindowID.WindowID_Login);
                // self.DomainScene().GetComponent<UIComponent>().ShowWindow(WindowID.WindowID_Server);
                
            }
            catch(Exception e)
            {
                Log.Error(e.ToString());
                return;
            }
        }
        
        
        
        
    }
}