using DbContext.Common;
using DbContext.SharedContext.DbResultModel;
using DbContext.SharedContext.ProcedureCommands.AdminTool;
using Microsoft.Data.SqlClient;
using NetworkProtocols.WebApi;

namespace DbContext.SharedContext;

public partial class SharedDbContext
{
    public async Task<List<GetAccountDbResult>> GetAccountInfoListAsync(string loginId)
    {
        await using var connection = _GetConnection();
        var command = new GetAccountInfoListAsync(this);
        
        command.SetParameters(new GetAccountInfoListAsync.InParameters
        {
            SearchValue = loginId
        });
        
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

    public async Task<bool> RemoveEventInfoAsync(EventDbResult dbResult, bool isRemoveData)
    {
        await using var connection = _GetConnection();
        var command = new RemoveEventDateAsync(this);
        command.SetParameters(new RemoveEventDateAsync.InParameters()
        {
            EventId = dbResult.event_id,
            IsRemove = isRemoveData
        });

        return await command.ExecuteProcedureAsync();
    }
}