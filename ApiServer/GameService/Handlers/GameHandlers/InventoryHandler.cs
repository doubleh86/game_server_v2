using ApiServer.GameService.GameModules;
using ApiServer.Services;
using ApiServer.Utils;
using DataTableLoader.Models;
using DataTableLoader.Utils.Helper;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using NetworkProtocols.Shared.Enums;
using NetworkProtocols.WebApi;
using NetworkProtocols.WebApi.ToClientModels;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.GameService.Handlers.GameHandlers;

public class InventoryHandler(long accountId, ApiServerService serverService, EventService eventService) : BaseHandler(accountId, serverService, eventService)
{
    public override async Task InitializeModulesAsync(SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo)
    {
        await base.InitializeModulesAsync(masterDbInfo, slaveDbInfo);
        var inventoryModule = new InventoryModule(_accountId, masterDbInfo, slaveDbInfo);
        _AddModule(nameof(InventoryModule), inventoryModule);
    }

    public async Task<InventoryItemInfo> UseInventoryItemAsync(int itemIndex, int quantity)
    {
        var inventoryModule = GetModule<InventoryModule>();
        var itemInfo = await inventoryModule.GetInventoryOneItemAsync(itemIndex);
        if (itemInfo == null)
            throw new ApiServerException(ResultCode.InvalidRequest, "Invalid item index");

        if (itemInfo.item_amount < quantity)
            throw new ApiServerException(ResultCode.InvalidRequest, "Invalid item quantity");
        
        await _UseInventoryItemAsync(itemInfo, quantity);
        return itemInfo.ToClient();
    }

    private async Task<bool> _UseInventoryItemAsync(InventoryDbResult dbInfo, int quantity)
    {
        var itemTable = DataHelper.GetData<ItemInfoTable>(dbInfo.item_index);
        if(itemTable == null)
            throw new ApiServerException(ResultCode.InvalidRequest, $"Table not found [ItemInfoTable][{dbInfo.item_index}]");

        dbInfo.UseItem(quantity);
        return (ItemType)itemTable.item_type switch
        {
            ItemType.ExpItem => await _UseExpItemAsync(itemTable, dbInfo, quantity),
            _ => throw new ApiServerException(ResultCode.GameError, $"Invalid item type [{itemTable.item_type}]")
        };
    }

    private async Task<bool> _UseExpItemAsync(ItemInfoTable itemTable, InventoryDbResult dbInfo, int quantity)
    {
        var gameUserModule = GetModule<GameUserModule>();
        var addExp = itemTable.use_amount * quantity;
        var gameUserDbModel = await gameUserModule.AddPlayerExpUseItemAsync(addExp, [dbInfo]);
        
        var refreshDataHelper = _GetRefreshDataHelper();
                
        refreshDataHelper.AddChangeItemList(dbInfo);
        refreshDataHelper.SetGameUserInfo(gameUserDbModel);

        return true;
    }
}