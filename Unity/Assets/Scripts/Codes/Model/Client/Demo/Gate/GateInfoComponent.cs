namespace ET.Client
{
    [ComponentOf]
    public class GateInfoComponent: Entity, IAwake, IDestroy
    {
        public string Adderss { get; set; }
        public long Key { get; set; }
        public long GateId { get; set; }
        public long PlayerId { get; set; }
    }
}
