using MySqlConnector;
using ServerFramework.CommonUtils.RDBUtils;
using ServerFramework.MySqlServices.MySqlDapperUtils;

namespace ServerFramework.MySqlServices.MySqlCommandModel;

public abstract class QueryCommandBaseAsync<TResult, TDbModel>(MySqlDapperServiceBase dbContext, MySqlTransaction transaction = null)
{
    public abstract Task<TResult> ExecuteQueryAsync(IDbInParameters inParameters);
    
    protected async Task<IEnumerable<TDbModel>> _RunQueryReturnModelAsync(string sql, object parameters = null)
    {
        return await dbContext.ExecuteQueryWithModelAsync<TDbModel>(sql, parameters, transaction);
    }
    
    protected async Task<IEnumerable<dynamic>> _RunQueryReturnDynamicAsync(string sql, object parameters = null)
    {
        return await dbContext.ExecuteQueryDynamicAsync(sql, parameters, transaction);
    }
}