namespace ServerFramework.SqlServerServices.Models;

public class SqlServerDbInfo
{
    public string Ip { get; set; }
    public int Port { get; set; }
    public string DatabaseName { get; set; }
    public string UserId { get; set; }
    public string Password { get; set; }
    public int MaxPoolSize { get; set; } = 100;
    public string modelAssembly { get; set; } = "";
}

public class SqlServerDbSettings
{
    public Dictionary<string, SqlServerDbInfo> ConnectionInfos { get; set; }
}