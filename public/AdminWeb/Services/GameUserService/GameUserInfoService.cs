using AdminWeb.Services.GameUserService.Modules;
using Blazored.SessionStorage;
using DbContext.MainDbContext.DbResultModel.AdminTool;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.SubContexts;
using DbContext.SharedContext.DbResultModel;
using ServerFramework.CommonUtils.Helper;

namespace AdminWeb.Services.GameUserService;

public class GameUserInfoService
{
    private readonly ISessionStorageService _sessionStorageService;
    private readonly AdminGameUserModule _gameUserModule;
    
    private GetAccountDbResult _accountDbResult;
    
    public GetAccountDbResult GetAccountDbResult => _accountDbResult;
    
    public GameUserInfoService(AdminToolServerService serverService, ISessionStorageService sessionStorageService)
    {
        _sessionStorageService = sessionStorageService;
        _gameUserModule = new AdminGameUserModule(serverService);
    }

    public async Task<AdminGameUserDbModel> SetAccountDbResult(GetAccountDbResult selected)
    {
        if (selected == null || selected.AccountId < 1)
            return null;
        
        _accountDbResult = selected;
        
        
        var dbUserInfo = await _gameUserModule.GetAdminGameUserDbModelAsync(_accountDbResult);
        if (dbUserInfo == null)
            return null;
        
        await _sessionStorageService.SetItemAsync("selectedAccountInfo", selected);
        return dbUserInfo;
    }

    public GameUserDbModel GetUserDbModel()
    {
        return _gameUserModule.GetUserDbModel() ?? new GameUserDbModel();
    }

    public async Task<bool> IsSelectedUser()
    {
        var userDbModel = _gameUserModule.GetUserDbModel();
        if (userDbModel != null)
            return true;
        
        var cached = await _sessionStorageService.GetItemAsync<GetAccountDbResult>("selectedAccountInfo");
        if (cached == null)
            return false;
        
        await SetAccountDbResult(cached);
        return _gameUserModule.GetUserDbModel() != null;
    }

    public void ResetUserDbInfo()
    {
        _accountDbResult = null;
        _gameUserModule.Reset();
        
        _sessionStorageService.RemoveItemAsync("selectedAccountInfo");
    }
    
}