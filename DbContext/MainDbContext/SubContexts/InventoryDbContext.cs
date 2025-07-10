using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.ProcedureCommands.InventoryCommands;
using ServerFramework.SqlServerServices.Models;

namespace DbContext.MainDbContext.SubContexts;

public class InventoryDbContext : MainDbContext
{
    public InventoryDbContext(SqlServerDbInfo serverInfo) : base(serverInfo)
    {
    }

    public async Task<List<InventoryDbResult>> GetInventoryDbResult(long accountId)
    {
        await using var connection = _GetConnection();
        var command = new GetInventoryListAsync(this);
        command.SetParameters(accountId);

        return await command.ExecuteProcedureAsync();
    }
}