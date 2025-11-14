using DbContext.Common;
using DbContext.SharedContext.MySqlContext.QueryCommands;
using NetworkProtocols.WebApi;
using ServerFramework.MySqlServices.MySqlDapperUtils;
using ServerFramework.SqlServerServices.Models;

namespace DbContext.SharedContext.MySqlContext;

public class MySqlSharedDbContext(SqlServerDbInfo dbInfo) : MySqlDapperServiceBase(dbInfo)
{
    private static SqlServerDbInfo _defaultServerInfo;

    public static void SetDefaultServerInfo(SqlServerDbInfo serverInfo)
    {
        _defaultServerInfo = serverInfo;
    }
    
    public static MySqlSharedDbContext Create(SqlServerDbInfo serverInfo = null)
    {
        if(serverInfo == null && _defaultServerInfo == null)
            throw new DatabaseException(ServerError.DbError, "No server info provided");
        
        return serverInfo == null ? new MySqlSharedDbContext(_defaultServerInfo) : new MySqlSharedDbContext(serverInfo);
    }

    public async Task<int> CreateAccountAsync(string loginId)
    {
        await using var connection = _GetConnection();
        await connection.OpenAsync();
        
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var command = new CreateAccountCommandAsync(this, transaction);
            var gameDbUid = await command.ExecuteQueryAsync(new CreateAccountCommandAsync.InParameters()
            {
                LoginId = loginId
            });
            
            await transaction.CommitAsync();
            return gameDbUid;
        }
        catch (DbContextException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw new DbContextException(DbErrorCode.ProcedureError, $"[ErrorMessage : {e.Message}][ResultCode : {e.HResult}]");
        }
        
    }
}