using ApiServer.GameService.Models;
using DbContext.Common.Models;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.SubContexts;
using ServerFramework.CommonUtils.DateTimeHelper;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.GameService.GameModules;

public class MailModule : BaseModule<MailDbContext>, IGameModule
{
    public long AccountId { get; set; }
    private Dictionary<long, MailInfoDbResult> _mailLists = null;
    public MailModule(long accountId, SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo) : base(masterDbInfo, slaveDbInfo)
    {
        AccountId = accountId;
    }

    public async Task<Dictionary<long, MailInfoDbResult>> GetMailListAsync()
    {
        if (_mailLists != null)
            return _mailLists;

        var dbContext = GetDbContext(true);
        var dbResult = await dbContext.GetMailInfoDbResultAsync(AccountId);
        if (dbResult == null)
        {
            _mailLists = new Dictionary<long, MailInfoDbResult>();
            return _mailLists;
        }

        _mailLists = dbResult.ToDictionary(x => x.mail_uid);
        return _mailLists;
    }

    public async Task<List<long>> ReceiveMailRewardAsync(List<long> mailUidList)
    {
        var mailList = await GetMailListAsync();
        var receivedFailed =  new List<long>();
        var currentServerTime = TimeZoneHelper.ServerTimeNow;
        
        foreach (var mailUid in mailUidList)
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
            
            mailDbInfo.is_reward_received = 0;
        }
        
        return receivedFailed;
    }
}