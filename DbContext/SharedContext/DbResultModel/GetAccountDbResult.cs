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
    
    public string SlaveDbName { get; set; }
    public string SlaveHost { get; set; }
    public int SlavePort { get; set; }
    public string SlaveDbLoginId { get; set; }
    public string SlaveDbPassword { get; set; }
    
    public DateTime UpdateDate { get; set; }
    public DateTime CreateDate { get; set; }

    public SqlServerDbInfo GetMainDbInfo(bool isSlave = false)
    {
        if (isSlave == false)
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
        
        return new SqlServerDbInfo()
        {
            Ip = SlaveHost,
            Port = SlavePort,
            DatabaseName = SlaveDbName,
            UserId = SlaveDbLoginId,
            Password = SlaveDbPassword,
        };
        
    }
}