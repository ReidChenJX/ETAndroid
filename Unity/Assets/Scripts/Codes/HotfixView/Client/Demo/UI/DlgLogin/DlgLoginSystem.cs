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
                int errorCode =  await LoginHelper.LoginAccount(self.ClientScene(), self.View.E_AccountInputField.text, self.View.E_PasswordInputField.text);

                if(errorCode != ErrorCode.ERR_Success)
                {
                    // 登录错误
                    Log.Error(errorCode.ToString());
                    return;
                }
            }
            catch(Exception e)
            {
                Log.Error(e.ToString());
                return;
            }
            
        }
		
    }
}