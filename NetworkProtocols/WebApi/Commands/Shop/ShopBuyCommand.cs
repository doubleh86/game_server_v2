using NetworkProtocols.WebApi.Commands.User;
using NetworkProtocols.WebApi.ToClientModels;

namespace NetworkProtocols.WebApi.Commands.Shop;

public class ShopBuyCommand
{
    public class Request : RequestBase
    {
        public int ItemIndex { get; set; }
        public int Amount { get; set; }
        public Request() : base("/shop/shop-buy")
        {
            
        }
    }

    public class Response : ResponseBase
    {
        public List<InventoryItemInfo> Items { get; set; }
        public AssetInfo Asset { get; set; }
    }    
}