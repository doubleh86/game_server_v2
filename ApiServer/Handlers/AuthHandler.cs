using ApiServer.Services;
using ApiServer.Utils;
using DbContext.SharedContext.DbResultModel;
using NetworkProtocols.WebApi;

namespace ApiServer.Handlers;

public class AuthHandler(ApiServerService serverService) : BaseHandler(serverService)
{
    public async Task<GetAccountDbResult> GetAccountInfoAsync(string loginId)
    {
        var dbContext = _GetSharedDbContext();
        var result = await dbContext.GetAccountInfoAsync(loginId);
        if (result != null)
            return result;

        return await _CreateAccountAsync(loginId);
    }

    private async Task<GetAccountDbResult> _CreateAccountAsync(string loginId)
    {
        var dbContext = _GetSharedDbContext();
        var result = await dbContext.CreateAccountAsync(loginId);
        if(result != 0)
            throw new ApiServerException(ResultCode.GameError, "Create account failed");
        
        return await dbContext.GetAccountInfoAsync(loginId);
    }

}