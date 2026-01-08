using DbContext.GameDbContext.DbResultModel;
using DbContext.GameDbContext.MySqlContext.QueryCommand;
using DbContext.SharedContext.MySqlContext;
using ServerFramework.MySqlServices.MySqlDapperUtils;
using ServerFramework.SqlServerServices.Models;

namespace DbContext.GameDbContext;

public class MySqlGameDbContext(SqlServerDbInfo dbInfo) : MySqlDapperServiceBase(dbInfo), IGameDbContext
{
    private static SqlServerDbInfo _defaultServerInfo;

    public static void SetDefaultServerInfo(SqlServerDbInfo serverInfo)
    {
        _defaultServerInfo = serverInfo;
    }

    public static MySqlGameDbContext Create(SqlServerDbInfo serverInfo = null)
    {
        if (serverInfo == null && _defaultServerInfo == null)
            throw new DatabaseException(ServerError.DbError, "No server info provided");

        return serverInfo == null ? new MySqlGameDbContext(_defaultServerInfo) : new MySqlGameDbContext(serverInfo);
    }

    public async Task<PlayerInfoResult> GetPlayerInfoAsync(long accountId)
    {
        await using var connection = _GetConnection();
        var command = new GetPlayerInfoCommandAsync(this);

        return await command.ExecuteQueryAsync(new GetPlayerInfoCommandAsync.InParameters()
        {
            accountId = accountId
        });
    }

    public async Task<int> ItemUseAsync(long accountId, int itemId, int itemCount)
    {
        await using var connection = _GetConnection();
        var command = new ItemUseCommandAsync(this);

        return await command.ExecuteQueryAsync(new ItemUseCommandAsync.InParameters()
        {
            accountId = accountId,
            itemId = itemId,
            itemCount = itemCount
        });
    }
}