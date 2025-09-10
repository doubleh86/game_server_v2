using ApiServer.GameService.GameModules;
using ApiServer.GameService.GameModules.Manager;
using ApiServer.GameService.Models;
using ApiServer.Utils;
using ApiServer.Utils.GameUtils;
using DataTableLoader.Models;
using DataTableLoader.Utils.Helper;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using NetworkProtocols.Shared.Enums;
using NetworkProtocols.WebApi;
using NetworkProtocols.WebApi.ToClientModels;
using ServerFramework.CommonUtils.Helper;

namespace ApiServer.GameService.Handlers;

public class RewardHandler
{
    private long _accountId;
    private readonly List<RewardInfo> _receiveRewards;
    private readonly RefreshDataHelper _refreshDataHelper;
    private readonly GameDbModuleManager _moduleManager;

    private readonly Dictionary<string, IGameModule> _modules;
    
    private readonly LoggerService _loggerService;
    
    public RewardHandler(long accountId, GameDbModuleManager moduleManager,
                         List<RewardInfo> receiveRewards, RefreshDataHelper refreshDataHelper,
                         LoggerService loggerService)
    {
        _accountId = accountId;
        _moduleManager = moduleManager;
        _receiveRewards = receiveRewards;
        _refreshDataHelper = refreshDataHelper;
        _loggerService = loggerService;
    }

    private T _GetModule<T>() where T : class, IGameModule
    {
        return _moduleManager.GetModule<T>();
    }

    public async Task ReceiveRewardAsync()
    {
        if (_receiveRewards == null || _receiveRewards.Count == 0)
            return;

        var rewardGroup = _receiveRewards.GroupBy(x => x.RewardType);
        foreach (var group in rewardGroup)
        {
            var rewardType = group.Key;
            var rewards = group.ToList();
            switch (rewardType)
            {
                case RewardTypeEnums.Asset:
                    await _ReceiveAssetReward(rewards);
                    break;
                case RewardTypeEnums.Item:
                    await _ReceiveItemReward(rewards);
                    break;
                default:
                    continue;
            }
        }
    }

    private async Task<bool> _ReceiveItemReward(List<RewardInfo> receiveRewards)
    {
        foreach (var reward in receiveRewards)
        {
            var tableData = DataHelper.GetData<ItemInfoTable>(reward.Index);
            if (tableData == null)
            {
                _loggerService.Warning($"Invalid item reward: {reward.Index}");
                continue;
            }

            var result = await _GetModule<InventoryModule>().AddInventoryItemAsync(tableData.item_index, reward.Amount);
            _refreshDataHelper.AddChangeItemList(result);
        }

        return true;
    }

    private async Task<bool> _ReceiveAssetReward(List<RewardInfo> receiveRewards)
    {
        foreach (var reward in receiveRewards)
        {
            var tableData = DataHelper.GetData<AssetInfoTable>(reward.Index);
            if (tableData == null)
            {
                _loggerService.Warning($"Invalid Asset Index [{reward.Index}]");
                continue;
            }
                
            var result = await _GetModule<AssetInfoModule>().AddAssetInfoAsync((AssetType)tableData.asset_type, reward.Amount);
            _refreshDataHelper.AddChangeAssetList(result);
        }

        return true;
    }
    
}