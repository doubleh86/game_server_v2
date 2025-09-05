using DbContext.MainDbContext.DbResultModel.GameDbModels;
using NetworkProtocols.WebApi.Commands.User;
using NetworkProtocols.WebApi.ToClientModels;

namespace ApiServer.Utils.GameUtils;

public class RefreshDataHelper
{
    private GameUserDbModel _gameUserDbModel;
    private readonly Dictionary<int, InventoryDbResult> _changeItemInfo = [];
    private readonly Dictionary<int, AssetDbResult> _changeAssetInfo = [];

    public List<AssetDbResult> AssetChangeList => _changeAssetInfo.Values.ToList();
    public List<InventoryDbResult> InventoryChangeList => _changeItemInfo.Values.ToList();
    public bool HasRefreshData { get; } = false;

    public RefreshDataHelper()
    {
        HasRefreshData = true;
    }

    public void SetGameUserInfo(GameUserDbModel dbModel)
    {
        _gameUserDbModel = dbModel;
    }

    public void AddChangeItemList(InventoryDbResult changeItemInfo)
    {
        _changeItemInfo[changeItemInfo.item_index] = changeItemInfo;
    }

    public void AddChangeAssetList(AssetDbResult changeAssetInfo)
    {
        _changeAssetInfo[changeAssetInfo.asset_type] = changeAssetInfo;
    }

    public GameUserInfo GetGameUserInfo()
    {
        return _gameUserDbModel.ToClient();
    }

    public List<InventoryItemInfo> GetChangeItemListToClient()
    {
        return _changeItemInfo.Values.Select(x => x.ToClient()).ToList();
    }

    public List<AssetInfo> GetChangeAssetListToClient()
    {
        return _changeAssetInfo.Values.Select(x => x.ToClient()).ToList();
    }
}