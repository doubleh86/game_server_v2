using NetworkProtocols.WebApi.ToClientModels;

namespace NetworkProtocols.WebApi.Commands.Inventory;

public class ItemUseCommand
{
    public class Request : RequestBase
    {
        public int ItemIndex { get; set; }
        public int Quantity { get; set; }
        
        public Request() : base("/inventory/use-item")
        {
            
        }
    }

    public class Response : ResponseBase
    {
        public InventoryItemInfo UseItemInfo { get; set; }
    }
}