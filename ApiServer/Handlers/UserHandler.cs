using ApiServer.Handlers.GameModules;
using ApiServer.Handlers.Models;
using ApiServer.Services;
using DbContext.MainDbContext;
using DbContext.MainDbContext.DbResultModel;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.Handlers;

public class UserHandler : BaseHandler, IUseSlaveDbContext<MainDbContext>
{
    private readonly long _accountId;
    public List<MainDbContext> DBContextList { get; set; }
    
    public MainDbContext MasterDbInfo { get; set; }
    public MainDbContext SlaveDbInfo { get; set; }

    public UserHandler(long accountId, ApiServerService service, SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo) : base(service)
    {
        _accountId = accountId;
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

    public override void InitializeModules(SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo)
    {
        var inventoryModule = new InventoryModule(masterDbInfo, slaveDbInfo);
        _AddModule(nameof(InventoryModule), inventoryModule);
    }
    
    public MainDbContext GetDbContext(bool isSlave)
    {
        if (isSlave == true && SlaveDbInfo != null)
            return SlaveDbInfo;
        
        return MasterDbInfo;
    }
    
    public async Task<GameUserDbModel> GetUserInfoAsync()
    {
        var result = await GetDbContext(true).GetUserInfoAsync(_accountId);
        if(result == null)
            return await GetDbContext(false).CreateNewGameUser(_accountId);
        
        var inventoryModule = GetModule<InventoryModule>();
        var list = await inventoryModule.GetInventoryListAsync(_accountId);
        
        return result;
    }


    
}