using ApiServer.Handlers.Models;
using DbContext.Common.Models;
using DbContext.MainDbContext;
using DbContext.MainDbContext.DbResultModel;
using DbContext.MainDbContext.SubContexts;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.Handlers.GameModules;

public class GameUserModule : BaseModule<GameUserDbContext>, IGameModule
{
    public long AccountId { get; set; }
    private GameUserDbModel _gameUserDbModel;
    public GameUserModule(long accountId, SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo) : base(masterDbInfo, slaveDbInfo)
    {
        AccountId = accountId;
    }

    public async Task<GameUserDbModel> GetGameUserDbModelAsync()
    {
        if(_gameUserDbModel != null)
            return _gameUserDbModel;
        
        _gameUserDbModel = await GetDbContext(true).GetUserInfoAsync(AccountId);
        return _gameUserDbModel;
    }

    
}