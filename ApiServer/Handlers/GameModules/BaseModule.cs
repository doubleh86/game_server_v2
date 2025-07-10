using ApiServer.Handlers.Models;
using DbContext.Common.Models;
using DbContext.MainDbContext.SubContexts;
using ServerFramework.SqlServerServices.DapperUtils;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.Handlers.GameModules;

public static class SubDbContextFactory
{
    public static DapperServiceBase CreateSubDbContext(string key, SqlServerDbInfo connectionInfo)
    {
        return key switch
        {
            nameof(InventoryDbContext) => new InventoryDbContext(connectionInfo),
            _ => null,
        };
    }
}

public abstract class BaseModule<TDbContext> : IUseSlaveDbContext<TDbContext>, IDisposable where TDbContext : DapperServiceBase
{
    public TDbContext MasterDbInfo { get; set; }
    public TDbContext SlaveDbInfo { get; set; }

    protected BaseModule(SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo)
    {
        InitializedDbContexts(masterDbInfo, slaveDbInfo);
    }
    
    public void InitializedDbContexts(SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo)
    {
        MasterDbInfo = SubDbContextFactory.CreateSubDbContext(typeof(TDbContext).Name, masterDbInfo) as TDbContext;
        if (slaveDbInfo != null)
        {
            SlaveDbInfo = SubDbContextFactory.CreateSubDbContext(typeof(TDbContext).Name, masterDbInfo) as TDbContext;
        }
    }

    public TDbContext GetDbContext(bool isSlave)
    {
        if (isSlave == true && SlaveDbInfo != null)
            return SlaveDbInfo;
        
        return MasterDbInfo;
    }
    
    public void Dispose()
    {
        MasterDbInfo?.Dispose();
        SlaveDbInfo?.Dispose();
    }
}