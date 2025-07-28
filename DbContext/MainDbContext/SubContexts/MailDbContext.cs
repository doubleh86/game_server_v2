using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.ProcedureCommands.MailCommands;
using ServerFramework.SqlServerServices.Models;

namespace DbContext.MainDbContext.SubContexts;

public class MailDbContext : BaseMainDbContext
{
    public MailDbContext(SqlServerDbInfo serverInfo) : base(serverInfo)
    {
    }

    public async Task<List<MailInfoDbResult>> GetMailInfoDbResultAsync(long accountId)
    {
        await using var connection = _GetConnection();
        var command = new GetMailListAsync(this);
        command.SetParameters(new GetMailListAsync.InParameters
        {
            AccountId = accountId
        });

        return await command.ExecuteProcedureAsync();
    }
}