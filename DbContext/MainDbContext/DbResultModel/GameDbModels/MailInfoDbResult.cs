using System.Data;
using DbContext.Common.Models;

namespace DbContext.MainDbContext.DbResultModel.GameDbModels;

public class MailInfoDbResult : IHasCustomTableData
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
    

    public void SetCustomTableData(DataRow row)
    {
        row[nameof(mail_uid)] = mail_uid;
        row[nameof(message_content)] = message_content;
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
}