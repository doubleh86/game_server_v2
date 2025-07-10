using ApiServer.Handlers.Models;
using ApiServer.Services;
using DbContext.MainDbContext;
using DbContext.MainDbContext.DbResultModel;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.Handlers;

public class UserHandler : BaseHandler, IUseSlaveDbContext<MainDbContext>
{
    public List<MainDbContext> DBContextList { get; set; }
    
    public MainDbContext MasterDbInfo { get; set; }
    public MainDbContext SlaveDbInfo { get; set; }
    
    public UserHandler(ApiServerService service, SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo) : base(service)
    {
        InitializedDbContexts(masterDbInfo, slaveDbInfo);
    }

    public void InitializedDbContexts(SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo)
    {
        MasterDbInfo = new MainDbContext(masterDbInfo);
        if (slaveDbInfo != null)
        {
            SlaveDbInfo = new MainDbContext(slaveDbInfo);
        }
    }

    public MainDbContext GetDbContext(bool isSlave)
    {
        if (isSlave == true && SlaveDbInfo != null)
            return SlaveDbInfo;
        
        return MasterDbInfo;
    }
    
    public async Task<GameUserDbModel> GetUserInfoAsync(long accountId)
    {
        var result = await GetDbContext(true).GetUserInfoAsync(accountId);
        if(result == null)
            return await GetDbContext(false).CreateNewGameUser(accountId);
        
        return result;
    }
}