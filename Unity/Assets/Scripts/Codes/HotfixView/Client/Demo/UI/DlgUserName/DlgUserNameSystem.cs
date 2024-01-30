/*
 *FileName:      DlgUserNameSystem.cs
 *Author:        ReidChen
 *Date:          2024/01/30 16:45:13
 *UnityVersion:  2021.3.29f1c1
 *Description:
*/
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[FriendOf(typeof(DlgUserName))]
	public static  class DlgUserNameSystem
	{

		public static void RegisterUIEvent(this DlgUserName self)
		{
		 
		}

		public static void ShowWindow(this DlgUserName self, Entity contextData = null)
		{
			self.View.E_UserNameText.text = "欢迎 ： " + self.ClientScene().GetComponent<AccountInfoComponent>().AccountName;
		}

		 

	}
}
