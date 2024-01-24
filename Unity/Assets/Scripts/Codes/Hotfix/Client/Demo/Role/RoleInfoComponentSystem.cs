using System;

namespace ET.Client
{
    public class RoleInfoComponentDestroySystem: DestroySystem<RoleInfoComponent>
    {
        protected override void Destroy(RoleInfoComponent self)
        {
            foreach (var roleInfo in self.RoleInfos)
            {
                roleInfo?.Dispose();
            }
            self.RoleInfos.Clear();
            self.CurrentRoleId = 0;
        }
    }
    
    
    
    
    
    [FriendOf(typeof(RoleInfoComponent))]
    public static class RoleInfoComponentSystem
    {
        
    }
}

