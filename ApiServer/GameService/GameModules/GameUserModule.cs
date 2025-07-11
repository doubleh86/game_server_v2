using ApiServer.GameService.Models;
using DataTableLoader.Models;
using DataTableLoader.Utils.Helper;
using DbContext.Common.Models;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.SubContexts;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.GameService.GameModules;

public class GameUserModule : BaseModule<GameUserDbContext>, IGameModule
{
    public long AccountId { get; set; }
    private GameUserDbModel _gameUserDbModel;
    private List<AssetDbResult> _assetDbResults;
    public GameUserModule(long accountId, SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo) : base(masterDbInfo, slaveDbInfo)
    {
        AccountId = accountId;
    }

    public async Task<GameUserDbModel> GetGameUserDbModelAsync()
    {
        if(_gameUserDbModel != null)
            return _gameUserDbModel;
        
        _gameUserDbModel = await GetDbContext(true).GetUserInfoAsync(AccountId);
        return _gameUserDbModel;
    }

    public async Task<GameUserDbModel> CreateGameUserAsync(List<AssetDbResult> defaultAssets)
    {
        var result = await GetDbContext().CreateNewGameUser(AccountId, defaultAssets);
        return result;
    }

    public async Task<List<AssetDbResult>> GetAssetInfoListAsync()
    {
        if(_assetDbResults != null)
            return _assetDbResults;
        
        _assetDbResults = await GetDbContext(true).GetAssetInfoAsync(AccountId);
        if (_assetDbResults != null && _assetDbResults.Count > 0)
            return _assetDbResults;
        
        var tableList = DataHelper.GetDataList<AssetInfoTable>();
        _assetDbResults = tableList.Select(table => AssetDbResult.Create(table.asset_type, table.default_asset_value)).ToList();
            
        await GetDbContext().UpdateAssetInfoAsync(AccountId, _assetDbResults);
        return _assetDbResults;
    }

    public async Task<AssetDbResult> GetAssetInfoAsync(int assetType)
    {
        var list = await GetAssetInfoListAsync();
        var assetInfo = list.FirstOrDefault(dbResult => dbResult.asset_type == assetType);
        if(assetInfo != null)
            return assetInfo;
        
        var tableData = DataHelper.GetData<AssetInfoTable>(assetType);
        var defaultAssetInfo = AssetDbResult.Create(tableData.asset_type, tableData.default_asset_value);
        list.Add(defaultAssetInfo);
        
        return defaultAssetInfo;
    }
    
}