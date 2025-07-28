using ApiServer.GameService.GameModules;
using ApiServer.Services;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
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
        return await mailModule.GetMailListAsync();
    }
}