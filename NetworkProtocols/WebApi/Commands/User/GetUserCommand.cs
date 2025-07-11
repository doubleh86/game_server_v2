using NetworkProtocols.WebApi.ToClientModels;

namespace NetworkProtocols.WebApi.Commands.User;

public class GetUserCommand
{
    public class Request : RequestBase
    {
        public Request() : base("/user/get-user-info")
        {
            
        }
    }

    public class Response : ResponseBase
    {
        public GameUserInfo UserInfo { get; set; }
        public List<AssetInfo> Assets { get; set; }
        public List<InventoryItemInfo> InventoryItems { get; set; }
    }
}