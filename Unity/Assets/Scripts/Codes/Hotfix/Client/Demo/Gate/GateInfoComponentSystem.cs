namespace ET.Client
{

    public class GateInfoComponentDestroySystem: DestroySystem<GateInfoComponent>
    {
        protected override void Destroy(GateInfoComponent self)
        {
            self.Adderss = string.Empty;
            self.Key = 0;
            self.GateId = 0;
            self.PlayerId = 0;
        }
    }
    
    
    
    
    [FriendOf(typeof(GateInfoComponent))]
    public static class GateInfoComponentSystem
    {
    
    }
}

