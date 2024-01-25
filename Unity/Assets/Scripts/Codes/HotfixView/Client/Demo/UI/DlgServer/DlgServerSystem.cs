using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[FriendOf(typeof(DlgServer))]
	public static  class DlgServerSystem
	{

		public static void RegisterUIEvent(this DlgServer self)
		{
		 self.View.ELoopScrollList_ServerLoopVerticalScrollRect.AddItemRefreshListener((Transform transform, int i) =>
		 {
			 self.OnLoopListItemRefreshHandler(transform, i);
		 });
		 
		 
		}

		public static void ShowWindow(this DlgServer self, Entity contextData = null)
		{
			// 滑动列表内容显示
			int serverCount = self.DomainScene().GetComponent<ServerInfoComponent>().ServerInfoList.Count;
			
			self.AddUIScrollItems(ref self.ScrollItemServerInfosDict, serverCount);
			self.View.ELoopScrollList_ServerLoopVerticalScrollRect.SetVisible(true, serverCount);
			

		}

		public static void HideWindow(this DlgServer self)
		{
			self.RemoveUIScrollItems(ref self.ScrollItemServerInfosDict);
		}
		

		public static void OnLoopListItemRefreshHandler(this DlgServer self, Transform transform, int i)
		{
			// 滑动列表内 Item 处理
			Scroll_Item_ServerInfo itemServerInfo = self.ScrollItemServerInfosDict[i].BindTrans(transform);
			
			itemServerInfo.EText_serverText.text = self.DomainScene().GetComponent<ServerInfoComponent>().ServerInfoList[i].ServerName;
		}

		

	}
}
