using DbContext.MainDbContext.DbResultModel;
using DbContext.MainDbContext.ProcedureCommands;
using ServerFramework.SqlServerServices.DapperUtils;
using ServerFramework.SqlServerServices.Models;

namespace DbContext.MainDbContext.SubContexts;

public class GameUserDbContext : DapperServiceBase
{
    public GameUserDbContext(SqlServerDbInfo serverInfo) : base(serverInfo)
    {
    }

    public async Task<GameUserDbModel> GetUserInfoAsync(long accountId)
    {
        await using var connection = _GetConnection();
        var command = new GetGameUserInfoAsync(this);
        command.SetParameters(accountId);

        return await command.ExecuteProcedureAsync();
    }

    public async Task<GameUserDbModel> CreateNewGameUser(long accountId)
    {
        await using var connection = _GetConnection();
        var command = new CreateNewGameUserAsync(this);
        command.SetParameters(accountId);

        return await command.ExecuteProcedureAsync();
    }
}