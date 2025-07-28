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

public class UserHandler(long accountId, ApiServerService service) : BaseHandler(accountId, service)
{
    public override async Task InitializeModulesAsync(SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo)
    {
        await base.InitializeModulesAsync(masterDbInfo, slaveDbInfo);
        
        var inventoryModule = new InventoryModule(_accountId, masterDbInfo, slaveDbInfo);
        _AddModule(nameof(InventoryModule), inventoryModule);

        var assetModule = new AssetInfoModule(_accountId, masterDbInfo, slaveDbInfo);
        _AddModule(nameof(AssetInfoModule), assetModule);
    }
    
    public async Task<(GameUserInfo, List<InventoryItemInfo>, List<AssetInfo>)> GetUserInfoAsync()
    {
        var module = GetModule<GameUserModule>();
        var result = await module.GetGameUserDbModelAsync() ?? await _CreateUserDbModelAsync();

        var inventoryModule = GetModule<InventoryModule>();
        var inventoryInfo = await inventoryModule.GetInventoryListAsync();
        var inventoryListToClient = inventoryInfo.Select(dbResult => dbResult.ToClient()).ToList();

        var assetModule = GetModule<AssetInfoModule>();
        var assetLists = await assetModule.GetAssetInfoListAsync();
        var assetListToClient = assetLists.Select(dbResult => dbResult.ToClient()).ToList();
        
        return (result.ToClient(), inventoryListToClient, assetListToClient); 
    }

    private async Task<GameUserDbModel> _CreateUserDbModelAsync()
    {
        var module = GetModule<GameUserModule>();

        var tableList = DataHelper.GetDataList<AssetInfoTable>();
        var defaultAssets = tableList.Select(table => AssetDbResult.Create(table.asset_type, table.default_asset_value)).ToList();
        
        var gameResult = await module.CreateGameUserAsync(defaultAssets);
        if(gameResult == null)
            throw new ApiServerException(GameResultCode.GameError, "Create game user failed");
        
        return gameResult;
    }
    
}