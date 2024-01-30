/*
 *FileName:      DlgUserName.cs
 *Author:        ReidChen
 *Date:          2024/01/30 16:45:13
 *UnityVersion:  2021.3.29f1c1
 *Description:
*/
namespace ET.Client
{
	 [ComponentOf(typeof(UIBaseWindow))]
	public  class DlgUserName :Entity,IAwake,IUILogic
	{

		public DlgUserNameViewComponent View { get => this.GetComponent<DlgUserNameViewComponent>();} 

		 

	}
}
