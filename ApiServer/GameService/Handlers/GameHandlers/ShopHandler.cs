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

public class ShopHandler(long accountId, ApiServerService serverService, EventService eventService) : BaseHandler(accountId, serverService, eventService)
{
    public override async Task InitializeModulesAsync(SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo)
    {
        await base.InitializeModulesAsync(masterDbInfo, slaveDbInfo);
        
        var inventoryModule = new InventoryModule(_accountId, masterDbInfo, slaveDbInfo);
        _AddModule(nameof(InventoryModule), inventoryModule);

        var assetModule = new AssetInfoModule(_accountId, masterDbInfo, slaveDbInfo);
        _AddModule(nameof(AssetInfoModule), assetModule);
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
        var itemInfo = await inventoryModule.GetInventoryOneItemAsync(itemIndex) ?? InventoryDbResult.Create(itemIndex, 0);
        itemInfo.AddItemAmount(amount);
        assetInfo.UseAsset(needAssetAmount);
        
        await inventoryModule.BuyInventoryItemAsync(itemInfo, assetInfo);

        var refreshDataHelper = _GetRefreshDataHelper();
        refreshDataHelper.AddChangeItemList(itemInfo);
        refreshDataHelper.AddChangeAssetList(assetInfo);
        
        return (itemInfo.ToClient(), assetInfo.ToClient());
    }

    private async Task<(bool, AssetDbResult)> _IsPossiblePayment(int assetType, int price)
    {
        var assetInfo = await GetModule<AssetInfoModule>().GetAssetInfoAsync(assetType);
        if(assetInfo == null)
            throw new ApiServerException(ResultCode.GameError, $"Asset type not found [{assetType}]");
        
        return (assetInfo.amount >= price, assetInfo);
    }

}