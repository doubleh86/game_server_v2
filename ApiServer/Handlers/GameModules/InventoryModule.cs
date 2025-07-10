using ApiServer.Handlers.Models;
using DbContext.Common.Models;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.SubContexts;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.Handlers.GameModules;

public sealed class InventoryModule : BaseModule<InventoryDbContext>, IGameModule
{
    public long AccountId { get; set; }
    private List<InventoryDbResult> _inventoryList;
    public InventoryModule(long accountId, SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo) : base(masterDbInfo, slaveDbInfo)
    {
        AccountId = accountId;
    }
    
    public async Task<List<InventoryDbResult>> GetInventoryListAsync()
    {
        if (_inventoryList != null)
            return _inventoryList;
        
        var dbContext = GetDbContext(true);
        _inventoryList = await dbContext.GetInventoryDbResult(AccountId);
        
        return _inventoryList;
    }

    
}