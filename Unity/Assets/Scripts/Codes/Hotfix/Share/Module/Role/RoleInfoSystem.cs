namespace ET
{
    // 角色信息 proto 客户端与服务器 通信
    [FriendOf(typeof(RoleInfo))]
    public static class RoleInfoSystem
    {
        public static void FromMessage(this RoleInfo self, RoleInfoProto roleInfoProto)
        {
            self.Id = roleInfoProto.Id;
            self.Name = roleInfoProto.Name;
            self.State = roleInfoProto.State;
            self.AccountId = roleInfoProto.AccountId;
            self.CreateTime = roleInfoProto.CreateTime;
            self.LastLoginTime = roleInfoProto.LastLoginTime;
            self.ServerId = roleInfoProto.ServerId;
        }


        public static RoleInfoProto ToMessage(this RoleInfo self)
        {
            return new RoleInfoProto()
            {
                Id = (int)self.Id,
                Name = self.Name,
                State = self.State,
                AccountId = self.AccountId,
                CreateTime = self.CreateTime,
                LastLoginTime = self.LastLoginTime,
                ServerId = self.ServerId
            };
        }
    }
}


