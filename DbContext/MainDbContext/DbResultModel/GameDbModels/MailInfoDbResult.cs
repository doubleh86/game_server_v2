using System.Data;
using System.Text.Json;
using DbContext.Common.Models;
using NetworkProtocols.WebApi.ToClientModels;

namespace DbContext.MainDbContext.DbResultModel.GameDbModels;

public class MailInfoDbResult : IHasCustomTableData, IHasClientModel<MailInfo>
{
    public long mail_uid { get; set; }
    public string message_content { get; set; }
    public string reward_content { get; set; }
    public int is_read { get; set; }
    public int is_reward_received { get; set; }
    public DateTime update_date { get; set; }
    public DateTime create_date { get; set; }
    public DateTime expiry_date { get; set; }
    
    public static string GetCustomTableName() => "dbo.TVP_MailInfo";

    private List<RewardInfo> _rewardInfo = null;

    public List<RewardInfo> GetMailRewardInfoList()
    {
        if (string.IsNullOrWhiteSpace(reward_content) == true)
        {
            _rewardInfo = [];
            return _rewardInfo;
        }

        try
        {
            _rewardInfo = JsonSerializer.Deserialize<List<RewardInfo>>(reward_content); 
            return _rewardInfo;
        }
        catch (Exception e)
        {
            _rewardInfo = [];
            return _rewardInfo;
        }
    }
    public void AddMailReward(List<RewardInfo> mailRewardInfo)
    {
        var rewardList = GetMailRewardInfoList();
        rewardList.AddRange(mailRewardInfo);
        
        reward_content = JsonSerializer.Serialize(rewardList);
    }

    public void SetCustomTableData(DataRow row)
    {
        row[nameof(mail_uid)] = mail_uid;
        row[nameof(message_content)] = string.IsNullOrEmpty(message_content) == true ? "" : message_content;
        row[nameof(reward_content)] = reward_content;
        row[nameof(is_read)] = is_read;
        row[nameof(is_reward_received)] = is_reward_received;
        row[nameof(expiry_date)] = expiry_date;
    }

    public static DataTable GetDataTable()
    {
        var result = new DataTable();
        
        result.Columns.Add(nameof(mail_uid), typeof(long));
        result.Columns.Add(nameof(message_content), typeof(string));
        result.Columns.Add(nameof(reward_content), typeof(string));
        result.Columns.Add(nameof(is_read), typeof(int));
        result.Columns.Add(nameof(is_reward_received), typeof(int));
        result.Columns.Add(nameof(expiry_date), typeof(DateTime));
        
        return result;
    }

    public MailInfo ToClient()
    {
        return new MailInfo()
        {
            MailUid = mail_uid,
            IsRead = is_read == 1,
            IsReceivedReward = is_reward_received == 1,
            RewardInfo = GetMailRewardInfoList(),
            Message = message_content,
            CreateDate = create_date,
            ExpireDate = expiry_date,
        };
    }
}