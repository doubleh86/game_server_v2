using ApiServer.GameService.Models;
using DbContext.Common.Models;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.SubContexts;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.GameService.GameModules;

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
        _inventoryList = await dbContext.GetInventoryDbResultAsync(AccountId);
        
        return _inventoryList;
    }
    
    public async Task<bool> AddInventoryItemAsync(int itemIndex, int amount)
    {
        var dbContext = GetDbContext();
        var dbInfo = await GetInventoryOneItemAsync(itemIndex) ?? InventoryDbResult.Create(itemIndex, 0);
        dbInfo.item_amount += amount;

        return await dbContext.InsertInventoryItemAsync(AccountId, [dbInfo]);
    }

    public async Task BuyInventoryItemAsync(InventoryDbResult itemInfo, AssetDbResult assetInfo)
    {
        var dbContext = GetDbContext();
        await dbContext.ShopBuyItemAsync(AccountId, [itemInfo], [assetInfo]);
    }

    public async Task<InventoryDbResult> GetInventoryOneItemAsync(int itemIndex)
    {
        var list = await GetInventoryListAsync();
        var item = list.FirstOrDefault(x => x.item_index == itemIndex);
        return item ?? null;
    }
    
}