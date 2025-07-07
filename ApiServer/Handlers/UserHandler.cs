using ApiServer.Services;
using DbContext.MainDbContext;
using DbContext.MainDbContext.DbResultModel;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.Handlers;

public class UserHandler : BaseHandler
{
    private readonly MainDbContext _mainDbContext;
    public UserHandler(ApiServerService service, SqlServerDbInfo serverDbInfo) : base(service)
    {
        _mainDbContext = new MainDbContext(serverDbInfo);
    }
    
    public async Task<GameUserDbModel> GetUserInfoAsync(long accountId)
    {
        var result = await _mainDbContext.GetUserInfoAsync(accountId);
        if(result == null)
            return await _mainDbContext.CreateNewGameUser(accountId);
        
        return result;
    }
}