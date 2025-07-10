using ApiServer.Handlers.GameModules;
using ApiServer.Services;
using ApiServer.Utils;
using NetworkProtocols.WebApi;
using NetworkProtocols.WebApi.ToClientModels;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.Handlers;

public class ShopHandler : BaseHandler
{
    private long _accountId;
    public ShopHandler(long accountId, ApiServerService serverService) : base(serverService)
    {
        _accountId = accountId;
    }
    
    public override Task InitializeModulesAsync(SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo)
    {
        var inventoryModule = new InventoryModule(_accountId, masterDbInfo, slaveDbInfo);
        _AddModule(nameof(InventoryModule), inventoryModule);
        
        return Task.CompletedTask;
    }

    public async Task<List<InventoryItemInfo>> BuyShopItemAsync(int itemIndex)
    {
        var inventoryModule = GetModule<InventoryModule>();
        var result = await inventoryModule.AddInventoryItemAsync(itemIndex, 1);
        if(result == false)
            throw new ApiServerException(ResultCode.GameError, "Failed to buy shop item");

        var list = await inventoryModule.GetInventoryListAsync();
        return list.Select(x => x.ToClient()).ToList();
    }

}