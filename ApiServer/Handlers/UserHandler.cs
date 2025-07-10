using ApiServer.Handlers.GameModules;
using ApiServer.Services;
using DbContext.MainDbContext.DbResultModel;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.Handlers;

public class UserHandler : BaseHandler
{
    private readonly long _accountId;
    
    public UserHandler(long accountId, ApiServerService service) : base(service)
    {
        _accountId = accountId;
    }

    public override Task InitializeModulesAsync(SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo)
    {
        var gameUserModule = new GameUserModule(masterDbInfo, slaveDbInfo);
        _AddModule(nameof(GameUserModule), gameUserModule);
        
        var inventoryModule = new InventoryModule(masterDbInfo, slaveDbInfo);
        _AddModule(nameof(InventoryModule), inventoryModule);
        
        return Task.CompletedTask;
    }
    
    public async Task<GameUserDbModel> GetUserInfoAsync()
    {
        var module = GetModule<GameUserModule>();
        var result = await module.GetGameUserDbModel(_accountId);
        
        var inventoryModule = GetModule<InventoryModule>();
        var list = await inventoryModule.GetInventoryListAsync(_accountId);
        
        return result;
    }


    
}