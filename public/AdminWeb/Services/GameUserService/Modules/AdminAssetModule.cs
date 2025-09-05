using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.SubContexts;
using DbContext.SharedContext.DbResultModel;

namespace AdminWeb.Services.GameUserService.Modules;

public class AdminAssetModule
{
    private readonly GetAccountDbResult  _accountDbResult;
    private List<AssetDbResult> _assetDbResults = null;

    public AdminAssetModule(GetAccountDbResult accountDbResult)
    {
        _accountDbResult = accountDbResult;
    }

    public async Task<List<AssetDbResult>> GetAssetListAsync()
    {
        if(_assetDbResults != null)
            return _assetDbResults;
        
        var gameDbInfo = _accountDbResult.GetMainDbInfo(isSlave: true);
        using var dbContext = new AssetDbContext(gameDbInfo);

        _assetDbResults = await dbContext.GetAssetInfoAsync(_accountDbResult.AccountId);
        return _assetDbResults;
    }
}