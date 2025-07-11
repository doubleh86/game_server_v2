using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.SubContexts;
using DbContext.SharedContext.DbResultModel;

namespace AdminWeb.Services.GameUserService.Modules;

public class AdminInventoryModule
{
    private readonly GetAccountDbResult  _accountDbResult;
    private List<InventoryDbResult> _inventoryDbResult = null;

    public AdminInventoryModule(GetAccountDbResult accountDbResult)
    {
        _accountDbResult = accountDbResult;
    }

    public async Task<List<InventoryDbResult>> GetInventoryListAsync()
    {
        if(_inventoryDbResult != null)
            return _inventoryDbResult;
        
        var gameDbInfo = _accountDbResult.GetMainDbInfo(isSlave: true);
        using var inventoryDbContext = new InventoryDbContext(gameDbInfo);
        
        _inventoryDbResult = await inventoryDbContext.GetInventoryDbResultAsync(_accountDbResult.AccountId);
        return _inventoryDbResult;
    }
}