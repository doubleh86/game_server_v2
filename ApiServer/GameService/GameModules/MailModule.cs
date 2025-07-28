using ApiServer.GameService.Models;
using DbContext.Common.Models;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.SubContexts;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.GameService.GameModules;

public class MailModule : BaseModule<MailDbContext>, IGameModule
{
    public long AccountId { get; set; }
    private List<MailInfoDbResult> _mailLists = null;
    public MailModule(long accountId, SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo) : base(masterDbInfo, slaveDbInfo)
    {
        AccountId = accountId;
    }

    public async Task<List<MailInfoDbResult>> GetMailListAsync()
    {
        if (_mailLists != null)
            return _mailLists;

        var dbContext = GetDbContext(true);
        _mailLists = await dbContext.GetMailInfoDbResultAsync(AccountId);
        return _mailLists;
    }
}