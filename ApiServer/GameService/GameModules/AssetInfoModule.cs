using ApiServer.GameService.Models;
using DataTableLoader.Models;
using DataTableLoader.Utils.Helper;
using DbContext.Common.Models;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.SubContexts;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.GameService.GameModules;

public class AssetInfoModule : BaseModule<AssetDbContext>, IGameModule
{
    public long AccountId { get; set; }
    
    private List<AssetDbResult> _assetDbResults;
    
    public AssetInfoModule(long accountId, SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo) : base(masterDbInfo, slaveDbInfo)
    {
        AccountId = accountId;
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