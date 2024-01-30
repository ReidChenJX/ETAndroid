/*
 *FileName:      DlgUserNameViewComponent.cs
 *Author:        ReidChen
 *Date:          2024/01/30 16:45:13
 *UnityVersion:  2021.3.29f1c1
 *Description:
*/

using UnityEngine;
using UnityEngine.UI;
namespace ET.Client
{
	[ComponentOf(typeof(DlgUserName))]
	[EnableMethod]
	public  class DlgUserNameViewComponent : Entity,IAwake,IDestroy 
	{
		public UnityEngine.UI.Text E_UserNameText
     	{
     		get
     		{
     			if (this.uiTransform == null)
     			{
     				Log.Error("uiTransform is null.");
     				return null;
     			}
     			if( this.m_E_UserNameText == null )
     			{
		    		this.m_E_UserNameText = UIFindHelper.FindDeepChild<UnityEngine.UI.Text>(this.uiTransform.gameObject,"E_UserName");
     			}
     			return this.m_E_UserNameText;
     		}
     	}

		public void DestroyWidget()
		{
			this.m_E_UserNameText = null;
			this.uiTransform = null;
		}

		private UnityEngine.UI.Text m_E_UserNameText = null;
		public Transform uiTransform = null;
	}
}
