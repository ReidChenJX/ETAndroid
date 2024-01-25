
using UnityEngine;
using UnityEngine.UI;
namespace ET.Client
{
	[ObjectSystem]
	public class DlgServerViewComponentAwakeSystem : AwakeSystem<DlgServerViewComponent> 
	{
		protected override void Awake(DlgServerViewComponent self)
		{
			self.uiTransform = self.Parent.GetParent<UIBaseWindow>().uiTransform;
		}
	}


	[ObjectSystem]
	public class DlgServerViewComponentDestroySystem : DestroySystem<DlgServerViewComponent> 
	{
		protected override void Destroy(DlgServerViewComponent self)
		{
			self.DestroyWidget();
		}
	}
}
