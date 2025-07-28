namespace NetworkProtocols.WebApi.ToClientModels;

public class MailInfo
{
    public long MailUid { get; set; }
    public bool IsRead { get; set; }
    public bool IsReceivedReward { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime ExpireDate { get; set; }
    
    public string Message { get; set; }
    public List<MailRewardInfo> RewardInfo { get; set; }
}