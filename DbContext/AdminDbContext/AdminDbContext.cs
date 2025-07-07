using DbContext.AdminDbContext.DbResultModel;
using DbContext.AdminDbContext.ProcedureCommands;
using ServerFramework.SqlServerServices.DapperUtils;
using ServerFramework.SqlServerServices.Models;

namespace DbContext.AdminDbContext;

public class AdminDbContext : DapperServiceBase
{
    private static SqlServerDbInfo _defaultServerInfo;

    public AdminDbContext(SqlServerDbInfo serverInfo) : base(serverInfo)
    {
    }
    
    public static void SetDefaultServerInfo(SqlServerDbInfo serverInfo)
    {
        _defaultServerInfo = serverInfo;
    }
    
    public static AdminDbContext Create(SqlServerDbInfo serverInfo = null)
    {
        if(serverInfo == null && _defaultServerInfo == null)
            throw new DatabaseException(ServerError.DbError, "No server info provided");
        
        return serverInfo == null ? new AdminDbContext(_defaultServerInfo) : new AdminDbContext(serverInfo);
    }

    public async Task<AdminUserDbModel> GetAdminUserInfo(string userId)
    {
        await using var connection = _GetConnection();
        var command = new GetAdminUserInfoAsync(this);
        command.SetParameters(userId);

        return await command.ExecuteProcedureAsync();
    }

    public async Task<AdminUserDbModel> CreateNewAdminUserInfo(string userId, string password, int adminType)
    {
        await using var connection = _GetConnection();
        var command = new CreateNewAdminUserAsync(this);
        command.SetParameters(userId, password, adminType);
        
        return await command.ExecuteProcedureAsync();
    }

    public async Task<List<AdminUserDbModel>> GetAllAdminUserList()
    {
        await using var connection = _GetConnection();
        var command = new GetAllAdminUserListAsync(this);
        
        return await command.ExecuteProcedureAsync();
    }
}