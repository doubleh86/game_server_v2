using DbContext.SharedContext.DbResultModel;
using DbContext.SharedContext.SqlServerContext.ProcedureCommands;
using DbContext.SharedContext.SqlServerContext.ProcedureCommands.AdminTool;
using ServerFramework.SqlServerServices.DapperUtils;
using ServerFramework.SqlServerServices.Models;

namespace DbContext.SharedContext.SqlServerContext;

public partial class SharedDbContext : DapperServiceBase
{
    private static SqlServerDbInfo _defaultServerInfo;

    private SharedDbContext(SqlServerDbInfo settings) : base(settings)
    {
    }

    public static void SetDefaultServerInfo(SqlServerDbInfo serverInfo)
    {
        _defaultServerInfo = serverInfo;
    }

    public static SharedDbContext Create(SqlServerDbInfo serverInfo = null)
    {
        if(serverInfo == null && _defaultServerInfo == null)
            throw new DatabaseException(ServerError.DbError, "No server info provided");
        
        return serverInfo == null ? new SharedDbContext(_defaultServerInfo) : new SharedDbContext(serverInfo);
    }

    public async Task<GetAccountDbResult> GetAccountInfoAsync(string loginId)
    {
        await using var connections = _GetConnection();
        var command = new GetAccountInfoByIdAsync(this);
        command.SetParameters(new GetAccountInfoByIdAsync.InParameters
        {
            LoginId = loginId
        });

        return await command.ExecuteProcedureAsync();
    }

    public async Task<int> CreateAccountAsync(string loginId)
    {
        await using var connections = _GetConnection();
        var command = new CreateAccountInfoAsync(this);
        command.SetParameters(new CreateAccountInfoAsync.InParameters
        {
            LoginId = loginId,
        });
        
        return await command.ExecuteProcedureAsync();
    }
    

    public async Task<List<EventDbResult>> GetEventInfoListAsync()
    {
        await using var connection = _GetConnection();
        var command = new GetEventInfoTotalList(this);
        
        return await command.ExecuteProcedureAsync();
    }
}