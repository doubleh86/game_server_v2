using DbContext.Common;
using DbContext.MainDbContext.DbResultModel;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.ProcedureCommands;
using DbContext.MainDbContext.ProcedureCommands.GameUserCommands;
using Microsoft.Data.SqlClient;
using NetworkProtocols.WebApi;
using ServerFramework.SqlServerServices.Models;

namespace DbContext.MainDbContext.SubContexts;

public sealed class GameUserDbContext : BaseMainDbContext
{
    public GameUserDbContext(SqlServerDbInfo serverInfo) : base(serverInfo)
    {
    }

    public async Task<GameUserDbModel> GetUserInfoAsync(long accountId)
    {
        await using var connection = _GetConnection();
        var command = new GetGameUserInfoAsync(this);
        command.SetParameters(new GetGameUserInfoAsync.InParameters
        {
            AccountId = accountId
        });

        return await command.ExecuteProcedureAsync();
    }

    public async Task<GameUserDbModel> CreateNewGameUser(long accountId, List<AssetDbResult> defaultAssets)
    {
        await using var connection = _GetConnection();
        await connection.OpenAsync();
        if(await connection.BeginTransactionAsync() is not SqlTransaction transaction)
            throw new DbContextException(DbErrorCode.TransactionError, "[CreateNewGameUser] transaction is not opened");

        try
        {
            var dbResult = await _CreateNewGameUserAsync(accountId);
            if(dbResult == null)
                throw new DbContextException(DbErrorCode.ProcedureError, "[CreateNewGameUser][_CreateNewGameUserAsync] procedure returned null");

            if (defaultAssets != null)
            {
                var assetResult = await _UpdateAssetInfoAsync(accountId, defaultAssets);
                if(assetResult == false)
                    throw new DbContextException(DbErrorCode.ProcedureError, "[CreateNewGameUser][_UpdateAssetInfoAsync] procedure returned null"); 
                
            }
            
            transaction.Commit();
            return dbResult;
        }
        catch (DbContextException)
        {
            transaction.Rollback();
            throw;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            throw new DbContextException(DbErrorCode.ProcedureError, $"[CreateNewGameUser][{e.Message}]");
        }
        
        var command = new CreateNewGameUserAsync(this);
        command.SetParameters(new CreateNewGameUserAsync.InParameters
        {
            AccountId = accountId,
        });

        return await command.ExecuteProcedureAsync();
    }

    private async Task<GameUserDbModel> _CreateNewGameUserAsync(long accountId, SqlTransaction transaction = null)
    {
        var command = new CreateNewGameUserAsync(this, transaction: transaction);
        command.SetParameters(new CreateNewGameUserAsync.InParameters
        {
            AccountId = accountId,
        });

        return await command.ExecuteProcedureAsync();
    }
}