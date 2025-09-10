using ApiServer.GameService.GameModules;
using ApiServer.Services;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using NetworkProtocols.Shared.Enums;
using NetworkProtocols.WebApi.ToClientModels;
using ServerFramework.CommonUtils.DateTimeHelper;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.GameService.Handlers.GameHandlers;

public class MailHandler(long accountId, ApiServerService serverService) : BaseHandler(accountId, serverService)
{
    
    public async Task<List<MailInfoDbResult>> GetMailListAsync()
    {
        var mailModule = _GetModule<MailModule>();
        var dbResult = await mailModule.GetMailListAsync();
        if (dbResult == null)
            return [];
        
        return dbResult.Values.ToList();
    }

    public async Task<List<long>> ReceivedMailRewardAsync(List<long> mailIds)
    {
        var currentServerTime = TimeZoneHelper.ServerTimeNow;
        var receivedFailed =  new List<long>();
        
        var mailModule = _GetModule<MailModule>();
        var mailList =  await mailModule.GetMailListAsync();
        
        var rewards = new List<RewardInfo>();
        var receivedMailInfo = new List<MailInfoDbResult>();
        foreach (var mailUid in mailIds)
        {
            if (mailList.TryGetValue(mailUid, out var mailDbInfo) == false)
            {
                receivedFailed.Add(mailUid);
                continue;
            }

            if (mailDbInfo.expiry_date.ToServerTime() < currentServerTime)
            {
                receivedFailed.Add(mailUid);
                continue;
            }

            if (mailDbInfo.is_reward_received == 1)
            {
                receivedFailed.Add(mailUid);
                continue;
            }
            
            rewards.AddRange(mailDbInfo.GetMailRewardInfoList());
            mailDbInfo.is_reward_received = 1;
            receivedMailInfo.Add(mailDbInfo);
        }

        var refreshDataHelper = _GetRefreshDataHelper();
        var rewardHandler = new RewardHandler(_accountId, _GetModuleManager(), rewards, refreshDataHelper, _loggerService);
        
        await rewardHandler.ReceiveRewardAsync();
        await mailModule.ReceiveMailRewardAsync(receivedMailInfo, refreshDataHelper);
        
        return receivedFailed;
    }

    public static MailInfoDbResult CreateNewMailItem(string message, List<RewardInfo> rewardInfo, int expiryDays)
    {
        var newMailInfo = new MailInfoDbResult
        {
            is_reward_received = 0,
            message_content = message,
            expiry_date = TimeZoneHelper.UtcNow.AddDays(expiryDays)
        };

        if (rewardInfo is { Count: > 0 })
        {
            newMailInfo.AddMailReward(rewardInfo);    
        }
        
        return newMailInfo;
    }
    
#region Maybe for test

    public async Task<List<MailInfoDbResult>> InsertMailItemForTestAsync()
    {
        var rewardInfo = new RewardInfo()
        {
            RewardType = RewardTypeEnums.Asset,
            Index = (int)AssetType.Gold,
            Amount = 100,
        };

        var newMail = CreateNewMailItem("test mail", [rewardInfo], 5);
        var module = _GetModule<MailModule>();
        var result = await module.InsertMailItemAsync([newMail]);

        return result.Values.ToList();
    }
#endregion Maybe for test
}