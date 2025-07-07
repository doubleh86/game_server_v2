using Microsoft.EntityFrameworkCore;
using ServerFramework.SqlServerServices.Models;

namespace ServerFramework.SqlServerServices;

/// <summary>
/// Entity Framework를 DB 접속 구현 시 사용되는 클래스
/// </summary>
public class SqlServerServiceBase : DbContext, ISqlServerContext
{
    private readonly SqlServerDbInfo _serverInfo;
    private DbContextOptionsBuilder _optionsBuilder;
    
    public string ModelAssembly => _serverInfo.modelAssembly;
    
    protected SqlServerServiceBase(SqlServerDbInfo settings)
    {
        _serverInfo = settings;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (_serverInfo == null)
            return; 
        
        _optionsBuilder = options;
        ConnectDatabase();
        
        base.OnConfiguring(options);
    }

    protected void UseLazyLoading(bool isUse)
    {
        ChangeTracker.LazyLoadingEnabled = isUse;
    }

    public void ConnectDatabase()
    {
        var connString = $"server={_serverInfo.Ip},{_serverInfo.Port};uid={_serverInfo.UserId};" +
                         $"pwd={_serverInfo.Password};database={_serverInfo.DatabaseName};Encrypt=false";

        _optionsBuilder.UseSqlServer(connString);
        
    }
    
    
}