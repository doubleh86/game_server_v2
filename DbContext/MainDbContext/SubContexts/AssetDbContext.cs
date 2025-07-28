using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.ProcedureCommands.AssetCommands;
using ServerFramework.SqlServerServices.Models;

namespace DbContext.MainDbContext.SubContexts;

public class AssetDbContext : BaseMainDbContext
{
    public AssetDbContext(SqlServerDbInfo serverInfo) : base(serverInfo)
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
}