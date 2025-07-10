using ApiServer.Handlers.Models;
using DbContext.Common.Models;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.SubContexts;
using Microsoft.Extensions.Logging.Abstractions;
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
    
    public async Task<bool> AddInventoryItemAsync(int itemIndex, int amount)
    {
        var dbContext = GetDbContext(false);
        var dbInfo = await _GetInventoryOneItem(itemIndex) ?? InventoryDbResult.Create(itemIndex, 0);
        dbInfo.item_amount += amount;

        return await dbContext.InsertInventoryItem(AccountId, [dbInfo]);
    }

    private async Task<InventoryDbResult> _GetInventoryOneItem(int itemIndex)
    {
        var list = await GetInventoryListAsync();
        if (list == null || list.Count == 0)
            return null;
        
        return list.FirstOrDefault(x => x.item_index == itemIndex);
    }

}