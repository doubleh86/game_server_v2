using DbContext.Common;
using DbContext.SharedContext.DbResultModel;
using MySqlConnector;
using NetworkProtocols.WebApi;
using ServerFramework.CommonUtils.RDBUtils;
using ServerFramework.MySqlServices.MySqlCommandModel;
using ServerFramework.MySqlServices.MySqlDapperUtils;

namespace DbContext.SharedContext.MySqlContext.QueryCommands;

public class GetAccountInfoCommand(MySqlDapperServiceBase dbContext, MySqlTransaction transaction = null) 
    : QueryCommandBaseAsync<GetAccountDbResult, GetAccountDbResult>(dbContext, transaction)
{
    private const string _GetAccountLoginID = "SELECT A.account_id AS AccountId, ISNULL(A.login_id, '') AS LoginId, " +
                                              "ISNULL(A.account_type, 0) AS AccountType," +
                                              "ISNULL(A.game_db_uid, -1) AS GameDbUID, " +
                                              "ISNULL(B.db_name, '') AS DbName, " +
                                              "ISNULL(B.host, '') AS Host, " +
                                              "ISNULL(B.port, 0) AS Port, " +
                                              "ISNULL(B.db_login_id, '') AS DbLoginId, " +
                                              "ISNULL(B.db_password, '') AS DbPassword, " +
                                              "ISNULL(B.slave_db_name, '') AS SlaveDbName, " +
                                              "ISNULL(B.slave_host, '') AS SlaveHost, " +
                                              "ISNULL(B.slave_port, 0) AS SlavePort, " +
                                              "ISNULL(B.slave_db_login_id, '') AS SlaveDbLoginId, " +
                                              "ISNULL(B.slave_db_password, '') AS SlaveDbPassword, " +
                                              "A.update_date as UpdateDate, A.create_date as CreateDate " +
                                              "FROM dbo.account_info AS A LEFT JOIN " +
                                              "dbo.game_db_info AS B ON A.game_db_uid = B.uid " +
                                              "WHERE A.login_id = @loginId;";
    public struct InParameters : IDbInParameters
    {
        public string LoginId  { get; init; }
    }
    
    public override async Task<GetAccountDbResult> ExecuteQueryAsync(IDbInParameters inParameters)
    {
        if (inParameters is not InParameters inParams)
            throw new DbContextException(DbErrorCode.InParameterWrongType, $"[{GetType().Name}] Parameter Type is wrong");

        var result =  await _RunQueryReturnModelAsync(_GetAccountLoginID, 
            new { inParams.LoginId });
        
        return result.FirstOrDefault();
    }
}