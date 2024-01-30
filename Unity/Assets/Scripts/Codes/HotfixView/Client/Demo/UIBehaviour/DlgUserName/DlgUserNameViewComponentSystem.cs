/*
 *FileName:      DlgUserNameViewComponentSystem.cs
 *Author:        ReidChen
 *Date:          2024/01/30 16:45:13
 *UnityVersion:  2021.3.29f1c1
 *Description:
*/

using UnityEngine;
using UnityEngine.UI;
namespace ET.Client
{
	[ObjectSystem]
	public class DlgUserNameViewComponentAwakeSystem : AwakeSystem<DlgUserNameViewComponent> 
	{
		protected override void Awake(DlgUserNameViewComponent self)
		{
			self.uiTransform = self.Parent.GetParent<UIBaseWindow>().uiTransform;
		}
	}


	[ObjectSystem]
	public class DlgUserNameViewComponentDestroySystem : DestroySystem<DlgUserNameViewComponent> 
	{
		protected override void Destroy(DlgUserNameViewComponent self)
		{
			self.DestroyWidget();
		}
	}
}
