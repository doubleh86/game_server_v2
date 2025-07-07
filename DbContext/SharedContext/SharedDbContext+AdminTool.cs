using DbContext.SharedContext.DbResultModel;
using DbContext.SharedContext.ProcedureCommands.AdminTool;

namespace DbContext.SharedContext;

public partial class SharedDbContext
{
    public async Task<List<GetAccountDbResult>> GetAccountInfoListAsync(string loginId)
    {
        await using var connection = _GetConnection();
        var command = new GetAccountInfoListAsync(this);
        
        command.SetParameters(loginId);
        return await command.ExecuteProcedureAsync();
    }

    public async Task<List<GetAccountDbResult>> GetAccountInfoTotalListAsync()
    {
        await using var connection = _GetConnection();
        var command = new GetAccountInfoTotalListAsync(this);
        
        return await command.ExecuteProcedureAsync();
    }

    public async Task<bool> CreateEventInfoAsync(EventDbResult dbResult)
    {
        await using var connection = _GetConnection();
        var command = new CreateEventDataAsync(this);
        command.SetParameters(dbResult);

        return await command.ExecuteProcedureAsync() == 0;

    }
}