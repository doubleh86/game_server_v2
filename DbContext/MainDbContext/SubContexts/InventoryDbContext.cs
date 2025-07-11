using System.Data;
using DbContext.Common;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.ProcedureCommands.AssetCommands;
using DbContext.MainDbContext.ProcedureCommands.InventoryCommands;
using Microsoft.Data.SqlClient;
using NetworkProtocols.WebApi;
using ServerFramework.SqlServerServices.Models;

namespace DbContext.MainDbContext.SubContexts;

public sealed class InventoryDbContext : BaseMainDbContext
{
    public InventoryDbContext(SqlServerDbInfo serverInfo) : base(serverInfo)
    {
    }

    public async Task<List<InventoryDbResult>> GetInventoryDbResultAsync(long accountId)
    {
        await using var connection = _GetConnection();
        var command = new GetInventoryListAsync(this);
        command.SetParameters(new GetInventoryListAsync.InParameters
        {
            AccountId = accountId
        });

        return await command.ExecuteProcedureAsync();
    }

    public async Task<bool> InsertInventoryItemAsync(long accountId, List<InventoryDbResult> itemList)
    {
        await using var connection = _GetConnection();
        return await _InsertInventoryItemAsync(accountId, itemList);
    }

    private async Task<bool> _InsertInventoryItemAsync(long accountId, List<InventoryDbResult> itemList, SqlTransaction transaction = null)
    {
        var command = new UpdateInventoryListAsync(this, transaction: transaction);
        command.SetParameters(new UpdateInventoryListAsync.InParameters
        {
            AccountId = accountId,
            InventoryList = itemList
        });
        
        return await command.ExecuteProcedureAsync();
    }

    public async Task<bool> ShopBuyItemAsync(long accountId, List<InventoryDbResult> itemList, List<AssetDbResult> assetList)
    {
        await using var connection = _GetConnection();
        await connection.OpenAsync();
        if (await connection.BeginTransactionAsync(IsolationLevel.ReadUncommitted) is not SqlTransaction transaction)
            throw new DbContextException(DbErrorCode.TransactionError, "ShopBuyItemAsync transaction is not open");
        
        try
        {
            var result = await _InsertInventoryItemAsync(accountId, itemList, transaction);
            if (result == false)
                throw new DbContextException(DbErrorCode.ProcedureError, "_InsertInventoryItemAsync error");
            
            var result2 = await _UpdateAssetInfoAsync(accountId, assetList, transaction);
            if(result2 == false)
                throw new DbContextException(DbErrorCode.ProcedureError, "_UpdateAssetInfoAsync error");
            
            transaction.Commit();
            return true;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            throw new DbContextException(DbErrorCode.ProcedureError, $"[ErrorMessage : {e.Message}][ResultCode : {e.HResult}]");
        }
    }
}