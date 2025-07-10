using ApiServer.Handlers.Models;
using DbContext.Common.Models;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.SubContexts;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.Handlers.GameModules;

public sealed class InventoryModule : BaseModule<InventoryDbContext>, IGameModule
{
    private List<InventoryDbResult> _inventoryList;
    public InventoryModule(SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo) : base(masterDbInfo, slaveDbInfo)
    {
        
    }
    
    public async Task<List<InventoryDbResult>> GetInventoryListAsync(long accountId)
    {
        if (_inventoryList != null)
            return _inventoryList;
        
        var dbContext = GetDbContext(true);
        _inventoryList = await dbContext.GetInventoryDbResult(accountId);
        
        return _inventoryList;
    }

}