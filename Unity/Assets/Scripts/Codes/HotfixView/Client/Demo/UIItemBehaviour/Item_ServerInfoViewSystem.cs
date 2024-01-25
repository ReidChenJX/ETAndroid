
using UnityEngine;
using UnityEngine.UI;
namespace ET.Client
{
	[ObjectSystem]
	public class Scroll_Item_ServerInfoDestroySystem : DestroySystem<Scroll_Item_ServerInfo> 
	{
		protected override void Destroy( Scroll_Item_ServerInfo self )
		{
			self.DestroyWidget();
		}
	}
}
