using DbContext.MainDbContext;
using DbContext.MainDbContext.DbResultModel;
using DbContext.MainDbContext.DbResultModel.AdminTool;
using DbContext.SharedContext.DbResultModel;
using ServerFramework.CommonUtils.Helper;

namespace AdminWeb.Services;

public class UserInfoService
{
    private readonly AdminToolServerService _serverService;
    
    private GetAccountDbResult _accountDbResult;
    private AdminGameUserDbModel _gameUserDbModel;

    public UserInfoService(AdminToolServerService serverService)
    {
        _serverService = serverService;
    }

    public async Task<AdminGameUserDbModel> SetAccountDbResult(GetAccountDbResult selected)
    {
        if (selected == null || selected.AccountId < 1)
            return null;
        
        _accountDbResult = selected;
        var gameDbInfo = _accountDbResult.GetMainDbInfo();
        
        using var dbContext = new MainDbContext(gameDbInfo);
        var dbResult = await dbContext.GetUserInfoAsync(_accountDbResult.AccountId);
        if (dbResult == null)
            return null;
        
        _gameUserDbModel = AdminGameUserDbModel.ToAdminModel(dbResult, TimeZoneHelper.CurrentTimeZone, _serverService.ServerExtraOption.DateTimeVisibleType);
        _gameUserDbModel.LoginId = _accountDbResult.LoginId;
        _gameUserDbModel.CreateDate = _accountDbResult.CreateDate.ToServerTime().ToString(_serverService.ServerExtraOption.DateTimeVisibleType);
        
        return _gameUserDbModel;
    }

    public GameUserDbModel GetUserDbModel()
    {
        return _gameUserDbModel ?? new GameUserDbModel();
    }

    public bool IsSelectedUser()
    {
        return _gameUserDbModel != null;
    }

    public void ResetUserDbInfo()
    {
        _accountDbResult = null;
        _gameUserDbModel = null;
    }
    
}