using ApiServer.GameService.Models;
using ApiServer.Utils.GameUtils;
using DbContext.Common.Models;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.SubContexts;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.GameService.GameModules;

public class MailModule : BaseModule<MailDbContext>, IGameModule
{
    public long AccountId { get; set; }
    private Dictionary<long, MailInfoDbResult> _mailLists;
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

    public async Task ReceiveMailRewardAsync(List<MailInfoDbResult> mailList, RefreshDataHelper refreshDataHelper)
    {
        var dbContext = GetDbContext();
        await dbContext.ReceivedMailRewardsAsync(AccountId, mailList, refreshDataHelper.InventoryChangeList, refreshDataHelper.AssetChangeList);
    }

    public async Task<Dictionary<long, MailInfoDbResult>> InsertMailItemAsync(List<MailInfoDbResult> mailList)
    {
        var dbContext = GetDbContext();
        if (await dbContext.InsertMailItemAsync(AccountId, mailList) == false) 
            return null;

        if (_mailLists == null) 
            return await GetMailListAsync();
        
        _mailLists.Clear();
        _mailLists = null;

        return await GetMailListAsync();

    }

}