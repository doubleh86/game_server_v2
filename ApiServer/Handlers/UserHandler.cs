using ApiServer.Handlers.GameModules;
using ApiServer.Services;
using DbContext.MainDbContext.DbResultModel;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.Handlers;

public class UserHandler(long accountId, ApiServerService service) : BaseHandler(accountId, service)
{
    
    public override Task InitializeModulesAsync(SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo)
    {
        var gameUserModule = new GameUserModule(_accountId, masterDbInfo, slaveDbInfo);
        _AddModule(nameof(GameUserModule), gameUserModule);
        
        var inventoryModule = new InventoryModule(_accountId, masterDbInfo, slaveDbInfo);
        _AddModule(nameof(InventoryModule), inventoryModule);
        
        return Task.CompletedTask;
    }
    
    public async Task<GameUserDbModel> GetUserInfoAsync()
    {
        var module = GetModule<GameUserModule>();
        var result = await module.GetGameUserDbModelAsync();
        
        var inventoryModule = GetModule<InventoryModule>();
        await inventoryModule.GetInventoryListAsync();
        
        return result;
    }


    
}