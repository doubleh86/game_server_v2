using ApiServer.GameService.Models;
using DataTableLoader.Models;
using DataTableLoader.Utils.Helper;
using DbContext.Common.Models;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using DbContext.MainDbContext.SubContexts;
using NetworkProtocols.WebApi.ToClientModels;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.GameService.GameModules;

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

    public async Task<GameUserDbModel> CreateGameUserAsync(List<AssetDbResult> defaultAssets)
    {
        var result = await GetDbContext().CreateNewGameUser(AccountId, defaultAssets);
        return result;
    }

    private void _AddPlayerExp(GameUserDbModel gameUser, int addExp)
    {
        var levelTableList = DataHelper.GetDataList<PlayerLevelTable>().OrderBy(table => table.level);
        gameUser.user_exp += addExp;

        foreach (var levelTable in levelTableList)
        {
            if (levelTable.level < gameUser.user_level)
                continue;

            if (levelTable.accumulated_exp < gameUser.user_exp)
            {
                gameUser.user_level = levelTable.level;
            }
        }
    }

    public async Task<bool> AddPlayerExpAsync(int addExp)
    {
        var gameUser = await GetGameUserDbModelAsync();
        _AddPlayerExp(gameUser, addExp);
        
        var dbResult = await GetDbContext().ChangePlayerExpAndLevelAsync(gameUser.account_id, gameUser.user_exp, gameUser.user_level);
        return dbResult;
    }

    public async Task<GameUserDbModel> AddPlayerExpUseItemAsync(int addExp, List<InventoryDbResult> itemInfo)
    {
        var gameUser = await GetGameUserDbModelAsync();
        _AddPlayerExp(gameUser, addExp);
        
        await GetDbContext().ChangePlayerExpAndLevelUseItemAsync(gameUser.account_id, gameUser.user_exp, gameUser.user_level, itemInfo);
        return gameUser;
    }
    
    
}