using ApiServer.GameService.GameModules;
using ApiServer.Services;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using ServerFramework.CommonUtils.DateTimeHelper;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.GameService.Handlers.GameHandlers;

public class MailHandler(long accountId, ApiServerService serverService) : BaseHandler(accountId, serverService)
{
    public override async Task InitializeModulesAsync(SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo)
    {
        await base.InitializeModulesAsync(masterDbInfo, slaveDbInfo);
        var mailModule = new MailModule(_accountId, masterDbInfo, slaveDbInfo);
        _AddModule(nameof(MailModule), mailModule);
    }
    
    public async Task<List<MailInfoDbResult>> GetMailListAsync()
    {
        var mailModule = GetModule<MailModule>();
        var dbResult = await mailModule.GetMailListAsync();
        if (dbResult == null)
            return [];
        
        return dbResult.Values.ToList();
    }

    public async Task ReceivedMailRewardAsync(List<long> mailIds)
    {
        var currentServerTime = TimeZoneHelper.ServerTimeNow;
        var receivedFailed =  new List<long>();
        
        var mailModule = GetModule<MailModule>();
        var mailList =  await mailModule.GetMailListAsync();
        
        foreach (var mailUid in mailIds)
        {
            if (mailList.TryGetValue(mailUid, out var mailDbInfo) == false)
            {
                receivedFailed.Add(mailUid);
                continue;
            }

            if (mailDbInfo.expiry_date.ToServerTime() > currentServerTime)
            {
                receivedFailed.Add(mailUid);
                continue;
            }

            if (mailDbInfo.is_reward_received == 1)
            {
                receivedFailed.Add(mailUid);
                continue;
            }
            
            mailDbInfo.is_reward_received = 1;
        }
    }
}