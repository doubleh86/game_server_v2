using ApiServer.Handlers.Models;
using DbContext.Common.Models;
using DbContext.MainDbContext;
using DbContext.MainDbContext.DbResultModel;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.Handlers.GameModules;

public class GameUserModule : BaseModule<MainDbContext>, IGameModule
{
    private GameUserDbModel _gameUserDbModel;
    public GameUserModule(SqlServerDbInfo masterDbInfo, SqlServerDbInfo slaveDbInfo) : base(masterDbInfo, slaveDbInfo)
    {
    }

    public async Task<GameUserDbModel> GetGameUserDbModel(long accountId)
    {
        if(_gameUserDbModel != null)
            return _gameUserDbModel;
        
        _gameUserDbModel = await GetDbContext(true).GetUserInfoAsync(accountId);
        return _gameUserDbModel;
    }
}