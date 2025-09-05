using NetworkProtocols.WebApi.Commands.User;
using NetworkProtocols.WebApi.ToClientModels;

namespace NetworkProtocols.WebApi.Commands;

public class RequestBase
{
    public string Url { get; }
    public string Token { get; set; }
    public long AccountId { get; set; }
    public byte Sequence { get; set; }
    public byte SubSequence { get; set; }

    protected RequestBase()
    {
        
    }

    protected RequestBase(string url)
    {
        Url = url;
    }
}

public class ResponseBase
{
    public int ResultCode { get; set; }
    
    public long ServerTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public string DebugMessage { get; set; }
}


public class RefreshResponse : ResponseBase
{
    public GameUserInfo GameUserInfo { get; set; }
    public List<InventoryItemInfo> ChangeInventoryItems { get; set; }
    public List<AssetInfo> ChangeAssets { get; set; }
    protected RefreshResponse()
    {
        
    }
}

