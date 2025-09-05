using NetworkProtocols.Shared.Enums;

namespace NetworkProtocols.WebApi.ToClientModels;

public class RewardInfo
{
    public RewardTypeEnums RewardType { get; set; }
    public int Index { get; set; }
    public int Amount { get; set; }
}