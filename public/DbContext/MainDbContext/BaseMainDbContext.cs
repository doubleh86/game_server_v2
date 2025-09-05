using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.ProcedureCommands.AssetCommands;
using DbContext.MainDbContext.ProcedureCommands.GameUserCommands;
using DbContext.MainDbContext.ProcedureCommands.InventoryCommands;
using Microsoft.Data.SqlClient;
using ServerFramework.SqlServerServices.DapperUtils;
using ServerFramework.SqlServerServices.Models;

namespace DbContext.MainDbContext;

public abstract class BaseMainDbContext : DapperServiceBase
{
    protected BaseMainDbContext(SqlServerDbInfo serverInfo) : base(serverInfo)
    {
    }

    

    protected async Task<bool> _UpdateAssetInfoAsync(long accountId, List<AssetDbResult> assetList, SqlTransaction transaction = null)
    {
        var command = new UpdateAssetListAsync(this, transaction: transaction);
        command.SetParameters(new UpdateAssetListAsync.InParameters
        {
            AccountId = accountId,
            AssetList = assetList
        });
        
        return await command.ExecuteProcedureAsync();
    }
    
    protected async Task<bool> _UpdateInventoryItemAsync(long accountId, List<InventoryDbResult> itemList, SqlTransaction transaction = null)
    {
        var command = new UpdateInventoryListAsync(this, transaction: transaction);
        command.SetParameters(new UpdateInventoryListAsync.InParameters
        {
            AccountId = accountId,
            InventoryList = itemList
        });
        
        return await command.ExecuteProcedureAsync();
    }
    
}