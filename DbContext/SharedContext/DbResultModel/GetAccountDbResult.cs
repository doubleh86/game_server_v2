using ServerFramework.SqlServerServices.Models;

namespace DbContext.SharedContext.DbResultModel;

public class GetAccountDbResult
{
    public long AccountId { get; set; }
    public string LoginId { get; set; }
    public int AccountType { get; set; }
    
    public int GameDbUID { get; set; }
    public string DbName { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public string DbLoginId { get; set; }
    public string DbPassword { get; set; }
    public DateTime UpdateDate { get; set; }
    public DateTime CreateDate { get; set; }

    public SqlServerDbInfo GetMainDbInfo()
    {
        return new SqlServerDbInfo()
        {
            Ip = Host,
            Port = Port,
            DatabaseName = DbName,
            UserId = DbLoginId,
            Password = DbPassword,
        };
    }
}