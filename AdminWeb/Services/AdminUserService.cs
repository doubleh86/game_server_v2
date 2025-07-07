using AdminWeb.Services.Models;
using Blazored.SessionStorage;
using DbContext.AdminDbContext;
using DbContext.AdminDbContext.DbResultModel;
using ServerFramework.CommonUtils.Helper;
using ServerFramework.SqlServerServices.Models;

namespace AdminWeb.Services;

public enum SignInErrorType
{
    Ok = 0,
    NotExistAdmin = 1,
    WrongPassword = 2,
    NeedSignOut = 3,
    UnknownError = 4
}
public class AdminUserService
{
    private const string _SuperUserId = "root";
    
    private readonly AdminToolServerService _serverService;
    private readonly ISessionStorageService _sessionStorageService;
    private readonly LoggerService _loggerService;
    private readonly AdminDbContext _adminDbContext;

    private AdminUserDbModel _adminUserInfo;
    public string GetUserId => _adminUserInfo?.user_id;

    public event Func<Task> RefreshRequestedAsync;
    
    public AdminUserService(AdminToolServerService serverService, ISessionStorageService sessionStorage)
    {
        _serverService = serverService;
        _loggerService = _serverService.LoggerService;
        _sessionStorageService = sessionStorage;
        
        _adminDbContext = AdminDbContext.Create();
    }

    public async Task InitializeAsync()
    {
        var result = await IsSignedAsync();
        if (result == true)
            return;

        if (_serverService.ServerExtraOption.UseDefaultSuperUser == false)
            return;

        try
        {
            var dbResult = await _adminDbContext.GetAdminUserInfo(_SuperUserId);
            if (dbResult == null)
            {
                var password = PasswordHelper.PasswordHash(_SuperUserId, "Fkdnsem1!");
                dbResult = await _adminDbContext.CreateNewAdminUserInfo(_SuperUserId, password, (int)AdminType.SuperAdmin);
                if (dbResult == null)
                    throw new DatabaseException(ServerError.DbError, "Create Super admin user failed");
            }

            _adminUserInfo = dbResult;
            await _sessionStorageService.SetItemAsync("adminInfo", _adminUserInfo);
        }
        catch (DatabaseException e)
        {
            _loggerService.Warning("Database exception occured", e);
        }
        catch (Exception e)
        {
            _loggerService.Warning("Database exception occured [Exception]", e);
        }
    }
    
    public async Task<bool> IsSignedAsync()
    {
        if (_adminUserInfo != null)
            return true;
        
        var result = await _sessionStorageService.GetItemAsync<AdminUserDbModel>("adminInfo");
        if (result == null) 
            return false;
        
        _adminUserInfo = result;
        return true;
    }

    public async Task SignOutAsync()
    {
        await _sessionStorageService.RemoveItemAsync("adminInfo");
        _adminUserInfo = null;
    }

    public async Task<SignInErrorType> SignInAsync(string userId, string password)
    {
        if (await IsSignedAsync() == true)
            return SignInErrorType.NeedSignOut;

        try
        {
            _adminUserInfo = await _adminDbContext.GetAdminUserInfo(userId);
            if (_adminUserInfo == null)
                return SignInErrorType.NotExistAdmin;

            if (PasswordHelper.CheckPassword(userId, password, _adminUserInfo.user_password) == false)
                return SignInErrorType.WrongPassword;
            
            await _sessionStorageService.SetItemAsync("adminInfo", _adminUserInfo);
            return SignInErrorType.Ok;
        }
        catch (DatabaseException e)
        {
            _loggerService.Error("SignInAsync Db Failed", e);
            return SignInErrorType.UnknownError;
        }
        catch (Exception e)
        {
            _loggerService.Error("SignInAsync Failed", e);
            return SignInErrorType.UnknownError;
        }
    }

    public async Task CallRequestRefresh()
    {
        if (RefreshRequestedAsync == null)
            return;

        var handlers = RefreshRequestedAsync.GetInvocationList()
                                                           .Cast<Func<Task>>()
                                                           .Select(handler => handler.Invoke());

        await Task.WhenAll(handlers);
    }

    public async Task<bool> CheckExistAdminAsync(string loginId)
    {
        var result = await _adminDbContext.GetAdminUserInfo(loginId);
        
        return result != null;
    }

    public async Task<bool> CreateAdminUserInfoAsync(string adminId, string password, int adminType)
    {
        var result = await _adminDbContext.CreateNewAdminUserInfo(adminId, password, adminType);
        return result != null;
    }
}