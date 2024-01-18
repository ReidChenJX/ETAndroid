

namespace ET.Server
{
    // session Dispose 方法补充 一秒后再断开链接
    public static class DisconnectHelper
    {
        public static async ETTask Disconnect(this Session self)
        {
            if(self == null || self.IsDisposed)
            {
                return;
            }

            long instanceId = self.InstanceId;

            await TimerComponent.Instance.WaitAsync(1000);

            if (self.InstanceId == instanceId)
            {
                self.Dispose();
            }

        }
    }
}
