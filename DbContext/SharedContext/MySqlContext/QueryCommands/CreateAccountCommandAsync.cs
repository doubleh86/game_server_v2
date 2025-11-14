using DbContext.Common;
using MySqlConnector;
using NetworkProtocols.WebApi;
using ServerFramework.CommonUtils.RDBUtils;
using ServerFramework.MySqlServices.MySqlCommandModel;
using ServerFramework.MySqlServices.MySqlDapperUtils;

namespace DbContext.SharedContext.MySqlContext.QueryCommands;

public class CreateAccountCommandAsync(MySqlDapperServiceBase dbContext, MySqlTransaction transaction = null) 
    : QueryCommandBaseAsync<int, int>(dbContext, transaction)
{
    private const string _GetGameDbUID = "SELECT uid FROM game_db_info WHERE is_use = 1 ORDER BY current_user_count LIMIT 1;";
    private const string _InsertAccountInfo = "INSERT INTO account_info (login_id, game_db_uid) VALUES (@loginId, @gameDbUID);";
    public struct InParameters : IDbInParameters
    {
        public string LoginId { get; init; }
    }
    
    public override async Task<int> ExecuteQueryAsync(IDbInParameters inParameters)
    {
        if (inParameters is not InParameters inParams)
            throw new DbContextException(DbErrorCode.InParameterWrongType, $"[{GetType().Name}] Parameter Type is wrong");
        
        var gameDbUID = await _RunQueryReturnDynamicAsync(_GetGameDbUID);
        var result = await _RunQueryReturnModelAsync(_InsertAccountInfo, new { loginId = inParams.LoginId, gameDbUID = gameDbUID.FirstOrDefault() });

        return result.FirstOrDefault();
    }
}