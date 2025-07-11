using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.ProcedureCommands.InventoryCommands;
using ServerFramework.SqlServerServices.Models;

namespace DbContext.MainDbContext.SubContexts;

public sealed class InventoryDbContext : BaseMainDbContext
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

    public async Task<bool> InsertInventoryItem(long accountId, List<InventoryDbResult> itemList)
    {
        await using var connection = _GetConnection();
        var command = new UpdateInventoryListAsync(this);
        command.SetParameters(accountId, itemList);
        
        return await command.ExecuteProcedureAsync();
    }
}