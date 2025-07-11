using ApiServer.GameService.GameModules;
using ApiServer.Services;
using ApiServer.Utils;
using DataTableLoader.Models;
using DataTableLoader.Utils.Helper;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using NetworkProtocols.WebApi;
using NetworkProtocols.WebApi.Commands.User;
using NetworkProtocols.WebApi.ToClientModels;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.GameService.Handlers.GameHandlers;

public class ShopHandler(long accountId, ApiServerService serverService) : BaseHandler(accountId, serverService)
{
    public override Task InitializeModulesAsync(SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo)
    {
        base.InitializeModulesAsync(masterDbInfo, slaveDbInfo);
        
        var inventoryModule = new InventoryModule(_accountId, masterDbInfo, slaveDbInfo);
        _AddModule(nameof(InventoryModule), inventoryModule);
        
        return Task.CompletedTask;
    }

    public async Task<(InventoryItemInfo, AssetInfo)> BuyShopItemAsync(int itemIndex, int amount)
    {
        var tableData = DataHelper.GetData<ItemInfoTable>(itemIndex);
        if (tableData == null)
            throw new ApiServerException(ResultCode.SystemError, $"Table data is null [ItemInfoTable][{itemIndex}]");

        var needAssetAmount = tableData.GetPrice(amount);
        var (isPossible, assetInfo) = await _IsPossiblePayment(tableData.asset_type, needAssetAmount); 
        if(isPossible == false)
            throw new ApiServerException(ResultCode.GameError, $"Not enough asset price for {tableData.asset_type}");
        
        var inventoryModule = GetModule<InventoryModule>();
        var itemInfo = await inventoryModule.GetInventoryOneItemAsync(itemIndex);
        itemInfo.AddItemAmount(amount);
        assetInfo.UseAsset(needAssetAmount);
        
        await inventoryModule.BuyInventoryItemAsync(itemInfo, assetInfo);
        
        return (itemInfo.ToClient(), assetInfo.ToClient());
    }

    private async Task<(bool, AssetDbResult)> _IsPossiblePayment(int assetType, int price)
    {
        var assetInfo = await GetModule<GameUserModule>().GetAssetInfoAsync(assetType);
        if(assetInfo == null)
            throw new ApiServerException(ResultCode.GameError, $"Asset type not found [{assetType}]");
        
        return (assetInfo.amount >= price, assetInfo);
    }

}