using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.ProcedureCommands.AssetCommands;
using DbContext.MainDbContext.ProcedureCommands.GameUserCommands;
using Microsoft.Data.SqlClient;
using ServerFramework.SqlServerServices.DapperUtils;
using ServerFramework.SqlServerServices.Models;

namespace DbContext.MainDbContext;

public abstract class BaseMainDbContext : DapperServiceBase
{
    protected BaseMainDbContext(SqlServerDbInfo serverInfo) : base(serverInfo)
    {
    }

    public async Task<List<AssetDbResult>> GetAssetInfoAsync(long accountId)
    {
        await using var connection = _GetConnection();
        var command = new GetAssetInfoAsync(this);
        command.SetParameters(new GetAssetInfoAsync.InParameters
        {
            AccountId = accountId
        });

        return await command.ExecuteProcedureAsync();
    }

    public async Task<bool> UpdateAssetInfoAsync(long accountId, List<AssetDbResult> assetInfo)
    {
        await using var connection = _GetConnection();
        return await _UpdateAssetInfoAsync(accountId, assetInfo);
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
}