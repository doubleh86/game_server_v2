using DbContext.SharedContext.MySqlContext;
using DbContext.SharedContext.SqlServerContext;
using ServerFramework.SqlServerServices.Models;

namespace DbContext.SharedContext;

public static class SharedDbContextWrapper
{
    private static bool _isMySql = false;
    public static ISharedDbContext Create()
    {
        return _isMySql == true ? MySqlSharedDbContext.Create() : SharedDbContext.Create();
    }
    
    public static void SetDefaultServerInfo(SqlServerDbInfo settings)
    {
        _isMySql = settings.IsMySql;
        if (_isMySql == true)
        {
            MySqlSharedDbContext.SetDefaultServerInfo(settings);
            return;
        }
        
        SharedDbContext.SetDefaultServerInfo(settings);
    }
}