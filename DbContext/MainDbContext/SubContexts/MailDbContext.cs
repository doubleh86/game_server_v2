using System.Data;
using DbContext.Common;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.ProcedureCommands.MailCommands;
using Microsoft.Data.SqlClient;
using NetworkProtocols.WebApi;
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

    public async Task<bool> ReceivedMailRewardsAsync(long accountId, List<MailInfoDbResult> mailDbList, 
                                                List<InventoryDbResult> inventoryDbList, 
                                                List<AssetDbResult> assetDbList)
    {
        await using var connection = _GetConnection();
        await connection.OpenAsync();
        if (await connection.BeginTransactionAsync(IsolationLevel.ReadUncommitted) is not SqlTransaction transaction)
            throw new DbContextException(DbErrorCode.TransactionError, "ReceivedMailRewards transaction is not open");

        try
        {
            var command = new ReceiveMailRewardAsync(this, transaction: transaction);
            command.SetParameters(new ReceiveMailRewardAsync.InParameters
            {
                AccountId = accountId,
                MailList = mailDbList,
            });
            
            var result1 = await command.ExecuteProcedureAsync();
            if(result1 == false)
                throw new DbContextException(DbErrorCode.TransactionError, "ReceiveMailRewardAsync procedure error");
            
            var result2 = await _UpdateInventoryItemAsync(accountId, inventoryDbList, transaction);
            if (result2 == false)
                throw new DbContextException(DbErrorCode.ProcedureError, "_InsertInventoryItemAsync error");

            var result3 = await _UpdateAssetInfoAsync(accountId, assetDbList, transaction);
            if (result3 == false)
                throw new DbContextException(DbErrorCode.ProcedureError, "_UpdateAssetInfoAsync error");

            transaction.Commit();
            return true;
        }
        catch (DbContextException)
        {
            transaction.Rollback();
            throw;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            throw new DbContextException(DbErrorCode.ProcedureError, $"[ErrorMessage : {e.Message}][ResultCode : {e.HResult}]");
        }
    }
}