using DbContext.MainDbContext.DbResultModel.AdminTool;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.SubContexts;
using DbContext.SharedContext.DbResultModel;
using ServerFramework.CommonUtils.Helper;

namespace AdminWeb.Services.GameUserService.Modules;

public class AdminGameUserModule
{
    private readonly AdminToolServerService _serverService;
    private AdminGameUserDbModel _gameUserDbModel;
    
    public AdminGameUserDbModel GetUserDbModel() => _gameUserDbModel;

    public AdminGameUserModule(AdminToolServerService serverService)
    {
        _serverService = serverService;
    }
    
    public async Task<AdminGameUserDbModel> GetAdminGameUserDbModelAsync(GetAccountDbResult accountDbResult)
    {
        if(_gameUserDbModel != null)
            return _gameUserDbModel;
        
        using var dbContext = new GameUserDbContext(accountDbResult.GetMainDbInfo(isSlave: true));
        var dbResult = await dbContext.GetUserInfoAsync(accountDbResult.AccountId);
        if (dbResult == null)
            return null;
        
        _gameUserDbModel = AdminGameUserDbModel.ToAdminModel(dbResult, TimeZoneHelper.CurrentTimeZone, _serverService.ServerExtraOption.DateTimeVisibleType);
        _gameUserDbModel.LoginId = accountDbResult.LoginId;
        _gameUserDbModel.CreateDate = accountDbResult.CreateDate.ToServerTime().ToString(_serverService.ServerExtraOption.DateTimeVisibleType);
        
        return _gameUserDbModel;
    }

    public void Reset()
    {
        _gameUserDbModel = null;
    }
}